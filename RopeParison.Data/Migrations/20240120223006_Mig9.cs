using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeParison.Migrations
{
    /// <inheritdoc />
    public partial class Mig9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RopeEditSuggestions",
                columns: table => new
                {
                    RopeEditSuggestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: true),
                    Diameter = table.Column<double>(type: "float", nullable: true),
                    MassPerUnitLength = table.Column<double>(type: "float", nullable: true),
                    SheathPercentage = table.Column<double>(type: "float", nullable: true),
                    ImpactForce55kgOneStrand = table.Column<double>(type: "float", nullable: true),
                    ImpactForce80kgOneStrand = table.Column<double>(type: "float", nullable: true),
                    ImpactForce80kgTwoStrand = table.Column<double>(type: "float", nullable: true),
                    StaticElongation80kgOneStrand = table.Column<double>(type: "float", nullable: true),
                    StaticElongation80kgTwoStrand = table.Column<double>(type: "float", nullable: true),
                    DynamicElongation55kgOneStrand = table.Column<double>(type: "float", nullable: true),
                    DynamicElongation80kgOneStrand = table.Column<double>(type: "float", nullable: true),
                    DynamicElongation80kgTwoStrand = table.Column<double>(type: "float", nullable: true),
                    DropsBeforeBreak55kgOneStrand = table.Column<int>(type: "int", nullable: true),
                    DropsBeforeBreak80kgOneStrand = table.Column<int>(type: "int", nullable: true),
                    DropsBeforeBreak80kgTwoStrand = table.Column<int>(type: "int", nullable: true),
                    SheathSlippage = table.Column<double>(type: "float", nullable: true),
                    UpVoteCount = table.Column<double>(type: "float", nullable: false),
                    DownVoteCount = table.Column<double>(type: "float", nullable: false),
                    RopeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RopeEditSuggestions", x => x.RopeEditSuggestionId);
                    table.ForeignKey(
                        name: "FK_RopeEditSuggestions_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "BrandId");
                    table.ForeignKey(
                        name: "FK_RopeEditSuggestions_Ropes_RopeId",
                        column: x => x.RopeId,
                        principalTable: "Ropes",
                        principalColumn: "RopeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RopeEditSuggestions_BrandId",
                table: "RopeEditSuggestions",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_RopeEditSuggestions_RopeId",
                table: "RopeEditSuggestions",
                column: "RopeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RopeEditSuggestions");
        }
    }
}
