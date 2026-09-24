namespace GorevTakip.Models;

/// <summary>
/// Görev önceliği.
///
/// ⭐ Sayı değerlerini AÇIKÇA yazıyoruz (= 1, = 2, = 3).
///    Yazmasaydık C# 0, 1, 2 verirdi ve sıralamada
///    "Dusuk = 0" olurdu. Biz 1'den başlamasını istiyoruz.
///
///    Ayrıca: bu sayılar veritabanına YAZILACAK.
///    Sonradan değiştirirseniz eski kayıtlar bozulur.
/// </summary>
public enum Oncelik
{
    Dusuk = 1,
    Orta = 2,
    Yuksek = 3
}

/// <summary>
/// Görev durumu.
///
/// Bu enum veritabanına METİN olarak yazılacak (Modül'ün sonunda
/// nedenini göreceğiz). Bu yüzden isimler okunabilir olmalı.
/// </summary>
public enum GorevDurum
{
    Beklemede,
    DevamEdiyor,
    Tamamlandi
}