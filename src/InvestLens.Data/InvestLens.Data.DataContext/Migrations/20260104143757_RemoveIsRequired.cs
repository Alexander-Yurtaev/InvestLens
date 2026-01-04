using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestLens.Data.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "regnumber",
                table: "Security",
                type: "character varying(189)",
                maxLength: 189,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(189)",
                oldMaxLength: 189);

            migrationBuilder.AlterColumn<string>(
                name: "marketprice_boardid",
                table: "Security",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12);

            migrationBuilder.AlterColumn<string>(
                name: "isin",
                table: "Security",
                type: "character varying(51)",
                maxLength: 51,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(51)",
                oldMaxLength: 51);

            migrationBuilder.AlterColumn<string>(
                name: "emitent_title",
                table: "Security",
                type: "character varying(765)",
                maxLength: 765,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(765)",
                oldMaxLength: 765);

            migrationBuilder.AlterColumn<string>(
                name: "emitent_okpo",
                table: "Security",
                type: "character varying(21)",
                maxLength: 21,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);

            migrationBuilder.AlterColumn<string>(
                name: "emitent_inn",
                table: "Security",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<int>(
                name: "emitent_id",
                table: "Security",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "regnumber",
                table: "Security",
                type: "character varying(189)",
                maxLength: 189,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(189)",
                oldMaxLength: 189,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "marketprice_boardid",
                table: "Security",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "isin",
                table: "Security",
                type: "character varying(51)",
                maxLength: 51,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(51)",
                oldMaxLength: 51,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "emitent_title",
                table: "Security",
                type: "character varying(765)",
                maxLength: 765,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(765)",
                oldMaxLength: 765,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "emitent_okpo",
                table: "Security",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "emitent_inn",
                table: "Security",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "emitent_id",
                table: "Security",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
