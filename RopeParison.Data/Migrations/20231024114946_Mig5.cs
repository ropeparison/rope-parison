using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeParison.Migrations
{
    /// <inheritdoc />
    public partial class Mig5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RopeCalculatedParameterSets_Ropes_RopeID",
                table: "RopeCalculatedParameterSets");

            migrationBuilder.RenameColumn(
                name: "RopeID",
                table: "RopeCalculatedParameterSets",
                newName: "RopeId");

            migrationBuilder.RenameIndex(
                name: "IX_RopeCalculatedParameterSets_RopeID",
                table: "RopeCalculatedParameterSets",
                newName: "IX_RopeCalculatedParameterSets_RopeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RopeCalculatedParameterSets_Ropes_RopeId",
                table: "RopeCalculatedParameterSets",
                column: "RopeId",
                principalTable: "Ropes",
                principalColumn: "RopeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RopeCalculatedParameterSets_Ropes_RopeId",
                table: "RopeCalculatedParameterSets");

            migrationBuilder.RenameColumn(
                name: "RopeId",
                table: "RopeCalculatedParameterSets",
                newName: "RopeID");

            migrationBuilder.RenameIndex(
                name: "IX_RopeCalculatedParameterSets_RopeId",
                table: "RopeCalculatedParameterSets",
                newName: "IX_RopeCalculatedParameterSets_RopeID");

            migrationBuilder.AddForeignKey(
                name: "FK_RopeCalculatedParameterSets_Ropes_RopeID",
                table: "RopeCalculatedParameterSets",
                column: "RopeID",
                principalTable: "Ropes",
                principalColumn: "RopeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
