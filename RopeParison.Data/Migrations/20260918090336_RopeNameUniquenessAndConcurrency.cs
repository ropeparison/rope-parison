using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeParison.Migrations
{
    /// <inheritdoc />
    public partial class RopeNameUniquenessAndConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Ropes] WHERE [Name] IS NULL
                    OR LEN(LTRIM(RTRIM([Name]))) = 0 OR DATALENGTH([Name]) > 300)
                    THROW 51001, 'Rope names must be nonblank and at most 150 characters. Resolve invalid names before migrating.', 1;
                IF EXISTS (SELECT 1 FROM [Ropes]
                    GROUP BY LTRIM(RTRIM([Name])) COLLATE Latin1_General_100_CI_AS HAVING COUNT(*) > 1)
                    THROW 51002, 'Duplicate rope names exist after trimming and ignoring case. Resolve duplicates before migrating.', 1;
                """);
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ropes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Ropes",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Ropes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                computedColumnSql: "LTRIM(RTRIM([Name])) COLLATE Latin1_General_100_CI_AS",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "UX_Ropes_NormalizedName",
                table: "Ropes",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ropes_Name_NotBlank",
                table: "Ropes",
                sql: "LEN(LTRIM(RTRIM([Name]))) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Ropes_NormalizedName",
                table: "Ropes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ropes_Name_NotBlank",
                table: "Ropes");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Ropes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Ropes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ropes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
