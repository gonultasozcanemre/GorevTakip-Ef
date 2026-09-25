using Microsoft.EntityFrameworkCore;
using GorevTakip.Models;

namespace GorevTakip.Data;

public class GorevDbContext : DbContext
{
    public GorevDbContext(DbContextOptions<GorevDbContext> options)
        : base(options)
    {
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
        modelBuilder.Entity<Kategori>()
            .HasIndex(k => k.KategoriAd)
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

        // ══════════════════════════════════════════════════════
        //  5) GLOBAL QUERY FILTER — soft delete
        //
        //  Bundan sonra bu tablolara yapılan HER sorguya
        //  EF otomatik "WHERE AktifMi = 1" ekler.
        //
        //  ADO.NET'te bunu her sorguya elle yazıyorduk.
        // ══════════════════════════════════════════════════════
        modelBuilder.Entity<Kategori>().HasQueryFilter(k => k.AktifMi);
        modelBuilder.Entity<Gorev>().HasQueryFilter(g => g.AktifMi);
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
                KategoriAd = "İş",
                Renk = "primary",
                Aciklama = "İşle ilgili görevler ve toplantılar",
                CreatedDate = sabitTarih,
                AktifMi = true
            },
            new Kategori
            {
                KategoriId = 2,
                KategoriAd = "Okul",
                Renk = "success",
                Aciklama = "Ders, ödev ve sınav hazırlıkları",
                CreatedDate = sabitTarih,
                AktifMi = true
            },
            new Kategori
            {
                KategoriId = 3,
                KategoriAd = "Ev",
                Renk = "warning",
                Aciklama = null,               // ⭐ NULL olabilen alan
                CreatedDate = sabitTarih,
                AktifMi = true
            },
            new Kategori
            {
                KategoriId = 4,
                KategoriAd = "Kişisel",
                Renk = "info",
                Aciklama = "Kişisel gelişim ve sağlık",
                CreatedDate = sabitTarih,
                AktifMi = true
            }
        );

        // Admin hesabı — şifre: gorev123
        // SifreYardimcisi sınıfını Modül 2'de yazacağız.
        // Şimdilik bu bloğu YORUM olarak bırakın, Modül 2'de açacağız.
        
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