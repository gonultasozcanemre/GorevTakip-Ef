using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GorevTakip.Data;
using GorevTakip.Models;

namespace GorevTakip.Controllers;

// ⭐ [AllowAnonymous] ŞART!
//    Program.cs'te tüm uygulamayı "giriş zorunlu" yapacağız.
//    Bu controller muaf olmazsa, giriş sayfasına girmek için
//    giriş yapmak gerekir → SONSUZ DÖNGÜ.
[AllowAnonymous]
public class HesapController : Controller
{
    private readonly GorevDbContext _db;

    public HesapController(GorevDbContext db)
    {
        _db = db;
    }

    // GET: /Hesap/Giris
    [HttpGet]
    public IActionResult Giris(string? donusUrl = null)
    {
        // Zaten girmişse formu gösterme
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        ViewBag.DonusUrl = donusUrl;
        return View();
    }

    // POST: /Hesap/Giris
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisViewModel model, string? donusUrl = null)
    {
        ViewBag.DonusUrl = donusUrl;

        if (!ModelState.IsValid)
            return View(model);

        // ⭐ Karşılaştırmayı VERİTABANINDA yapıyoruz:
        //    "bu kullanıcı adı VE bu hash'e sahip satır var mı?"
        //
        //    ⚠️ SQL injection endişesi YOK — EF her değeri otomatik
        //       parametreye çevirir. Şifre alanına ' OR '1'='1
        //       yazsalar bile sadece metin olarak aranır.
        string hash = SifreYardimcisi.Hashle(model.Sifre);

        Kullanici? kullanici = await _db.Kullanicilar
            .FirstOrDefaultAsync(k => k.KullaniciAdi == model.KullaniciAdi
                                   && k.SifreHash == hash);

        if (kullanici == null)
        {
            // ⚠️⚠️ "Kullanıcı yok" ile "şifre yanlış"ı AYIRMA!
            //    Ayrı söyleseydik saldırgan, kayıtlı kullanıcı adlarını
            //    tek tek deneyerek öğrenirdi (user enumeration).
            //    Belirsizlik KASITLIDIR.
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        // Claim = kullanıcı hakkında bir bilgi parçası.
        // Çereze şifrelenerek yazılır, her istekte sunucuya gelir.
        var iddialar = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()),
            new Claim(ClaimTypes.Name, kullanici.AdSoyad),
            new Claim("KullaniciAdi", kullanici.KullaniciAdi)
        };

        var kimlik = new ClaimsIdentity(iddialar,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var ozellikler = new AuthenticationProperties
        {
            IsPersistent = model.BeniHatirla,   // tarayıcı kapansa da yaşasın mı?
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(kimlik),
            ozellikler);

        // ⚠️ Url.IsLocalUrl kontrolü ŞART!
        //    Olmasaydı saldırgan şöyle bir bağlantı hazırlayabilirdi:
        //      /Hesap/Giris?donusUrl=https://sahte-site.com
        //    Kullanıcı giriş sonrası sahte siteye giderdi (open redirect).
        if (!string.IsNullOrEmpty(donusUrl) && Url.IsLocalUrl(donusUrl))
            return Redirect(donusUrl);

        return RedirectToAction("Index", "Home");
    }

    // POST: /Hesap/Cikis
    //
    // ⚠️ Neden POST? Çıkış durumu DEĞİŞTİREN bir işlem.
    //    GET olsaydı kötü niyetli bir sitedeki
    //        <img src=".../Hesap/Cikis">
    //    etiketi kullanıcıyı habersizce çıkartırdı.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Bilgi"] = "Oturumunuz kapatıldı.";
        return RedirectToAction(nameof(Giris));
    }
}