using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InvestLens.Data.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddGinIndexesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.DropIndex(
                name: "IX_security_secid",
                table: "security");

            migrationBuilder.DropPrimaryKey(
                name: "PK_duration",
                table: "duration");

            migrationBuilder.AlterColumn<int>(
                name: "interval",
                table: "duration",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_duration",
                table: "duration",
                column: "interval");

            migrationBuilder.CreateIndex(
                name: "IX_Securities_Isin_GIN",
                table: "security",
                column: "isin")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Securities_Name_GIN",
                table: "security",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Securities_SecId_GIN",
                table: "security",
                column: "secid")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Securities_ShortName_GIN",
                table: "security",
                column: "shortname")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.DropIndex(
                name: "IX_Securities_Isin_GIN",
                table: "security");

            migrationBuilder.DropIndex(
                name: "IX_Securities_Name_GIN",
                table: "security");

            migrationBuilder.DropIndex(
                name: "IX_Securities_SecId_GIN",
                table: "security");

            migrationBuilder.DropIndex(
                name: "IX_Securities_ShortName_GIN",
                table: "security");

            migrationBuilder.DropPrimaryKey(
                name: "PK_duration",
                table: "duration");

            migrationBuilder.AlterColumn<int>(
                name: "interval",
                table: "duration",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_duration",
                table: "duration",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_security_secid",
                table: "security",
                column: "secid",
                unique: true);
        }
    }
}
