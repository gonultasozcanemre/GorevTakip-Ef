using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GorevTakip.Data;
using GorevTakip.Models;

namespace GorevTakip.Controllers;

public class GorevController : Controller
{
    private readonly GorevDbContext _db;

    public GorevController(GorevDbContext db)
    {
        _db = db;
    }

    // ══════════════════════════════════════════════════════
    //  YARDIMCI: kategori açılır listesi
    //
    //  ⭐ Select ile sadece iki alan çekiyoruz — tüm kategori
    //     nesnesine gerek yok.
    // ══════════════════════════════════════════════════════
    private async Task KategoriListesiniHazirlaAsync(long? secili = null)
    {
        var kategoriler = await _db.Kategoriler
            .AsNoTracking()
            .OrderBy(k => k.KategoriAd)
            .Select(k => new { k.KategoriId, k.KategoriAd })
            .ToListAsync();

        ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriId", "KategoriAd", secili);
    }

    // ══════════════════════════════════════════════════════
    //  1) LİSTELEME
    //  GET: /Gorev
    //
    //  (Filtreleme Modül 5'te eklenecek)
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Index()
    {
        var gorevler = await _db.Gorevler
            .AsNoTracking()
            .Include(g => g.Kategori)                        // JOIN
                                                             // ⭐ KOŞULLU SIRALAMA — CASE WHEN'in LINQ karşılığı
            .OrderBy(g => g.Durum == GorevDurum.Tamamlandi)  // tamamlananlar alta
            .ThenByDescending(g => g.Oncelik)                // yüksek öncelik üste
            .ThenBy(g => g.BitisTarihi == null)              // tarihsizler alta
            .ThenBy(g => g.BitisTarihi)                      // yakın tarih üste
            .ToListAsync();

        return View(gorevler);
    }

    // ══════════════════════════════════════════════════════
    //  2) YENİ KAYIT FORMU
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Create()
    {
        await KategoriListesiniHazirlaAsync();
        return View();
    }

