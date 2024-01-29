using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeParison.Migrations
{
    /// <inheritdoc />
    public partial class Mig4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Ropes");

            migrationBuilder.DropColumn(
                name: "CoreArea",
                table: "Ropes");

            migrationBuilder.DropColumn(
                name: "CoreDiameter",
                table: "Ropes");

            migrationBuilder.DropColumn(
                name: "SheathArea",
                table: "Ropes");

            migrationBuilder.DropColumn(
                name: "SheathThickness",
                table: "Ropes");

            migrationBuilder.CreateTable(
                name: "RopeCalculatedParameterSets",
                columns: table => new
                {
                    RopeCalculatedParameterSetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Area = table.Column<double>(type: "float", nullable: false),
                    SheathArea = table.Column<double>(type: "float", nullable: false),
                    SheathThickness = table.Column<double>(type: "float", nullable: false),
                    CoreArea = table.Column<double>(type: "float", nullable: false),
                    CoreDiameter = table.Column<double>(type: "float", nullable: false),
                    RopeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RopeCalculatedParameterSets", x => x.RopeCalculatedParameterSetId);
                    table.ForeignKey(
                        name: "FK_RopeCalculatedParameterSets_Ropes_RopeID",
                        column: x => x.RopeID,
                        principalTable: "Ropes",
                        principalColumn: "RopeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RopeCalculatedParameterSets_RopeID",
                table: "RopeCalculatedParameterSets",
                column: "RopeID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RopeCalculatedParameterSets");

            migrationBuilder.AddColumn<double>(
                name: "Area",
                table: "Ropes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CoreArea",
                table: "Ropes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CoreDiameter",
                table: "Ropes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SheathArea",
                table: "Ropes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SheathThickness",
                table: "Ropes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
