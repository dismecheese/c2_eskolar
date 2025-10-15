using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyStatisticsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonthlyStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TotalApplications = table.Column<int>(type: "int", nullable: false),
                    AcceptedApplications = table.Column<int>(type: "int", nullable: false),
                    PendingApplications = table.Column<int>(type: "int", nullable: false),
                    RejectedApplications = table.Column<int>(type: "int", nullable: false),
                    TotalUsers = table.Column<int>(type: "int", nullable: false),
                    NewUsers = table.Column<int>(type: "int", nullable: false),
                    TotalStudents = table.Column<int>(type: "int", nullable: false),
                    TotalBenefactors = table.Column<int>(type: "int", nullable: false),
                    TotalInstitutions = table.Column<int>(type: "int", nullable: false),
                    VerifiedUsers = table.Column<int>(type: "int", nullable: false),
                    TotalScholarships = table.Column<int>(type: "int", nullable: false),
                    ActiveScholarships = table.Column<int>(type: "int", nullable: false),
                    TotalScholarshipValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DistributedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BenefactorSuccessRate = table.Column<double>(type: "float", nullable: false),
                    InstitutionSuccessRate = table.Column<double>(type: "float", nullable: false),
                    AverageProcessingDays = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyStatistics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyStatistics_Year_Month",
                table: "MonthlyStatistics",
                columns: new[] { "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonthlyStatistics");
        }
    }
}
