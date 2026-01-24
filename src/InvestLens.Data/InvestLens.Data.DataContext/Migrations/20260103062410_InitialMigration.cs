using Microsoft.EntityFrameworkCore.Migrations;

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
                name: "RefreshStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    refresh_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_error = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    secid = table.Column<string>(type: "character varying(51)", maxLength: 51, nullable: false),
                    shortname = table.Column<string>(type: "character varying(189)", maxLength: 189, nullable: false),
                    regnumber = table.Column<string>(type: "character varying(189)", maxLength: 189, nullable: false),
                    name = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    isin = table.Column<string>(type: "character varying(51)", maxLength: 51, nullable: false),
                    is_trade = table.Column<bool>(type: "boolean", nullable: false),
                    emitent_id = table.Column<int>(type: "integer", nullable: false),
                    emitent_title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    emitent_inn = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    emitent_okpo = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    type = table.Column<string>(type: "character varying(93)", maxLength: 93, nullable: false),
                    group = table.Column<string>(type: "character varying(93)", maxLength: 93, nullable: false),
                    primaryprice_boardid = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    marketprice_boardid = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshStatus_entity_name",
                table: "RefreshStatus",
                column: "entity_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Security_secid",
                table: "Security",
                column: "secid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshStatus");

            migrationBuilder.DropTable(
                name: "Security");
        }
    }
}
