using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GorevTakip.Models;

public class Gorev
{
    public long GorevId { get; set; }

    // Yabancı anahtar. "Kategori" navigasyonuyla birlikte
    // olduğu için EF ilişkiyi kendiliğinden anlar.
    [Required(ErrorMessage = "Kategori seçmelisiniz.")]
    [Display(Name = "Kategori")]
    public long KategoriId { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(200, ErrorMessage = "En fazla 200 karakter.")]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = "";

    // NULL olabilen uzun metin.
    // [StringLength] YAZMIYORUZ → EF bunu NVARCHAR(MAX) yapar.
    // Uzun açıklamalar için bilinçli tercih.
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    // ⭐ enum — int olarak saklanacak
    [Required]
    [Display(Name = "Öncelik")]
    public Oncelik Oncelik { get; set; } = Oncelik.Orta;

    // ⭐ enum — metin olarak saklanacak (DbContext'te ayarlanıyor)
    [Required]
    [Display(Name = "Durum")]
    public GorevDurum Durum { get; set; } = GorevDurum.Beklemede;

    // ⭐ Formda BOŞ BIRAKILABİLEN tarih
    //
    //    ADO.NET'te iki yerde uğraşıyorduk:
    //      1) Okurken:  okuyucu.IsDBNull(...) ? null : okuyucu.GetDateTime(...)
    //      2) Yazarken: (object?)gorev.BitisTarihi ?? DBNull.Value
    //
    //    EF'te sadece "?" yeterli.
    [DataType(DataType.Date)]
    [Display(Name = "Bitiş tarihi")]
    public DateTime? BitisTarihi { get; set; }

    // Görev tamamlanınca dolar, geri alınınca NULL olur
    [Display(Name = "Tamamlanma tarihi")]
    public DateTime? TamamlanmaTarihi { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool AktifMi { get; set; } = true;

    // ── Navigasyon ───────────────────────────────────────────
    // ⚠️ Sondaki ? ŞART! Form gönderildiğinde bu nesne gelmez
    //    (sadece KategoriId gelir). ? olmazsa ModelState geçersiz olur.
    [Display(Name = "Kategori")]
    public Kategori? Kategori { get; set; }


    // ══════════════════════════════════════════════════════════
    //  HESAPLANAN ÖZELLİKLER
    //
    //  ⚠️⚠️ HEPSİ [NotMapped] OLMALI!
    //     Olmazsa EF bunları sütun sanar ve tabloya eklemeye çalışır.
    //
    //  ⚠️⚠️ VE ÇOK ÖNEMLİ:
    //     Bunları .Where() veya .OrderBy() İÇİNDE KULLANAMAZSINIZ.
    //     EF onları SQL'e çeviremez.
    //
    //     ❌ _db.Gorevler.Where(g => g.GecikmisMi)
    //        → "The LINQ expression could not be translated"
    //
    //     ✅ View içinde, ToListAsync()'ten SONRA kullanılabilir.
    //
    //     Sebebini ve çözümünü Modül 4 ve 5'te göreceğiz.
    // ══════════════════════════════════════════════════════════

    /// <summary>Görev tamamlandı mı?</summary>
    [NotMapped]
    public bool TamamlandiMi => Durum == GorevDurum.Tamamlandi;

    /// <summary>
    /// Görev gecikti mi? Üç koşulun HEPSİ gerekli:
    ///   1. Bitiş tarihi var mı?   (yoksa gecikemez)
    ///   2. O tarih geçmiş mi?
    ///   3. Hâlâ tamamlanmamış mı? (tamamlandıysa gecikme sayılmaz)
    /// </summary>
    [NotMapped]
    public bool GecikmisMi =>
        BitisTarihi.HasValue
        && BitisTarihi.Value.Date < DateTime.Today
        && !TamamlandiMi;

    /// <summary>Bugün bitmesi gerekiyor mu?</summary>
    [NotMapped]
    public bool BugunMu =>
        BitisTarihi.HasValue
        && BitisTarihi.Value.Date == DateTime.Today
        && !TamamlandiMi;

    /// <summary>Bitişe kaç gün kaldı? Tarih yoksa null. Negatif = gecikmiş.</summary>
    [NotMapped]
    public int? KalanGun =>
        BitisTarihi.HasValue
            ? (BitisTarihi.Value.Date - DateTime.Today).Days
            : null;

    /// <summary>Önceliğin ekranda görünecek adı.</summary>
    [NotMapped]
    public string OncelikAdi => Oncelik switch
    {
        Oncelik.Dusuk => "Düşük",
        Oncelik.Orta => "Orta",
        Oncelik.Yuksek => "Yüksek",
        _ => "Bilinmiyor"
    };

    /// <summary>Önceliğin Bootstrap renk sınıfı.</summary>
    [NotMapped]
    public string OncelikRenk => Oncelik switch
    {
        Oncelik.Dusuk => "secondary",
        Oncelik.Orta => "warning",
        Oncelik.Yuksek => "danger",
        _ => "light"
    };

    /// <summary>Durumun ekranda görünecek adı.</summary>
    [NotMapped]
    public string DurumAdi => Durum switch
    {
        GorevDurum.Beklemede => "Beklemede",
        GorevDurum.DevamEdiyor => "Devam ediyor",
        GorevDurum.Tamamlandi => "Tamamlandı",
        _ => "Bilinmiyor"
    };

    /// <summary>Durumun Bootstrap renk sınıfı.</summary>
    [NotMapped]
    public string DurumRenk => Durum switch
    {
        GorevDurum.Beklemede => "secondary",
        GorevDurum.DevamEdiyor => "info",
        GorevDurum.Tamamlandi => "success",
        _ => "light"
    };
}