using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nsia.Migrations
{
    /// <inheritdoc />
    public partial class AgreeToSubmissionAgreement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AgreeToSubmissionAgreement",
                table: "Applications",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgreeToSubmissionAgreement",
                table: "Applications");
        }
    }
}
