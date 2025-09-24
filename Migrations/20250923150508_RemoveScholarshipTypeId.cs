using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class RemoveScholarshipTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scholarships_ScholarshipTypes_ScholarshipTypeId",
                table: "Scholarships");

            migrationBuilder.AlterColumn<int>(
                name: "ScholarshipTypeId",
                table: "Scholarships",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Scholarships_ScholarshipTypes_ScholarshipTypeId",
                table: "Scholarships",
                column: "ScholarshipTypeId",
                principalTable: "ScholarshipTypes",
                principalColumn: "ScholarshipTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scholarships_ScholarshipTypes_ScholarshipTypeId",
                table: "Scholarships");

            migrationBuilder.AlterColumn<int>(
                name: "ScholarshipTypeId",
                table: "Scholarships",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Scholarships_ScholarshipTypes_ScholarshipTypeId",
                table: "Scholarships",
                column: "ScholarshipTypeId",
                principalTable: "ScholarshipTypes",
                principalColumn: "ScholarshipTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
