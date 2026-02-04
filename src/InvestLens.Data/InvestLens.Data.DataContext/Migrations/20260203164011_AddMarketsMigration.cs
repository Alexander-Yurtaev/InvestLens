using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InvestLens.Data.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "market",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trade_engine_id = table.Column<int>(type: "integer", nullable: false),
                    trade_engine_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    trade_engine_title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    market_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    market_title = table.Column<string>(type: "character varying(765)", maxLength: 765, nullable: false),
                    market_id = table.Column<int>(type: "integer", nullable: false),
                    marketplace = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    is_otc = table.Column<bool>(type: "boolean", nullable: false),
                    has_history_files = table.Column<bool>(type: "boolean", nullable: false),
                    has_history_trades_files = table.Column<bool>(type: "boolean", nullable: false),
                    has_trades = table.Column<bool>(type: "boolean", nullable: false),
                    has_history = table.Column<bool>(type: "boolean", nullable: false),
                    has_candles = table.Column<bool>(type: "boolean", nullable: false),
                    has_orderbook = table.Column<bool>(type: "boolean", nullable: false),
                    has_tradingsession = table.Column<bool>(type: "boolean", nullable: false),
                    has_extra_yields = table.Column<bool>(type: "boolean", nullable: false),
                    has_delay = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "market");
        }
    }
}
