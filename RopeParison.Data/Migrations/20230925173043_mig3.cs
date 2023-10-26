using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeParison.Migrations
{
    /// <inheritdoc />
    public partial class mig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropsBeforeBreak",
                table: "Ropes");

            migrationBuilder.AddColumn<int>(
                name: "DropsBeforeBreak55kgOneStrand",
                table: "Ropes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DropsBeforeBreak80kgOneStrand",
                table: "Ropes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DropsBeforeBreak80kgTwoStrand",
                table: "Ropes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropsBeforeBreak55kgOneStrand",
                table: "Ropes");

            migrationBuilder.DropColumn(
                name: "DropsBeforeBreak80kgOneStrand",
                table: "Ropes");

            migrationBuilder.DropColumn(
                name: "DropsBeforeBreak80kgTwoStrand",
                table: "Ropes");

            migrationBuilder.AddColumn<int>(
                name: "DropsBeforeBreak",
                table: "Ropes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
