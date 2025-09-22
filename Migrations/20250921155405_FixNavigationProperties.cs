using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserId",
                table: "ScholarshipApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId1",
                table: "ScholarshipApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipApplications_StudentProfileId1",
                table: "ScholarshipApplications",
                column: "StudentProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ScholarshipApplications_StudentProfiles_StudentProfileId1",
                table: "ScholarshipApplications",
                column: "StudentProfileId1",
                principalTable: "StudentProfiles",
                principalColumn: "StudentProfileId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScholarshipApplications_StudentProfiles_StudentProfileId1",
                table: "ScholarshipApplications");

            migrationBuilder.DropIndex(
                name: "IX_ScholarshipApplications_StudentProfileId1",
                table: "ScholarshipApplications");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "ScholarshipApplications");

            migrationBuilder.DropColumn(
                name: "StudentProfileId1",
                table: "ScholarshipApplications");
        }
    }
}
