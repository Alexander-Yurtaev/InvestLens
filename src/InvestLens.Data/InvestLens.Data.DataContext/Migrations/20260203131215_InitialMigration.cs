using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InvestLens.Data.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "engine",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    refresh_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_error = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    secid = table.Column<string>(type: "character varying(51)", maxLength: 51, nullable: false),
                    shortname = table.Column<string>(type: "character varying(189)", maxLength: 189, nullable: false, defaultValue: ""),
                    regnumber = table.Column<string>(type: "character varying(189)", maxLength: 189, nullable: true),
                    name = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    isin = table.Column<string>(type: "character varying(51)", maxLength: 51, nullable: true),
                    is_traded = table.Column<bool>(type: "boolean", nullable: false),
                    emitent_id = table.Column<int>(type: "integer", nullable: true),
                    emitent_title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: true),
                    emitent_inn = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    emitent_okpo = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: true),
                    type = table.Column<string>(type: "character varying(93)", maxLength: 93, nullable: false),
                    group = table.Column<string>(type: "character varying(93)", maxLength: 93, nullable: false),
                    primary_boardid = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    marketprice_boardid = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_status_entity_name",
                table: "refresh_status",
                column: "entity_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_security_secid",
                table: "security",
                column: "secid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine");

            migrationBuilder.DropTable(
                name: "refresh_status");

            migrationBuilder.DropTable(
                name: "security");
        }
    }
}
