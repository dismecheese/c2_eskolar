using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StudentProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "StudentProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "StudentProfiles",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "StudentProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentAddress",
                table: "StudentProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "StudentProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "PermanentAddress",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "StudentProfiles");
        }
    }
}
