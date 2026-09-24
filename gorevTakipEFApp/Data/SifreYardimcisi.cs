using System.Security.Cryptography;
using System.Text;

namespace GorevTakip.Data;

/// <summary>
/// Şifre hash'leme.
/// ⚠️ ÖĞRETİM AMAÇLIDIR. Gerçek projede BCrypt/Argon2 kullanın.
/// </summary>
public static class SifreYardimcisi
{
    public static string Hashle(string sifre)
    {
        byte[] bayt = Encoding.UTF8.GetBytes(sifre);   // metni bayta çevir
        byte[] hash = SHA256.HashData(bayt);           // özeti hesapla
        return Convert.ToHexString(hash);              // okunabilir metne çevir
    }
}