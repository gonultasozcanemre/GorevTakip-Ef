using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gorevTakipEFApp.Migrations
{
    /// <inheritdoc />
    public partial class AdminKullanicisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "KullaniciId", "AdSoyad", "AktifMi", "CreatedDate", "KullaniciAdi", "SifreHash" },
                values: new object[] { 1L, "Sistem Yöneticisi", true, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "admin", "EC7F93F0A99692C1F91E696226BF3F4C0F711A0380921642B466A62C1628C440" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Kullanicilar",
                keyColumn: "KullaniciId",
                keyValue: 1L);
        }
    }
}
