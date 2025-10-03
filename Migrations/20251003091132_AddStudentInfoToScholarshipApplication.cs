using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentInfoToScholarshipApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstSemesterGrades",
                table: "ScholarshipApplications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GWA",
                table: "ScholarshipApplications",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondSemesterGrades",
                table: "ScholarshipApplications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "ScholarshipApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstSemesterGrades",
                table: "ScholarshipApplications");

            migrationBuilder.DropColumn(
                name: "GWA",
                table: "ScholarshipApplications");

            migrationBuilder.DropColumn(
                name: "SecondSemesterGrades",
                table: "ScholarshipApplications");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "ScholarshipApplications");
        }
    }
}
