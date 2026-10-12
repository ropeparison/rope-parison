using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeParison.Migrations
{
    /// <inheritdoc />
    public partial class GroupedRopeEditSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RopeEditSuggestions_RopeId",
                table: "RopeEditSuggestions");

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionId",
                table: "RopeEditSuggestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RopeEditSuggestions_RopeId_SubmissionId",
                table: "RopeEditSuggestions",
                columns: new[] { "RopeId", "SubmissionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RopeEditSuggestions_RopeId_SubmissionId",
                table: "RopeEditSuggestions");

            migrationBuilder.DropColumn(
                name: "SubmissionId",
                table: "RopeEditSuggestions");

            migrationBuilder.CreateIndex(
                name: "IX_RopeEditSuggestions_RopeId",
                table: "RopeEditSuggestions",
                column: "RopeId");
        }
    }
}
