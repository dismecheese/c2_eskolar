using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedBookmarkSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookmarkedAnnouncements",
                columns: table => new
                {
                    BookmarkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    AnnouncementId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookmarkedAnnouncements", x => x.BookmarkId);
                    table.ForeignKey(
                        name: "FK_BookmarkedAnnouncements_Announcements_AnnouncementId1",
                        column: x => x.AnnouncementId1,
                        principalTable: "Announcements",
                        principalColumn: "AnnouncementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookmarkedScholarships",
                columns: table => new
                {
                    BookmarkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScholarshipId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookmarkReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MatchScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EnableDeadlineReminders = table.Column<bool>(type: "bit", nullable: false),
                    ReminderDaysBefore = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookmarkedScholarships", x => x.BookmarkId);
                    table.ForeignKey(
                        name: "FK_BookmarkedScholarships_Scholarships_ScholarshipId",
                        column: x => x.ScholarshipId,
                        principalTable: "Scholarships",
                        principalColumn: "ScholarshipId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkedAnnouncements_AnnouncementId1",
                table: "BookmarkedAnnouncements",
                column: "AnnouncementId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkedScholarships_ScholarshipId",
                table: "BookmarkedScholarships",
                column: "ScholarshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookmarkedAnnouncements");

            migrationBuilder.DropTable(
                name: "BookmarkedScholarships");
        }
    }
}
