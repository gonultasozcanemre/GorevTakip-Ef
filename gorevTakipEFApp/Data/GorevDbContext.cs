using Microsoft.EntityFrameworkCore;
using GorevTakip.Models;
using System.Security.Claims;

namespace GorevTakip.Data;

public class GorevDbContext : DbContext
{
    // ══════════════════════════════════════════════════════
    //  AKTİF KULLANICI
    //
    //  ⭐ readonly: yapıcıda bir kez atanır, sonra değişmez.
    //     Her HTTP isteği için YENİ bir DbContext üretildiğini
    //     hatırlayın (Modül 0 — Scoped ömür). Yani her istekte
    //     bu alan o isteğin kullanıcısıyla dolar.
    // ══════════════════════════════════════════════════════
    private readonly long _aktifKullaniciId;

    /// <summary>
    /// Controller'lar yeni kayıt oluştururken bunu kullanır.
    /// Giriş yapılmamışsa 0 döner.
    /// </summary>
    public long AktifKullaniciId => _aktifKullaniciId;

    public GorevDbContext(DbContextOptions<GorevDbContext> options,
                          IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        // Giriş yapan kullanıcının id'si çerezde claim olarak duruyor.
        // Modül 2'de şu satırı yazmıştık:
        //     new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString())
        // Şimdi onu geri okuyoruz.
        //
        // ⚠️ ?. operatörleri şart:
        //    - HttpContext null olabilir (arka plan işleri, seeder)
        //    - User null olabilir
        //    - Claim bulunamayabilir (giriş yapılmamış)
        var claim = httpContextAccessor.HttpContext?.User
                        ?.FindFirst(ClaimTypes.NameIdentifier);

        if (claim != null && long.TryParse(claim.Value, out long id))
            _aktifKullaniciId = id;
        else
            _aktifKullaniciId = 0;   // giriş yok → hiçbir kaydın sahibi 0 değil → boş liste
    }

    public DbSet<Kategori> Kategoriler { get; set; }
    public DbSet<Gorev> Gorevler { get; set; }
    public DbSet<Kullanici> Kullanicilar { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ══════════════════════════════════════════════════════
        //  1) ENUM DÖNÜŞÜMÜ
        //
        //  Durum → veritabanına METİN olarak yazılsın.
        //  Oncelik için bir şey yazmıyoruz → varsayılan olarak INT.
        //
        //  ⚠️ Migration üretmeden ÖNCE bu satırı yazın.
        //     Sonradan eklerseniz sütun tipi değişir ve
        //     var olan veriler dönüştürülemez.
        // ══════════════════════════════════════════════════════
        modelBuilder.Entity<Gorev>()
            .Property(g => g.Durum)
            .HasConversion<string>()
            .HasMaxLength(20);

        // ══════════════════════════════════════════════════════
        //  2) BENZERSİZLİK
        // ══════════════════════════════════════════════════════
        // ✅ Aynı kullanıcı aynı adı iki kez kullanamaz,
        //    farklı kullanıcılar aynı adı kullanabilir
        modelBuilder.Entity<Kategori>()
            .HasIndex(k => new { k.KullaniciId, k.KategoriAd })
            .IsUnique();

        modelBuilder.Entity<Kullanici>()
            .HasIndex(k => k.KullaniciAdi)
            .IsUnique();

        // ══════════════════════════════════════════════════════
        //  3) SIK SORGULANAN ALANLARA İNDEKS
        //
        //  Görev listesini durum ve bitiş tarihine göre
        //  sürekli filtreleyeceğiz. İndeks bunu hızlandırır.
        // ══════════════════════════════════════════════════════
        modelBuilder.Entity<Gorev>()
            .HasIndex(g => g.Durum);

        modelBuilder.Entity<Gorev>()
            .HasIndex(g => g.BitisTarihi);

        // ══════════════════════════════════════════════════════
        //  4) SİLME DAVRANIŞI
        //
        //  Varsayılan Cascade: kategori silinirse görevleri de silinir.
        //  Biz bunu İSTEMİYORUZ — zaten soft delete kullanacağız.
        // ══════════════════════════════════════════════════════
        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.Kategori)
            .WithMany(k => k.Gorevler)
            .HasForeignKey(g => g.KategoriId)
            .OnDelete(DeleteBehavior.Restrict);


        // ── Sahiplik ilişkileri ──────────────────────────
        modelBuilder.Entity<Kategori>()
            .HasOne(k => k.Kullanici)
            .WithMany(u => u.Kategoriler)
            .HasForeignKey(k => k.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.Kullanici)
            .WithMany(u => u.Gorevler)
            .HasForeignKey(g => g.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        // ══════════════════════════════════════════════════════
        //  5) GLOBAL QUERY FILTER — soft delete
        //
        //  Bundan sonra bu tablolara yapılan HER sorguya
        //  EF otomatik "WHERE AktifMi = 1" ekler.
        //
        //  ADO.NET'te bunu her sorguya elle yazıyorduk.
        // ══════════════════════════════════════════════════════
        // ⭐⭐ Artık iki koşul: silinmemiş VE bu kullanıcıya ait
        modelBuilder.Entity<Kategori>()
            .HasQueryFilter(k => k.AktifMi && k.KullaniciId == _aktifKullaniciId);
        modelBuilder.Entity<Gorev>()
           .HasQueryFilter(g => g.AktifMi && g.KullaniciId == _aktifKullaniciId);
        modelBuilder.Entity<Kullanici>().HasQueryFilter(k => k.AktifMi);



        // ══════════════════════════════════════════════════════
        //  SABİT BAŞLANGIÇ VERİSİ
        //
        //  ⚠️ DateTime.Now KULLANILAMAZ — sabit tarih zorunlu.
        //     Sebebi: bu veri migration dosyasına gömülür.
        // ══════════════════════════════════════════════════════
        var sabitTarih = new DateTime(2026, 1, 1, 9, 0, 0);

        modelBuilder.Entity<Kategori>().HasData(
            new Kategori
            {
                KategoriId = 1,
                KullaniciId = 1,
                KategoriAd = "İş",
                Renk = "primary",
                Aciklama = "İşle ilgili görevler ve toplantılar",
                CreatedDate = sabitTarih,
                AktifMi = true
            },
            new Kategori
            {
                KategoriId = 2,
                KullaniciId = 1,
                KategoriAd = "Okul",
                Renk = "success",
                Aciklama = "Ders, ödev ve sınav hazırlıkları",
                CreatedDate = sabitTarih,
                AktifMi = true
            },
            new Kategori
            {
                KategoriId = 3,
                KullaniciId = 1,
                KategoriAd = "Ev",
                Renk = "warning",
                Aciklama = null,               // ⭐ NULL olabilen alan
                CreatedDate = sabitTarih,
                AktifMi = true
            },
            new Kategori
            {
                KategoriId = 4,
                KullaniciId = 1,
                KategoriAd = "Kişisel",
                Renk = "info",
                Aciklama = "Kişisel gelişim ve sağlık",
                CreatedDate = sabitTarih,
                AktifMi = true
            }
        );

        //Admin hesabı — şifre: gorev123
        //SifreYardimcisi sınıfını Modül 2'de yazacağız.
        //Şimdilik bu bloğu YORUM olarak bırakın, Modül 2'de açacağız.

        modelBuilder.Entity<Kullanici>().HasData(
            new Kullanici
            {
                KullaniciId = 1,
                KullaniciAdi = "admin",
                SifreHash = SifreYardimcisi.Hashle("gorev123"),
                AdSoyad = "Sistem Yöneticisi",
                CreatedDate = sabitTarih,
                AktifMi = true
            }
        );
    }
}