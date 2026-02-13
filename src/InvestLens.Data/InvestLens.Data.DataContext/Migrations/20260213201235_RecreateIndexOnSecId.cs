using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestLens.Data.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class RecreateIndexOnSecId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Securities_SecId_GIN",
                table: "security");

            migrationBuilder.CreateIndex(
                name: "IX_security_secid",
                table: "security",
                column: "secid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_security_secid",
                table: "security");

            migrationBuilder.CreateIndex(
                name: "IX_Securities_SecId_GIN",
                table: "security",
                column: "secid")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}
