using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationFieldsToStudentProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorDocumentPath",
                table: "StudentProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPartnerInstitution",
                table: "StudentProfiles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerInstitutionName",
                table: "StudentProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentIdDocumentPath",
                table: "StudentProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorDocumentPath",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "IsPartnerInstitution",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "PartnerInstitutionName",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "StudentIdDocumentPath",
                table: "StudentProfiles");
        }
    }
}
