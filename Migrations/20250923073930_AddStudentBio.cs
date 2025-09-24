using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentBio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScholarshipApplications_StudentProfiles_StudentProfileId",
                table: "ScholarshipApplications");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "StudentProfiles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScholarshipApplications_StudentProfiles_StudentProfileId",
                table: "ScholarshipApplications",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "StudentProfileId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_AspNetUsers_UserId",
                table: "StudentProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScholarshipApplications_StudentProfiles_StudentProfileId",
                table: "ScholarshipApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_AspNetUsers_UserId",
                table: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "StudentProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_ScholarshipApplications_StudentProfiles_StudentProfileId",
                table: "ScholarshipApplications",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "StudentProfileId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
