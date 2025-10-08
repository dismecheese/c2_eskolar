using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class AddScrapedScholarshipManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulkOperationRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalItemsProcessed = table.Column<int>(type: "int", nullable: false),
                    SuccessfulItems = table.Column<int>(type: "int", nullable: false),
                    FailedItems = table.Column<int>(type: "int", nullable: false),
                    FilterCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorLog = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedScholarshipIds = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkOperationRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScrapedScholarships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Benefits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonetaryValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ApplicationDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SlotsAvailable = table.Column<int>(type: "int", nullable: true),
                    MinimumGPA = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    RequiredCourse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredYearLevel = table.Column<int>(type: "int", nullable: true),
                    RequiredUniversity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalApplicationUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalContactInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalEligibilityDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScrapedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParsingConfidence = table.Column<double>(type: "float(3)", precision: 3, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsEnhanced = table.Column<bool>(type: "bit", nullable: false),
                    RawText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParsingNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedScholarshipId = table.Column<int>(type: "int", nullable: true),
                    AuthorAttribution = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AiModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiPromptVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnhancedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedScholarships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapedScholarships_Scholarships_PublishedScholarshipId",
                        column: x => x.PublishedScholarshipId,
                        principalTable: "Scholarships",
                        principalColumn: "ScholarshipId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ScrapingConfigurations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectorRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExcludePatterns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EnableExternalUrlScraping = table.Column<bool>(type: "bit", nullable: false),
                    RateLimitDelayMs = table.Column<int>(type: "int", nullable: false),
                    MaxRetryAttempts = table.Column<int>(type: "int", nullable: false),
                    CustomPromptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinConfidenceThreshold = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapingConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScrapingProcessLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScrapedScholarshipId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProcessDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapingProcessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapingProcessLogs_ScrapedScholarships_ScrapedScholarshipId",
                        column: x => x.ScrapedScholarshipId,
                        principalTable: "ScrapedScholarships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedScholarships_ParsingConfidence",
                table: "ScrapedScholarships",
                column: "ParsingConfidence");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedScholarships_PublishedScholarshipId",
                table: "ScrapedScholarships",
                column: "PublishedScholarshipId",
                unique: true,
                filter: "[PublishedScholarshipId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedScholarships_ScrapedAt",
                table: "ScrapedScholarships",
                column: "ScrapedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedScholarships_Status",
                table: "ScrapedScholarships",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapingProcessLogs_ProcessedAt",
                table: "ScrapingProcessLogs",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapingProcessLogs_ScrapedScholarshipId",
                table: "ScrapingProcessLogs",
                column: "ScrapedScholarshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkOperationRecords");

            migrationBuilder.DropTable(
                name: "ScrapingConfigurations");

            migrationBuilder.DropTable(
                name: "ScrapingProcessLogs");

            migrationBuilder.DropTable(
                name: "ScrapedScholarships");
        }
    }
}
