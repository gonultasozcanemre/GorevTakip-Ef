using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GorevTakip.Data;
using GorevTakip.Models;

namespace GorevTakip.Controllers;

// [Authorize] yazmaya gerek YOK — Program.cs'teki global filtre
// zaten tüm controller'ları koruyor.
public class KategoriController : Controller
{
    private readonly GorevDbContext _db;

    public KategoriController(GorevDbContext db)
    {
        _db = db;
    }

    // ══════════════════════════════════════════════════════
    //  1) LİSTELEME
    //  GET: /Kategori
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Index()
    {
        // ⭐ AsNoTracking: sadece göstereceğiz, güncellemeyeceğiz.
        //    EF'in değişiklik takipçisini boş yere çalıştırmasın.
        //
        // ⭐ Query filter sayesinde .Where(k => k.AktifMi) yazmıyoruz.
        var kategoriler = await _db.Kategoriler
            .AsNoTracking()
            .Include(k => k.Gorevler)        // görev sayısını göstermek için
            .OrderBy(k => k.KategoriAd)
            .ToListAsync();

        return View(kategoriler);
    }

    // ══════════════════════════════════════════════════════
    //  2) YENİ KAYIT FORMU
    // ══════════════════════════════════════════════════════
    public IActionResult Create()
    {
        return View();
    }

    // ══════════════════════════════════════════════════════
    //  3) YENİ KAYDI KAYDET
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Kategori kategori)
    {
        // Aynı adda kategori var mı? (benzersiz indeks var ama
        // önce kontrol edip güzel mesaj veriyoruz)
        bool adVar = await _db.Kategoriler
            .AnyAsync(k => k.KategoriAd == kategori.KategoriAd);

        if (adVar)
            ModelState.AddModelError(nameof(Kategori.KategoriAd),
                "Bu adda bir kategori zaten var.");

        if (!ModelState.IsValid)
            return View(kategori);

        // Sistem alanlarını biz dolduruyoruz — kullanıcıdan almıyoruz
        kategori.CreatedDate = DateTime.Now;
        kategori.UpdatedDate = null;
        kategori.AktifMi = true;

        // ⭐ NULL alan için hiçbir özel işlem yok.
        //    ADO.NET'te şunu yazmak zorundaydık:
        //        (object?)kategori.Aciklama ?? DBNull.Value
        //    EF, string? gördüğü için gerekeni kendisi yapıyor.
        _db.Kategoriler.Add(kategori);
        await _db.SaveChangesAsync();

        TempData["Basarili"] = $"\"{kategori.KategoriAd}\" kategorisi eklendi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  4) DÜZENLEME FORMU
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Edit(long id)
    {
        var kategori = await _db.Kategoriler.FindAsync(id);

        // Kullanıcı adres çubuğuna olmayan bir id yazabilir
        if (kategori == null)
            return NotFound();

        return View(kategori);
    }

    // ══════════════════════════════════════════════════════
    //  5) DÜZENLEMEYİ KAYDET
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Kategori kategori)
    {
        // ⭐ Kendi kaydını çakışma sayma
        bool adVar = await _db.Kategoriler
            .AnyAsync(k => k.KategoriAd == kategori.KategoriAd
                        && k.KategoriId != kategori.KategoriId);

        if (adVar)
            ModelState.AddModelError(nameof(Kategori.KategoriAd),
                "Bu adda başka bir kategori var.");

        if (!ModelState.IsValid)
            return View(kategori);

        // ⭐ ÖNCE VERİTABANINDAN ÇEK, SONRA DEĞİŞTİR
        //
        // Neden doğrudan _db.Update(kategori) yazmıyoruz?
        // Çünkü formda olmayan alanlar (CreatedDate, AktifMi) boş gelir.
        // Update() tüm sütunları yazar ve o alanları SIFIRLAR.
        var mevcut = await _db.Kategoriler.FindAsync(kategori.KategoriId);

        if (mevcut == null)
            return NotFound();

        mevcut.KategoriAd = kategori.KategoriAd;
        mevcut.Renk = kategori.Renk;
        mevcut.Aciklama = kategori.Aciklama;
        mevcut.UpdatedDate = DateTime.Now;

        // ⭐ Add/Update yok! "mevcut" nesnesi zaten takip ediliyor.
        //    EF neyin değiştiğini biliyor ve sadece o sütunları günceller.
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Kategori güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  6) SİLME ONAY SAYFASI
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Delete(long id)
    {
        var kategori = await _db.Kategoriler
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.KategoriId == id);

        if (kategori == null)
            return NotFound();

        // Silinemeyecekse kullanıcıya ÖNCEDEN söyle
        ViewBag.GorevSayisi = await _db.Gorevler
            .CountAsync(g => g.KategoriId == id);

        return View(kategori);
    }

    // ══════════════════════════════════════════════════════
    //  7) SİLMEYİ ONAYLA — soft delete
    // ══════════════════════════════════════════════════════
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var kategori = await _db.Kategoriler.FindAsync(id);

        if (kategori == null)
            return NotFound();

        // ⭐ İlişkili kayıt kontrolü — POST'ta TEKRAR yapılıyor.
        //    GET'te de bakıyoruz ama arada kullanıcı görev eklemiş olabilir.
        //    Asıl karar her zaman veriyi DEĞİŞTİREN tarafta verilir.
        int gorevSayisi = await _db.Gorevler.CountAsync(g => g.KategoriId == id);

        if (gorevSayisi > 0)
        {
            TempData["Uyari"] = $"Bu kategoride {gorevSayisi} görev var. " +
                                 "Önce görevleri silmeli veya başka kategoriye taşımalısınız.";
            return RedirectToAction(nameof(Index));
        }

        // ⭐ SOFT DELETE
        //    _db.Remove(kategori) yazsaydık gerçekten SİLİNİRDİ.
        //    Biz sadece işaretliyoruz; query filter onu listeden gizleyecek.
        kategori.AktifMi = false;
        kategori.UpdatedDate = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Kategori silindi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  8) PASİF KAYITLAR
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> Pasifler()
    {
        var pasifler = await _db.Kategoriler
            .AsNoTracking()
            .IgnoreQueryFilters()            // ⭐ soft delete filtresini atla
            .Where(k => !k.AktifMi)
            .OrderBy(k => k.KategoriAd)
            .ToListAsync();

        return View(pasifler);
    }

    // ══════════════════════════════════════════════════════
    //  9) GERİ GETİR
    // ══════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeriAl(long id)
    {
        // Pasif kaydı bulmak için filtreyi atlamalıyız,
        // yoksa FindAsync bile onu bulamaz
        var kategori = await _db.Kategoriler
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.KategoriId == id);

        if (kategori == null)
            return NotFound();

        kategori.AktifMi = true;
        kategori.UpdatedDate = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Kategori geri getirildi.";
        return RedirectToAction(nameof(Pasifler));
    }
}