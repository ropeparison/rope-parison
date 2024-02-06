using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeParison.Migrations
{
    /// <inheritdoc />
    public partial class Mig10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RopeEditSuggestions_Ropes_RopeId",
                table: "RopeEditSuggestions");

            migrationBuilder.AlterColumn<int>(
                name: "RopeId",
                table: "RopeEditSuggestions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RopeProperty",
                table: "RopeEditSuggestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_RopeEditSuggestions_Ropes_RopeId",
                table: "RopeEditSuggestions",
                column: "RopeId",
                principalTable: "Ropes",
                principalColumn: "RopeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RopeEditSuggestions_Ropes_RopeId",
                table: "RopeEditSuggestions");

            migrationBuilder.DropColumn(
                name: "RopeProperty",
                table: "RopeEditSuggestions");

            migrationBuilder.AlterColumn<int>(
                name: "RopeId",
                table: "RopeEditSuggestions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_RopeEditSuggestions_Ropes_RopeId",
                table: "RopeEditSuggestions",
                column: "RopeId",
                principalTable: "Ropes",
                principalColumn: "RopeId");
        }
    }
}
