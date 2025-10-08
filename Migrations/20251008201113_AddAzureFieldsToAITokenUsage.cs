using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddAzureFieldsToAITokenUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeploymentName",
                table: "AITokenUsages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "AITokenUsages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestDurationMs",
                table: "AITokenUsages",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeploymentName",
                table: "AITokenUsages");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "AITokenUsages");

            migrationBuilder.DropColumn(
                name: "RequestDurationMs",
                table: "AITokenUsages");
        }
    }
}
