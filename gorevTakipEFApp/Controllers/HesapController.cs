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

        await OturumAcAsync(kullanici, model.BeniHatirla);

        // ⚠️ Url.IsLocalUrl kontrolü ŞART!
        //    Olmasaydı saldırgan şöyle bir bağlantı hazırlayabilirdi:
        //      /Hesap/Giris?donusUrl=https://sahte-site.com
        //    Kullanıcı giriş sonrası sahte siteye giderdi (open redirect).
        if (!string.IsNullOrEmpty(donusUrl) && Url.IsLocalUrl(donusUrl))
            return Redirect(donusUrl);

        return RedirectToAction("Index", "Home");
    }



    // ══════════════════════════════════════════════════════
    //  YARDIMCI: çerezi oluştur (giriş + kayıt ortak kullanır)
    // ══════════════════════════════════════════════════════
    private async Task OturumAcAsync(Kullanici kullanici, bool beniHatirla)
    {
        // Claim = kullanıcı hakkında bir bilgi parçası.
        // Çereze şifrelenerek yazılır, her istekte sunucuya gelir.
        var iddialar = new List<Claim>
        {
            // ⭐ Bu claim çok önemli: DbContext query filter için
            //    tam olarak bunu okuyor.
            new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()),
            new Claim(ClaimTypes.Name, kullanici.AdSoyad),
            new Claim("KullaniciAdi", kullanici.KullaniciAdi)
        };

        var kimlik = new ClaimsIdentity(iddialar,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var ozellikler = new AuthenticationProperties
        {
            IsPersistent = beniHatirla,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(kimlik),
            ozellikler);
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

    // ══════════════════════════════════════════════════════
    //  KAYIT FORMU
    //  GET: /Hesap/Kayit
    // ══════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult Kayit()
    {
        // Zaten giriş yapmışsa kayıt formunu gösterme
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View();
    }


    // ══════════════════════════════════════════════════════
    //  KAYDI İŞLE
    //  POST: /Hesap/Kayit
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kayit(KayitViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // ── 1) Kullanıcı adı boşta mı? ──────────────────────
        //
        // ⚠️ IgnoreQueryFilters ŞART!
        //    Kullanici tablosunda AktifMi filtresi var.
        //    Pasif bir kullanıcı aynı adı kullanıyorsa onu da
        //    görmemiz gerekir — yoksa benzersiz indeks hata verir
        //    ve kullanıcı sebebini anlayamaz.
        bool adAlinmis = await _db.Kullanicilar
            .IgnoreQueryFilters()
            .AnyAsync(k => k.KullaniciAdi == model.KullaniciAdi);

        if (adAlinmis)
        {
            ModelState.AddModelError(nameof(KayitViewModel.KullaniciAdi),
                "Bu kullanıcı adı zaten alınmış. Başka bir tane deneyin.");
            return View(model);
        }

        // ── 2) Kullanıcıyı oluştur ──────────────────────────
        var kullanici = new Kullanici
        {
            KullaniciAdi = model.KullaniciAdi.Trim(),
            AdSoyad = model.AdSoyad.Trim(),
            SifreHash = SifreYardimcisi.Hashle(model.Sifre),  // ⭐ düz metin DEĞİL
            CreatedDate = DateTime.Now,
            AktifMi = true
        };

        try
        {
            _db.Kullanicilar.Add(kullanici);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // ⭐ YARIŞ KOŞULU AĞI
            //    Yukarıdaki kontrolden sonra, bu satıra gelene kadar
            //    başka biri aynı adı almış olabilir. Benzersiz indeks
            //    bunu engeller ve buraya düşeriz.
            ModelState.AddModelError(nameof(KayitViewModel.KullaniciAdi),
                "Bu kullanıcı adı az önce alındı. Başka bir tane deneyin.");
            return View(model);
        }

        // ⭐ SaveChanges sonrası kullanici.KullaniciId DOLMUŞ olur.
        //    EF, veritabanının ürettiği IDENTITY değerini geri yazar.

        // ── 3) Başlangıç kategorileri ver ───────────────────
        //    Yeni kullanıcı bomboş bir ekranla karşılaşmasın.
        await BaslangicKategorileriEkleAsync(kullanici.KullaniciId);

        // ── 4) Otomatik giriş yaptır ────────────────────────
        //    Kullanıcıyı bir de giriş formuna göndermek gereksiz.
        await OturumAcAsync(kullanici, beniHatirla: false);

        TempData["Basarili"] = $"Hoş geldiniz {kullanici.AdSoyad}! Hesabınız oluşturuldu.";
        return RedirectToAction("Index", "Home");
    }


    // ══════════════════════════════════════════════════════
    //  YARDIMCI: yeni kullanıcıya hazır kategoriler
    // ══════════════════════════════════════════════════════
    private async Task BaslangicKategorileriEkleAsync(long kullaniciId)
    {
        var kategoriler = new List<Kategori>
        {
            new Kategori { KullaniciId = kullaniciId, KategoriAd = "İş",
                           Renk = "primary", Aciklama = "İşle ilgili görevler",
                           CreatedDate = DateTime.Now, AktifMi = true },

            new Kategori { KullaniciId = kullaniciId, KategoriAd = "Kişisel",
                           Renk = "info", Aciklama = null,
                           CreatedDate = DateTime.Now, AktifMi = true },

            new Kategori { KullaniciId = kullaniciId, KategoriAd = "Ev",
                           Renk = "warning", Aciklama = null,
                           CreatedDate = DateTime.Now, AktifMi = true }
        };

        _db.Kategoriler.AddRange(kategoriler);
        await _db.SaveChangesAsync();
    }



}