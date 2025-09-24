using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_StudentProfileId",
                table: "AspNetUsers",
                column: "StudentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_StudentProfiles_StudentProfileId",
                table: "AspNetUsers",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "StudentProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_StudentProfiles_StudentProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_StudentProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "AspNetUsers");
        }
    }
}
