using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nsia.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationAndHowDidYouHear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HowDidYouHear",
                table: "Applications",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Applications",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HowDidYouHear",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Applications");
        }
    }
}
