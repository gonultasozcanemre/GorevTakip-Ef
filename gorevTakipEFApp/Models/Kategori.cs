using System.ComponentModel.DataAnnotations;

namespace GorevTakip.Models;

public class Kategori
{
    // "<SınıfAdı>Id" → EF bunu birincil anahtar kabul eder
    public long KategoriId { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(100, ErrorMessage = "En fazla 100 karakter olabilir.")]
    [Display(Name = "Kategori adı")]
    public string KategoriAd { get; set; } = "";

    [Required(ErrorMessage = "Renk seçmelisiniz.")]
    [StringLength(20)]
    [Display(Name = "Renk")]
    public string Renk { get; set; } = "primary";   // Bootstrap renk adı

    // ⭐ NULL olabilen metin alanı
    //
    //    ADO.NET'te bu alan için repository'de şunu yazıyorduk:
    //        komut.Parameters.AddWithValue("@aciklama",
    //            (object?)kategori.Aciklama ?? DBNull.Value);
    //
    //    EF'te sadece "?" yazmak yeterli. DBNull köprüsü yok.
    [StringLength(500, ErrorMessage = "En fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Display(Name = "Kayıt tarihi")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Güncelleme tarihi")]
    public DateTime? UpdatedDate { get; set; }

    public bool AktifMi { get; set; } = true;

    // ── Navigasyon ───────────────────────────────────────────
    // Bu kategorideki görevler.
    // ⚠️ Veritabanında sütunu YOK — EF ilişkiden anlar.
    public List<Gorev> Gorevler { get; set; } = new();
}