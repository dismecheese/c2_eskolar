using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfileIdToAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "StudentProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "StudentProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "StudentProfiles",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }
    }
}
