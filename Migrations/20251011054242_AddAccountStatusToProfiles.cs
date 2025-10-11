using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountStatusToProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "InstitutionProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "BenefactorProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "InstitutionProfiles");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "BenefactorProfiles");
        }
    }
}
