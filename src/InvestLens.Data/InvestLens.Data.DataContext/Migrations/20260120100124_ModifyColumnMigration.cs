using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestLens.Data.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class ModifyColumnMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "shortname",
                table: "Security",
                type: "character varying(189)",
                maxLength: 189,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(189)",
                oldMaxLength: 189);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "shortname",
                table: "Security",
                type: "character varying(189)",
                maxLength: 189,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(189)",
                oldMaxLength: 189,
                oldDefaultValue: "");
        }
    }
}
