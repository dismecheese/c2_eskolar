using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionalEmailToStudentProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstitutionalEmail",
                table: "StudentProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstitutionalEmail",
                table: "StudentProfiles");
        }
    }
}
