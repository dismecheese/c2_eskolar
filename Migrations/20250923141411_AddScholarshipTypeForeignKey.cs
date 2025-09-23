using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddScholarshipTypeForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScholarshipType",
                table: "Scholarships");

            migrationBuilder.AddColumn<int>(
                name: "ScholarshipTypeId",
                table: "Scholarships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ScholarshipTypes",
                columns: table => new
                {
                    ScholarshipTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipTypes", x => x.ScholarshipTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scholarships_ScholarshipTypeId",
                table: "Scholarships",
                column: "ScholarshipTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scholarships_ScholarshipTypes_ScholarshipTypeId",
                table: "Scholarships",
                column: "ScholarshipTypeId",
                principalTable: "ScholarshipTypes",
                principalColumn: "ScholarshipTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scholarships_ScholarshipTypes_ScholarshipTypeId",
                table: "Scholarships");

            migrationBuilder.DropTable(
                name: "ScholarshipTypes");

            migrationBuilder.DropIndex(
                name: "IX_Scholarships_ScholarshipTypeId",
                table: "Scholarships");

            migrationBuilder.DropColumn(
                name: "ScholarshipTypeId",
                table: "Scholarships");

            migrationBuilder.AddColumn<string>(
                name: "ScholarshipType",
                table: "Scholarships",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
