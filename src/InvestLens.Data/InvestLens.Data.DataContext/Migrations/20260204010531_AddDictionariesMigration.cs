using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InvestLens.Data.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddDictionariesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "board",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    board_group_id = table.Column<int>(type: "integer", nullable: false),
                    engine_id = table.Column<int>(type: "integer", nullable: false),
                    market_id = table.Column<int>(type: "integer", nullable: false),
                    boardid = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    board_title = table.Column<string>(type: "character varying(381)", maxLength: 381, nullable: false),
                    is_traded = table.Column<bool>(type: "boolean", nullable: false),
                    has_candles = table.Column<bool>(type: "boolean", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_board", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "boardgroup",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trade_engine_id = table.Column<int>(type: "integer", nullable: false),
                    trade_engine_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    trade_engine_title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    market_id = table.Column<int>(type: "integer", nullable: false),
                    market_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    name = table.Column<string>(type: "character varying(192)", maxLength: 192, nullable: false),
                    title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    board_group_id = table.Column<int>(type: "integer", nullable: false),
                    is_traded = table.Column<bool>(type: "boolean", nullable: false),
                    is_order_driven = table.Column<bool>(type: "boolean", nullable: false),
                    category = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boardgroup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "duration",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    interval = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    days = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    hint = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_duration", x => x.interval);
                });

            migrationBuilder.CreateTable(
                name: "securitygroup",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(93)", maxLength: 93, nullable: false),
                    title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_securitygroup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "securitycollection",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    security_group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_securitycollection", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "securitytype",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trade_engine_id = table.Column<int>(type: "integer", nullable: false),
                    trade_engine_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    trade_engine_title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    security_type_name = table.Column<string>(type: "character varying(93)", maxLength: 93, nullable: false),
                    security_type_title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    security_group_name = table.Column<string>(type: "character varying(93)", maxLength: 93, nullable: false),
                    stock_type = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_securitytype", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "board");

            migrationBuilder.DropTable(
                name: "boardgroup");

            migrationBuilder.DropTable(
                name: "duration");

            migrationBuilder.DropTable(
                name: "securitygroup");

            migrationBuilder.DropTable(
                name: "securitycollection");

            migrationBuilder.DropTable(
                name: "securitytype");
        }
    }
}
