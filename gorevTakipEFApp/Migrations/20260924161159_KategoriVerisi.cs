using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace gorevTakipEFApp.Migrations
{
    /// <inheritdoc />
    public partial class KategoriVerisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Kategoriler",
                columns: new[] { "KategoriId", "Aciklama", "AktifMi", "CreatedDate", "KategoriAd", "Renk", "UpdatedDate" },
                values: new object[,]
                {
                    { 1L, "İşle ilgili görevler ve toplantılar", true, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "İş", "primary", null },
                    { 2L, "Ders, ödev ve sınav hazırlıkları", true, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "Okul", "success", null },
                    { 3L, null, true, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "Ev", "warning", null },
                    { 4L, "Kişisel gelişim ve sağlık", true, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "Kişisel", "info", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Kategoriler",
                keyColumn: "KategoriId",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Kategoriler",
                keyColumn: "KategoriId",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Kategoriler",
                keyColumn: "KategoriId",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Kategoriler",
                keyColumn: "KategoriId",
                keyValue: 4L);
        }
    }
}
