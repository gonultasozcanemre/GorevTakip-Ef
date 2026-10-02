using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gorevTakipEFApp.Migrations
{
    /// <inheritdoc />
    public partial class CokluKullanici : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Kategoriler_KategoriAd",
                table: "Kategoriler");

            migrationBuilder.AddColumn<long>(
                name: "KullaniciId",
                table: "Kategoriler",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "KullaniciId",
                table: "Gorevler",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Kategoriler",
                keyColumn: "KategoriId",
                keyValue: 1L,
                column: "KullaniciId",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "Kategoriler",
                keyColumn: "KategoriId",
                keyValue: 2L,
                column: "KullaniciId",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "Kategoriler",
                keyColumn: "KategoriId",
                keyValue: 3L,
                column: "KullaniciId",
                value: 1L);

            migrationBuilder.UpdateData(
                table: "Kategoriler",
                keyColumn: "KategoriId",
                keyValue: 4L,
                column: "KullaniciId",
                value: 1L);

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_KullaniciId_KategoriAd",
                table: "Kategoriler",
                columns: new[] { "KullaniciId", "KategoriAd" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_KullaniciId",
                table: "Gorevler",
                column: "KullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevler_Kullanicilar_KullaniciId",
                table: "Gorevler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "KullaniciId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kategoriler_Kullanicilar_KullaniciId",
                table: "Kategoriler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "KullaniciId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevler_Kullanicilar_KullaniciId",
                table: "Gorevler");

            migrationBuilder.DropForeignKey(
                name: "FK_Kategoriler_Kullanicilar_KullaniciId",
                table: "Kategoriler");

            migrationBuilder.DropIndex(
                name: "IX_Kategoriler_KullaniciId_KategoriAd",
                table: "Kategoriler");

            migrationBuilder.DropIndex(
                name: "IX_Gorevler_KullaniciId",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Kategoriler");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Gorevler");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_KategoriAd",
                table: "Kategoriler",
                column: "KategoriAd",
                unique: true);
        }
    }
}
