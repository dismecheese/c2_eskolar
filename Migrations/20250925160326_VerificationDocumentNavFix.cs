using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class VerificationDocumentNavFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VerificationDocuments_AspNetUsers_UserId2",
                table: "VerificationDocuments");

            migrationBuilder.DropIndex(
                name: "IX_VerificationDocuments_UserId2",
                table: "VerificationDocuments");

            migrationBuilder.DropColumn(
                name: "UserId2",
                table: "VerificationDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId2",
                table: "VerificationDocuments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerificationDocuments_UserId2",
                table: "VerificationDocuments",
                column: "UserId2");

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationDocuments_AspNetUsers_UserId2",
                table: "VerificationDocuments",
                column: "UserId2",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
