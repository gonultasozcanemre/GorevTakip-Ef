namespace GorevTakip.Models;

/// <summary>
/// Dashboard ekranının taşıyıcısı.
///
/// Model     = bir TABLONUN karşılığı  (Gorev, Kategori)
/// ViewModel = bir EKRANIN ihtiyacı    (bu sınıf)
///
/// Bu sınıfın DbSet'i YOK, tablosu YOK.
/// </summary>
public class DashboardViewModel
{
    // Üst kartlar
    public int ToplamGorev { get; set; }
    public int TamamlananGorev { get; set; }
    public int BekleyenGorev { get; set; }
    public int DevamEdenGorev { get; set; }
    public int GecikmisGorev { get; set; }
    public int BugunBitenGorev { get; set; }

    // = new()  →  boş başlasın, null olmasın (view'da çökmesin)
    public List<KategoriDagilim> KategoriDagilimlari { get; set; } = new();
    public List<YaklasanGorev> YaklasanGorevler { get; set; } = new();

    /// <summary>
    /// Tamamlanma yüzdesi.
    /// ⚠️ Sıfıra bölme koruması ŞART — hiç görev yoksa çökerdi.
    /// </summary>
    public int TamamlanmaYuzdesi =>
        ToplamGorev > 0 ? (TamamlananGorev * 100) / ToplamGorev : 0;
}

public class KategoriDagilim
{
    public string KategoriAd { get; set; } = "";
    public string Renk { get; set; } = "secondary";
    public int ToplamGorev { get; set; }
    public int TamamlananGorev { get; set; }

    public int Yuzde =>
        ToplamGorev > 0 ? (TamamlananGorev * 100) / ToplamGorev : 0;
}

public class YaklasanGorev
{
    public long GorevId { get; set; }
    public string Baslik { get; set; } = "";
    public string KategoriAd { get; set; } = "";
    public string KategoriRenk { get; set; } = "secondary";
    public DateTime BitisTarihi { get; set; }
    public Oncelik Oncelik { get; set; }

    // ⭐ Bu ViewModel'de hesaplanan özellik SERBEST.
    //    Çünkü bu sınıf hiç veritabanına gitmiyor —
    //    EF onu SQL'e çevirmeye çalışmıyor.
    public bool GecikmisMi => BitisTarihi.Date < DateTime.Today;

    public int KalanGun => (BitisTarihi.Date - DateTime.Today).Days;
}