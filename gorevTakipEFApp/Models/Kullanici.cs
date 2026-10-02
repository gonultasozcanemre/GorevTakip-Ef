using System.ComponentModel.DataAnnotations;

namespace GorevTakip.Models;

public class Kullanici
{
    public long KullaniciId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Kullanıcı adı")]
    public string KullaniciAdi { get; set; } = "";

    [Required]
    [StringLength(255)]
    public string SifreHash { get; set; } = "";   // şifrenin kendisi değil, özeti

    [Required]
    [StringLength(255)]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = "";

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool AktifMi { get; set; } = true;
    // ── Navigasyon ───────────────────────────────────────────
    // Bu kullanıcının kategorileri ve görevleri
    public List<Kategori> Kategoriler { get; set; } = new();
    public List<Gorev> Gorevler { get; set; } = new();

}


/// <summary>
/// Giriş formunun taşıyıcısı — veritabanı tablosu DEĞİL.
/// Bu yüzden DbContext'te DbSet'i yok.
/// </summary>
public class GirisViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı gerekli.")]
    [Display(Name = "Kullanıcı adı")]
    public string KullaniciAdi { get; set; } = "";

    [Required(ErrorMessage = "Şifre gerekli.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    [Display(Name = "Beni hatırla")]
    public bool BeniHatirla { get; set; }
}
/// <summary>
/// Kayıt formunun taşıyıcısı — veritabanı tablosu DEĞİL.
///
/// ⭐ Neden Kullanici sınıfını doğrudan kullanmıyoruz?
///    Çünkü o sınıfta SifreHash, AktifMi, CreatedDate var.
///    Forma koysaydık kötü niyetli biri F12 ile doğrudan
///    hash gönderebilir veya kendini pasif yapabilirdi.
///
///    KURAL: Kullanıcıdan gelen modele SADECE gereken alanları koy.
/// </summary>
public class KayitViewModel
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(255, MinimumLength = 3,
        ErrorMessage = "Ad soyad 3-255 karakter olmalıdır.")]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = "";

    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Kullanıcı adı 3-100 karakter olmalıdır.")]
    // Sadece harf, rakam, alt çizgi ve nokta — boşluk ve Türkçe karakter yok.
    // Kullanıcı adları URL'de ve sistemlerde sorun çıkarmasın.
    [RegularExpression(@"^[a-zA-Z0-9._]+$",
        ErrorMessage = "Kullanıcı adı sadece harf, rakam, nokta ve alt çizgi içerebilir.")]
    [Display(Name = "Kullanıcı adı")]
    public string KullaniciAdi { get; set; } = "";

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 6,
        ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    // ⭐ [Compare] — iki alanın AYNI olmasını şart koşar.
    //    Parametre olarak diğer ÖZELLİĞİN ADI yazılır, metni değil.
    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [Compare(nameof(Sifre), ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre (tekrar)")]
    public string SifreTekrar { get; set; } = "";
}