    // ══════════════════════════════════════════════════════
    //  3) YENİ KAYDI KAYDET
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Gorev gorev)
    {
        // Bitiş tarihi geçmişte olmasın (isteğe bağlı kural)
        if (gorev.BitisTarihi.HasValue && gorev.BitisTarihi.Value.Date < DateTime.Today)
            ModelState.AddModelError(nameof(Gorev.BitisTarihi),
                "Bitiş tarihi geçmiş bir gün olamaz.");

        if (!ModelState.IsValid)
        {
            // ⭐ EN ÇOK UNUTULAN SATIR
            // ViewBag sadece o istek boyunca yaşar. POST yeni bir istektir.
            // Doldurmazsak açılır liste boş gelir ve sayfa çöker.
            await KategoriListesiniHazirlaAsync(gorev.KategoriId);
            return View(gorev);
        }

        gorev.CreatedDate = DateTime.Now;
        gorev.UpdatedDate = null;
        gorev.AktifMi = true;

        // ⭐ DURUM İLE TAMAMLANMA TARİHİNİ BİRLİKTE YÖNET
        //    Kullanıcı görevi doğrudan "Tamamlandı" olarak eklerse
        //    tamamlanma tarihi de dolmalı. İkisi hep tutarlı kalmalı.
        gorev.TamamlanmaTarihi = (gorev.Durum == GorevDurum.Tamamlandi)
            ? DateTime.Now
            : null;

        // ⚠️ gorev.Kategori navigasyon özelliği NULL — bu SORUN DEĞİL.
        //    EF, KategoriId alanına bakar. Kategori nesnesini de
        //    doldursaydık EF onu YENİ BİR KATEGORİ sanıp eklemeye çalışabilirdi.
        _db.Gorevler.Add(gorev);
        await _db.SaveChangesAsync();

        TempData["Basarili"] = $"\"{gorev.Baslik}\" görevi eklendi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  4) DÜZENLEME FORMU
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Edit(long id)
    {
        var gorev = await _db.Gorevler.FindAsync(id);

        if (gorev == null)
            return NotFound();

        await KategoriListesiniHazirlaAsync(gorev.KategoriId);
        return View(gorev);
    }

    // ══════════════════════════════════════════════════════
    //  5) DÜZENLEMEYİ KAYDET
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Gorev gorev)
    {
        if (!ModelState.IsValid)
        {
            await KategoriListesiniHazirlaAsync(gorev.KategoriId);
            return View(gorev);
        }

        var mevcut = await _db.Gorevler.FindAsync(gorev.GorevId);

        if (mevcut == null)
            return NotFound();

        // ⭐ Durum DEĞİŞTİ mi? Tamamlanma tarihini ona göre ayarlayacağız.
        bool oncedenTamamlanmisti = mevcut.Durum == GorevDurum.Tamamlandi;
        bool simdiTamamlandi = gorev.Durum == GorevDurum.Tamamlandi;

        mevcut.KategoriId = gorev.KategoriId;
        mevcut.Baslik = gorev.Baslik;
        mevcut.Aciklama = gorev.Aciklama;
        mevcut.Oncelik = gorev.Oncelik;
        mevcut.Durum = gorev.Durum;
        mevcut.BitisTarihi = gorev.BitisTarihi;
        mevcut.UpdatedDate = DateTime.Now;

        // ⭐ TAMAMLANMA TARİHİ MANTIĞI
        if (simdiTamamlandi && !oncedenTamamlanmisti)
        {
            // Yeni tamamlandı → tarihi şimdi yaz
            mevcut.TamamlanmaTarihi = DateTime.Now;
        }
        else if (!simdiTamamlandi)
        {
            // Geri alındı veya hiç tamamlanmadı → tarihi temizle
            mevcut.TamamlanmaTarihi = null;
        }
        // Zaten tamamlanmıştı ve hâlâ tamamlanmış → eski tarihi KORU
        // (else bloğu yok — dokunmuyoruz)

        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Görev güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  6) SİLME ONAY SAYFASI
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Delete(long id)
    {
        var gorev = await _db.Gorevler
            .AsNoTracking()
            .Include(g => g.Kategori)
            .FirstOrDefaultAsync(g => g.GorevId == id);

        if (gorev == null)
            return NotFound();

        return View(gorev);
    }

    // ══════════════════════════════════════════════════════
    //  7) SİLMEYİ ONAYLA — soft delete
    // ══════════════════════════════════════════════════════
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var gorev = await _db.Gorevler.FindAsync(id);

        if (gorev == null)
            return NotFound();

        gorev.AktifMi = false;
        gorev.UpdatedDate = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Görev silindi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  8) DETAY
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Details(long id)
    {
        var gorev = await _db.Gorevler
            .AsNoTracking()
            .Include(g => g.Kategori)
            .FirstOrDefaultAsync(g => g.GorevId == id);

        if (gorev == null)
            return NotFound();

        return View(gorev);
    }
    // ══════════════════════════════════════════════════════
    //  8) PASİF KAYITLAR
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Pasifler()
    {
        var pasifler = await _db.Gorevler
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(g => g.Kategori)                // ⭐ soft delete filtresini atla
            .Where(g => !g.AktifMi)
            .OrderBy(g => g.Baslik)
            .ToListAsync();

        return View(pasifler);
    }
    // ══════════════════════════════════════════════════════
    //  10) GÖREVİ GERİ YÜKLE
    //  POST: /Gorev/GeriYukle/5
    //
    //  ⚠️ Adı "GeriAl" DEĞİL — Modül 5'te durum geri alma için
    //     o adı kullanacağız. Sebebi yukarıda açıklandı.
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeriYukle(long id)
    {
        // ⚠️⚠️ IgnoreQueryFilters ŞART!
        //    FindAsync(id) yazsaydık query filter devreye girer,
        //    pasif kaydı BULAMAZDI ve hep NotFound() dönerdi.
        var gorev = await _db.Gorevler
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.GorevId == id);

        if (gorev == null)
            return NotFound();

        // ⭐ Zaten aktifse boşuna işlem yapma
        if (gorev.AktifMi)
        {
            TempData["Uyari"] = "Bu görev zaten aktif.";
            return RedirectToAction(nameof(Index));
        }

        // ⭐ Kategorisi silinmişse geri yükleme anlamsız olur —
        //    görev listede görünmez çünkü Include, kategorinin
        //    query filter'ını da uygular ve satır elenir.
        bool kategoriAktif = await _db.Kategoriler
            .AnyAsync(k => k.KategoriId == gorev.KategoriId);

        if (!kategoriAktif)
        {
            TempData["Uyari"] = "Bu görevin kategorisi silinmiş. " +
                                "Önce kategoriyi geri getirmelisiniz.";
            return RedirectToAction(nameof(Pasifler));
        }

        gorev.AktifMi = true;
        gorev.UpdatedDate = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Görev geri getirildi.";
        return RedirectToAction(nameof(Pasifler));
    }
    // ══════════════════════════════════════════════════════
    //  HIZLI EYLEM: TAMAMLA
    //  POST: /Gorev/Tamamla/5
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Tamamla(long id, string? donusUrl = null)
    {
        // ⭐ TEK SORGU — nesneyi çekmiyoruz bile.
        //    Dönüş değeri: kaç satır etkilendi?
        int etkilenen = await _db.Gorevler
            .Where(g => g.GorevId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(g => g.Durum, GorevDurum.Tamamlandi)
                .SetProperty(g => g.TamamlanmaTarihi, DateTime.Now)
                .SetProperty(g => g.UpdatedDate, DateTime.Now));

        if (etkilenen == 0)
            return NotFound();

        TempData["Basarili"] = "Görev tamamlandı.";
        return GeriDon(donusUrl);
    }

    // ══════════════════════════════════════════════════════
    //  HIZLI EYLEM: GERİ AL
    //  POST: /Gorev/GeriAl/5
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeriAl(long id, string? donusUrl = null)
    {
        int etkilenen = await _db.Gorevler
            .Where(g => g.GorevId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(g => g.Durum, GorevDurum.Beklemede)
                // ⭐ DİKKAT: null atarken tip belirtmek gerekir.
                //    Sadece "null" yazsaydık derleyici hangi tip
                //    olduğunu anlayamazdı.
                .SetProperty(g => g.TamamlanmaTarihi, (DateTime?)null)
                .SetProperty(g => g.UpdatedDate, DateTime.Now));

        if (etkilenen == 0)
            return NotFound();

        TempData["Basarili"] = "Görev yeniden açıldı.";
        return GeriDon(donusUrl);
    }

    // ══════════════════════════════════════════════════════
    //  HIZLI EYLEM: BAŞLAT (devam ediyor yap)
    //  POST: /Gorev/Baslat/5
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Baslat(long id, string? donusUrl = null)
    {
        int etkilenen = await _db.Gorevler
            .Where(g => g.GorevId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(g => g.Durum, GorevDurum.DevamEdiyor)
                .SetProperty(g => g.TamamlanmaTarihi, (DateTime?)null)
                .SetProperty(g => g.UpdatedDate, DateTime.Now));

        if (etkilenen == 0)
            return NotFound();

        TempData["Basarili"] = "Görev başlatıldı.";
        return GeriDon(donusUrl);
    }

    // ══════════════════════════════════════════════════════
    //  YARDIMCI: filtreli listeye geri dön
    //
    //  ⚠️ Url.IsLocalUrl kontrolü ŞART!
    //     Kullanıcıdan gelen bir adrese yönlendirirken HER ZAMAN
    //     bu kontrol yapılır — yoksa açık yönlendirme açığı olur.
    //     (Modül 2'de giriş sonrası yönlendirmede de görmüştük.)
    // ══════════════════════════════════════════════════════
    private IActionResult GeriDon(string? donusUrl)
    {
        if (!string.IsNullOrEmpty(donusUrl) && Url.IsLocalUrl(donusUrl))
            return Redirect(donusUrl);

        return RedirectToAction(nameof(Index));
    }


}