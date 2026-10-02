using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GorevTakip.Data;
using GorevTakip.Models;

namespace GorevTakip.Controllers;

public class HomeController : Controller
{
    private readonly GorevDbContext _db;

    public HomeController(GorevDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var bugun = DateTime.Today;
        var model = new DashboardViewModel();

        // ══════════════════════════════════════════════════
        //  1) ÖZET SAYILAR — TEK SORGU
        //
        //  GroupBy(g => 1) numarası: tüm satırları tek gruba
        //  toplayıp grup üzerinde koşullu sayımlar yapıyoruz.
        //
        //  ⚠️ Hesaplanan özellikler (GecikmisMi) KULLANILAMAZ.
        //     SQL'e çevrilebilir koşullar yazıyoruz.
        // ══════════════════════════════════════════════════
        var ozet = await _db.Gorevler
            .GroupBy(g => 1)
            .Select(grup => new
            {
                Toplam = grup.Count(),
                Tamamlanan = grup.Count(g => g.Durum == GorevDurum.Tamamlandi),
                Bekleyen = grup.Count(g => g.Durum == GorevDurum.Beklemede),
                DevamEden = grup.Count(g => g.Durum == GorevDurum.DevamEdiyor),

                // GecikmisMi özelliğinin SQL'e çevrilebilir hâli
                Gecikmis = grup.Count(g => g.BitisTarihi != null
                                          && g.BitisTarihi < bugun
                                          && g.Durum != GorevDurum.Tamamlandi),

                // BugunMu özelliğinin SQL'e çevrilebilir hâli
                BugunBiten = grup.Count(g => g.BitisTarihi == bugun
                                          && g.Durum != GorevDurum.Tamamlandi)
            })
            .FirstOrDefaultAsync();

        // ⚠️ Hiç görev yoksa GroupBy hiç grup üretmez → ozet NULL olur
        if (ozet != null)
        {
            model.ToplamGorev = ozet.Toplam;
            model.TamamlananGorev = ozet.Tamamlanan;
            model.BekleyenGorev = ozet.Bekleyen;
            model.DevamEdenGorev = ozet.DevamEden;
            model.GecikmisGorev = ozet.Gecikmis;
            model.BugunBitenGorev = ozet.BugunBiten;
        }

        // ══════════════════════════════════════════════════
        //  2) KATEGORİ DAĞILIMI
        //
        //  ⭐ Select içinde alt sorgu: k.Gorevler.Count()
        //     Include'a GEREK YOK — EF gereken SQL'i kendisi üretir.
        // ══════════════════════════════════════════════════
        model.KategoriDagilimlari = await _db.Kategoriler
            .AsNoTracking()
            .Select(k => new KategoriDagilim
            {
                KategoriAd = k.KategoriAd,
                Renk = k.Renk,
                ToplamGorev = k.Gorevler.Count(),
                TamamlananGorev = k.Gorevler.Count(g => g.Durum == GorevDurum.Tamamlandi)
            })
            .OrderByDescending(x => x.ToplamGorev)
            .ThenBy(x => x.KategoriAd)
            .ToListAsync();

        // ══════════════════════════════════════════════════
        //  3) YAKLAŞAN GÖREVLER
        //
        //  ⚠️ SIRA ÖNEMLİ: OrderBy önce, Take sonra.
        //     Ters yazsaydık rastgele 5 kayıt alıp onları sıralardık.
        // ══════════════════════════════════════════════════
        model.YaklasanGorevler = await _db.Gorevler
            .AsNoTracking()
            .Where(g => g.Durum != GorevDurum.Tamamlandi
                     && g.BitisTarihi != null)
            .OrderBy(g => g.BitisTarihi)
            .Take(5)
            .Select(g => new YaklasanGorev
            {
                GorevId = g.GorevId,
                Baslik = g.Baslik,
                KategoriAd = g.Kategori!.KategoriAd,
                KategoriRenk = g.Kategori!.Renk,
                // ⚠️ .Value şart — sorguda IS NOT NULL filtresi var
                //    ama derleyici bunu bilmiyor
                BitisTarihi = g.BitisTarihi!.Value,
                Oncelik = g.Oncelik
            })
            .ToListAsync();

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewBag.HataKodu = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}