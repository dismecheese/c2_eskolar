using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddBenefactorFieldsToStudentProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPartnerBenefactor",
                table: "StudentProfiles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerBenefactorName",
                table: "StudentProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPartnerBenefactor",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "PartnerBenefactorName",
                table: "StudentProfiles");
        }
    }
}
