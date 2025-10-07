using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminValidationDocumentToInstitutionProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "StudentProfiles");

            migrationBuilder.AddColumn<string>(
                name: "AdminValidationDocument",
                table: "InstitutionProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminValidationDocument",
                table: "InstitutionProfiles");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "StudentProfiles",
                type: "int",
                nullable: true);
        }
    }
}
