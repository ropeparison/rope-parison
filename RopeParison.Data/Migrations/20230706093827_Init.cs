using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeParison.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "Ropes",
                columns: table => new
                {
                    RopeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Diameter = table.Column<double>(type: "float", nullable: false),
                    MassPerUnitLength = table.Column<double>(type: "float", nullable: false),
                    Area = table.Column<double>(type: "float", nullable: false),
                    SheathPercentage = table.Column<double>(type: "float", nullable: false),
                    SheathArea = table.Column<double>(type: "float", nullable: false),
                    SheathThickness = table.Column<double>(type: "float", nullable: false),
                    CoreArea = table.Column<double>(type: "float", nullable: false),
                    CoreDiameter = table.Column<double>(type: "float", nullable: false),
                    ImpactForce55kgOneStrand = table.Column<double>(type: "float", nullable: false),
                    ImpactForce80kgOneStrand = table.Column<double>(type: "float", nullable: false),
                    ImpactForce80kgTwoStrand = table.Column<double>(type: "float", nullable: false),
                    StaticElongation80kgOneStrand = table.Column<double>(type: "float", nullable: false),
                    StaticElongation80kgTwoStrand = table.Column<double>(type: "float", nullable: false),
                    DynamicElongation55kgOneStrand = table.Column<double>(type: "float", nullable: false),
                    DynamicElongation80kgOneStrand = table.Column<double>(type: "float", nullable: false),
                    DynamicElongation80kgTwoStrand = table.Column<double>(type: "float", nullable: false),
                    DropsBeforeBreak = table.Column<int>(type: "int", nullable: false),
                    SheathSlippage = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ropes", x => x.RopeId);
                    table.ForeignKey(
                        name: "FK_Ropes_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ropes_BrandId",
                table: "Ropes",
                column: "BrandId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ropes");

            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
