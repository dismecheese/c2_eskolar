using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    // SERVICE TO SEED SAMPLE ANNOUNCEMENTS INTO DATABASE
    public class AnnouncementSeedService
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementSeedService(ApplicationDbContext context)
        {
            _context = context;
        }

        // SEEDS DEMO ANNOUNCEMENTS IF NONE EXIST
        public async Task SeedSampleAnnouncementsAsync()
        {
            // Check if we already have announcements
            if (await _context.Announcements.AnyAsync())
            {
                return; // Skip seeding if data already present
            }

            // CREATE SAMPLE ANNOUNCEMENTS LIST
            var sampleAnnouncements = new List<Announcement>
            {
                // INSTITUTION ANNOUNCEMENTS
                new Announcement
                {
                    Title = "Welcome to the New Academic Year 2025!",
                    Content = "We are excited to welcome all students to the new academic year. Please check your enrollment status and course schedules. Orientation begins next week.",
                    Summary = "Academic year 2025 welcome message and orientation information",
                    AuthorId = "institution-001",
                    AuthorName = "University of Technology",
                    AuthorType = UserRole.Institution,
                    Category = "General",
                    Priority = AnnouncementPriority.High,
                    IsPublic = true,
                    IsActive = true,
                    IsPinned = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Announcement
                {
                    Title = "Library Hours Extended During Finals Week",
                    Content = "The university library will be open 24/7 during finals week (December 10-17) to support students during their examination period. Study rooms are available for booking.",
                    Summary = "Extended library hours during finals week",
                    AuthorId = "institution-001",
                    AuthorName = "University of Technology",
                    AuthorType = UserRole.Institution,
                    Category = "General",
                    Priority = AnnouncementPriority.Normal,
                    IsPublic = true,
                    IsActive = true,
                    IsPinned = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },

                // BENEFACTOR ANNOUNCEMENTS
                new Announcement
                {
                    Title = "Tech Excellence Scholarship Application Deadline Extended",
                    Content = "Due to high interest, we are extending the application deadline for the Tech Excellence Scholarship until January 15, 2025. This scholarship provides full tuition coverage for outstanding students in STEM fields.",
                    Summary = "Tech Excellence Scholarship deadline extension",
                    AuthorId = "benefactor-001",
                    AuthorName = "Tech Innovation Foundation",
                    AuthorType = UserRole.Benefactor,
                    Category = "Applications",
                    Priority = AnnouncementPriority.Urgent,
                    IsPublic = true,
                    IsActive = true,
                    IsPinned = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Announcement
                {
                    Title = "Academic Merit Scholarship Recipients - Congratulations!",
                    Content = "We are pleased to announce the recipients of the Academic Merit Scholarship for 2025. Congratulations to all winners! Your scholarship benefits will begin next semester. Please check your email for detailed information about fund disbursement.",
                    Summary = "Academic Merit Scholarship recipients announced",
                    AuthorId = "benefactor-002",
                    AuthorName = "Community Education Fund",
                    AuthorType = UserRole.Benefactor,
                    Category = "Funding",
                    Priority = AnnouncementPriority.High,
                    IsPublic = true,
                    IsActive = true,
                    IsPinned = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Announcement
                {
                    Title = "STEM Leadership Grant - Mid-term Report Due",
                    Content = "Attention STEM Leadership Grant recipients: Your mid-term progress reports are due by January 30, 2025. Please submit your report through the scholarship portal. Include your academic progress, community involvement, and research activities.",
                    Summary = "STEM Leadership Grant mid-term report deadline",
                    AuthorId = "benefactor-001",
                    AuthorName = "Tech Innovation Foundation",
                    AuthorType = UserRole.Benefactor,
                    Category = "Applications",
                    Priority = AnnouncementPriority.Normal,
                    IsPublic = true,
                    IsActive = true,
                    IsPinned = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-12),
                    UpdatedAt = DateTime.UtcNow.AddHours(-12)
                },

                // MORE INSTITUTION ANNOUNCEMENTS
                new Announcement
                {
                    Title = "Campus WiFi Maintenance Scheduled",
                    Content = "Campus-wide WiFi maintenance is scheduled for this Saturday from 2 AM to 6 AM. Internet services may be intermittent during this time. We apologize for any inconvenience.",
                    Summary = "Scheduled WiFi maintenance this Saturday",
                    AuthorId = "institution-001",
                    AuthorName = "University of Technology",
                    AuthorType = UserRole.Institution,
                    Category = "General",
                    Priority = AnnouncementPriority.Normal,
                    IsPublic = true,
                    IsActive = true,
                    IsPinned = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    UpdatedAt = DateTime.UtcNow.AddHours(-6)
                },

                // COMMUNITY SERVICE RELATED
                new Announcement
                {
                    Title = "Community Service Award - Application Open",
                    Content = "The Community Service Award is now accepting applications for outstanding students who have demonstrated exceptional commitment to community service. Award includes $2,000 and recognition at graduation.",
                    Summary = "Community Service Award applications now open",
                    AuthorId = "benefactor-003",
                    AuthorName = "Civic Engagement Foundation",
                    AuthorType = UserRole.Benefactor,
                    Category = "Funding",
                    Priority = AnnouncementPriority.Normal,
                    IsPublic = true,
                    IsActive = true,
                    IsPinned = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    UpdatedAt = DateTime.UtcNow.AddHours(-3)
                }
            };

            // SAVE TO DATABASE
            _context.Announcements.AddRange(sampleAnnouncements);
            await _context.SaveChangesAsync();
        }
    }
}
