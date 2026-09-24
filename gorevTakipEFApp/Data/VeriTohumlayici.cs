using Microsoft.EntityFrameworkCore;
using GorevTakip.Models;

namespace GorevTakip.Data;

/// <summary>
/// Uygulama açılışında çalışan test verisi yükleyicisi.
///
/// ⭐ HasData'dan farkı: burada DateTime.Today KULLANILABİLİR.
///    Çünkü bu kod migration dosyasına gömülmez, her açılışta çalışır.
///
/// ⚠️ SADECE GELİŞTİRME ORTAMI İÇİNDİR.
///    Program.cs'te ortam kontrolüyle çağıracağız.
/// </summary>
public static class VeriTohumlayici
{
    public static async Task GorevleriEkleAsync(GorevDbContext db)
    {
        // ⭐ EN ÖNEMLİ SATIR: zaten veri varsa hiçbir şey yapma.
        //    Olmasaydı uygulama her açıldığında kopya kayıt eklenirdi.
        if (await db.Gorevler.AnyAsync())
            return;

        var bugun = DateTime.Today;
        var simdi = DateTime.Now;

        var gorevler = new List<Gorev>
        {
            // ───────── İŞ (KategoriId = 1) ─────────

            // ⚠️ GECİKMİŞ: tarih geçmiş, tamamlanmamış
            new Gorev
            {
                KategoriId = 1,
                Baslik = "Aylık satış raporunu hazırla",
                Aciklama = "Geçen ayın rakamlarını tabloya dök, grafiklerle destekle.",
                Oncelik = Oncelik.Yuksek,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = bugun.AddDays(-5),
                CreatedDate = simdi.AddDays(-20)
            },

            // ⚠️ GECİKMİŞ
            new Gorev
            {
                KategoriId = 1,
                Baslik = "Müşteri sunumunu güncelle",
                Aciklama = null,                       // ⭐ açıklaması yok
                Oncelik = Oncelik.Orta,
                Durum = GorevDurum.DevamEdiyor,
                BitisTarihi = bugun.AddDays(-2),
                CreatedDate = simdi.AddDays(-15)
            },

            // 📅 BUGÜN
            new Gorev
            {
                KategoriId = 1,
                Baslik = "Ekip toplantısı notlarını paylaş",
                Aciklama = "Konuşulan maddeleri özetleyip e-posta at.",
                Oncelik = Oncelik.Orta,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = bugun,
                CreatedDate = simdi.AddDays(-2)
            },

            // ✅ TAMAMLANDI
            new Gorev
            {
                KategoriId = 1,
                Baslik = "Bütçe tablosunu kontrol et",
                Aciklama = "Geçen çeyrek harcamalarını gözden geçir.",
                Oncelik = Oncelik.Orta,
                Durum = GorevDurum.Tamamlandi,
                BitisTarihi = bugun.AddDays(-8),
                TamamlanmaTarihi = simdi.AddDays(-9),   // ⭐ durum ile tutarlı
                CreatedDate = simdi.AddDays(-25)
            },

            // Yaklaşan
            new Gorev
            {
                KategoriId = 1,
                Baslik = "Yeni proje teklifini yaz",
                Aciklama = "Kapsam, süre ve maliyet tahmini içermeli.",
                Oncelik = Oncelik.Yuksek,
                Durum = GorevDurum.DevamEdiyor,
                BitisTarihi = bugun.AddDays(3),
                CreatedDate = simdi.AddDays(-5)
            },

            // Tarihsiz
            new Gorev
            {
                KategoriId = 1,
                Baslik = "LinkedIn profilini güncelle",
                Aciklama = null,
                Oncelik = Oncelik.Dusuk,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = null,                     // ⭐ son tarihi yok
                CreatedDate = simdi.AddDays(-30)
            },

            // ───────── OKUL (KategoriId = 2) ─────────

            // ⚠️ GECİKMİŞ
            new Gorev
            {
                KategoriId = 2,
                Baslik = "Veritabanı ödevini teslim et",
                Aciklama = "ER diyagramı ve normalizasyon adımları dâhil.",
                Oncelik = Oncelik.Yuksek,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = bugun.AddDays(-1),
                CreatedDate = simdi.AddDays(-12)
            },

            // 📅 BUGÜN
            new Gorev
            {
                KategoriId = 2,
                Baslik = "Web programlama laboratuvarına hazırlan",
                Aciklama = "EF Core modüllerini tekrar et.",
                Oncelik = Oncelik.Yuksek,
                Durum = GorevDurum.DevamEdiyor,
                BitisTarihi = bugun,
                CreatedDate = simdi.AddDays(-3)
            },

            new Gorev
            {
                KategoriId = 2,
                Baslik = "Vize sınavına çalış",
                Aciklama = "İlk 6 hafta konuları.",
                Oncelik = Oncelik.Yuksek,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = bugun.AddDays(7),
                CreatedDate = simdi.AddDays(-4)
            },

            new Gorev
            {
                KategoriId = 2,
                Baslik = "Grup projesi için toplantı ayarla",
                Aciklama = null,
                Oncelik = Oncelik.Orta,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = bugun.AddDays(5),
                CreatedDate = simdi.AddDays(-6)
            },

            // ✅ TAMAMLANDI
            new Gorev
            {
                KategoriId = 2,
                Baslik = "Ders kayıtlarını yenile",
                Aciklama = "Seçmeli dersleri de eklemeyi unutma.",
                Oncelik = Oncelik.Yuksek,
                Durum = GorevDurum.Tamamlandi,
                BitisTarihi = bugun.AddDays(-12),
                TamamlanmaTarihi = simdi.AddDays(-13),
                CreatedDate = simdi.AddDays(-28)
            },

            // ✅ TAMAMLANDI
            new Gorev
            {
                KategoriId = 2,
                Baslik = "Kütüphaneden kitapları iade et",
                Aciklama = null,
                Oncelik = Oncelik.Dusuk,
                Durum = GorevDurum.Tamamlandi,
                BitisTarihi = bugun.AddDays(-4),
                TamamlanmaTarihi = simdi.AddDays(-4),
                CreatedDate = simdi.AddDays(-18)
            },

            // ───────── EV (KategoriId = 3) ─────────

            // ⚠️ GECİKMİŞ
            new Gorev
            {
                KategoriId = 3,
                Baslik = "Elektrik faturasını öde",
                Aciklama = "Son ödeme tarihi geçti, gecikme faizi olabilir.",
                Oncelik = Oncelik.Yuksek,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = bugun.AddDays(-3),
                CreatedDate = simdi.AddDays(-14)
            },

            new Gorev
            {
                KategoriId = 3,
                Baslik = "Market alışverişi yap",
                Aciklama = "Süt, ekmek, yumurta, deterjan.",
                Oncelik = Oncelik.Orta,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = bugun.AddDays(1),
                CreatedDate = simdi.AddDays(-1)
            },

            // ✅ TAMAMLANDI
            new Gorev
            {
                KategoriId = 3,
                Baslik = "Çamaşır makinesinin servisini ara",
                Aciklama = null,
                Oncelik = Oncelik.Orta,
                Durum = GorevDurum.Tamamlandi,
                BitisTarihi = bugun.AddDays(-6),
                TamamlanmaTarihi = simdi.AddDays(-6),
                CreatedDate = simdi.AddDays(-10)
            },

            // Tarihsiz
            new Gorev
            {
                KategoriId = 3,
                Baslik = "Kitaplığı düzenle",
                Aciklama = null,
                Oncelik = Oncelik.Dusuk,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = null,
                CreatedDate = simdi.AddDays(-40)
            },

            // ───────── KİŞİSEL (KategoriId = 4) ─────────

            new Gorev
            {
                KategoriId = 4,
                Baslik = "Diş hekimi randevusu al",
                Aciklama = "6 aylık kontrol zamanı geldi.",
                Oncelik = Oncelik.Orta,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = bugun.AddDays(10),
                CreatedDate = simdi.AddDays(-7)
            },

            // ✅ TAMAMLANDI
            new Gorev
            {
                KategoriId = 4,
                Baslik = "Spor salonu üyeliğini yenile",
                Aciklama = null,
                Oncelik = Oncelik.Dusuk,
                Durum = GorevDurum.Tamamlandi,
                BitisTarihi = bugun.AddDays(-15),
                TamamlanmaTarihi = simdi.AddDays(-16),
                CreatedDate = simdi.AddDays(-35)
            },

            // Tarihsiz
            new Gorev
            {
                KategoriId = 4,
                Baslik = "C# kitabını bitir",
                Aciklama = "Kalan 4 bölüm.",
                Oncelik = Oncelik.Dusuk,
                Durum = GorevDurum.DevamEdiyor,
                BitisTarihi = null,
                CreatedDate = simdi.AddDays(-45)
            },

            // Tarihsiz
            new Gorev
            {
                KategoriId = 4,
                Baslik = "Fotoğraf arşivini yedekle",
                Aciklama = null,
                Oncelik = Oncelik.Dusuk,
                Durum = GorevDurum.Beklemede,
                BitisTarihi = null,
                CreatedDate = simdi.AddDays(-50)
            }
        };

        // ⭐ AddRange — 20 nesneyi tek seferde ekle.
        //    SaveChanges bunları TEK transaction'da yazar.
        db.Gorevler.AddRange(gorevler);
        await db.SaveChangesAsync();
    }
}