
using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Data
{
    // APPLICATION DATABASE CONTEXT
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        // Constructor: passes DbContext options to the base class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

    // DbSets for your eSkolar models
    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<BenefactorProfile> BenefactorProfiles { get; set; }
    public DbSet<InstitutionProfile> InstitutionProfiles { get; set; }
    public DbSet<Scholarship> Scholarships { get; set; }
    public DbSet<ScholarshipApplication> ScholarshipApplications { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<PasswordReset> PasswordResets { get; set; }
    public DbSet<ScholarshipType> ScholarshipTypes { get; set; }
    public DbSet<VerificationDocument> VerificationDocuments { get; set; }
    public DbSet<RecentlyViewedScholarship> RecentlyViewedScholarships { get; set; }
    public DbSet<InstitutionBenefactorPartnership> InstitutionBenefactorPartnerships { get; set; }
    public DbSet<InstitutionAdminProfile> InstitutionAdminProfiles { get; set; }
    public DbSet<BenefactorAdminProfile> BenefactorAdminProfiles { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Photo> Photos { get; set; }
    // Notifications
    public DbSet<c2_eskolar.Models.Notification> Notifications { get; set; }
    
    // Bookmark system
    public DbSet<BookmarkedScholarship> BookmarkedScholarships { get; set; }
    public DbSet<BookmarkedAnnouncement> BookmarkedAnnouncements { get; set; }
    
    // Scraped scholarship management with EskoBot Intelligence
    public DbSet<ScrapedScholarship> ScrapedScholarships { get; set; }
    public DbSet<ScrapingProcessLog> ScrapingProcessLogs { get; set; }
    public DbSet<ScrapingConfiguration> ScrapingConfigurations { get; set; }
    public DbSet<BulkOperationRecord> BulkOperationRecords { get; set; }
    
    // AI Token Usage Tracking
    public DbSet<AITokenUsage> AITokenUsages { get; set; }
    
    // Monthly Statistics for Historical Analytics
    public DbSet<MonthlyStatistics> MonthlyStatistics { get; set; }

        // MODEL CONFIGURATION & RELATIONSHIPS
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        // Document relationship
        modelBuilder.Entity<Document>()
            .HasOne(d => d.ScholarshipApplication)
            .WithMany(sa => sa.Documents)
            .HasForeignKey(d => d.ScholarshipApplicationId);

        // Photo relationships
        modelBuilder.Entity<Photo>()
            .HasOne(p => p.Scholarship)
            .WithMany(s => s.Photos)
            .HasForeignKey(p => p.ScholarshipId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Photo>()
            .HasOne(p => p.Announcement)
            .WithMany(a => a.Photos)
            .HasForeignKey(p => p.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Call base method to apply Identity model configurations
        base.OnModelCreating(modelBuilder);
            base.OnModelCreating(modelBuilder);

            // Configure profile relationships with IdentityUser
            modelBuilder.Entity<StudentProfile>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<StudentProfile>(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // All custom User mappings removed. Only IdentityUser is used.

            modelBuilder.Entity<BenefactorProfile>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(bp => bp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InstitutionProfile>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(ip => ip.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal properties for SQL Server
            modelBuilder.Entity<StudentProfile>()
                .Property(s => s.GPA)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Scholarship>()
                .Property(s => s.MinimumGPA)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Scholarship>()
                .Property(s => s.MonetaryValue)
                .HasPrecision(18, 2);

            // VerificationDocument: removed navigation property to prevent UserId1 shadow property

            // RecentlyViewedScholarship: Scholarship 1-to-many only
            modelBuilder.Entity<RecentlyViewedScholarship>()
                .HasOne(r => r.Scholarship)
                .WithMany()
                .HasForeignKey(r => r.ScholarshipId)
                .OnDelete(DeleteBehavior.Restrict);

            // InstitutionBenefactorPartnership: Institution 1-to-many, Benefactor 1-to-many
            modelBuilder.Entity<InstitutionBenefactorPartnership>()
                .HasOne(p => p.Institution)
                .WithMany(i => i.Partnerships)
                .HasForeignKey(p => p.InstitutionId);
            modelBuilder.Entity<InstitutionBenefactorPartnership>()
                .HasOne(p => p.Benefactor)
                .WithMany(b => b.Partnerships)
                .HasForeignKey(p => p.BenefactorId);

            // InstitutionAdminProfile: Institution 1-to-many
            modelBuilder.Entity<InstitutionAdminProfile>()
                .HasOne(a => a.Institution)
                .WithMany(i => i.AdminProfiles)
                .HasForeignKey(a => a.InstitutionId);

            // BenefactorAdminProfile: Benefactor 1-to-many
            modelBuilder.Entity<BenefactorAdminProfile>()
                .HasOne(a => a.Benefactor)
                .WithMany(b => b.AdminProfiles)
                .HasForeignKey(a => a.BenefactorId);

            // A scholarship application belongs to one student, student has many applications
            modelBuilder.Entity<ScholarshipApplication>()
                .HasOne(sa => sa.Student)
                .WithMany(s => s.Applications)
                .HasForeignKey(sa => sa.StudentProfileId);

            // A scholarship application belongs to one scholarship, scholarship has many applications
            modelBuilder.Entity<ScholarshipApplication>()
                .HasOne(sa => sa.Scholarship)
                .WithMany(s => s.Applications)
                .HasForeignKey(sa => sa.ScholarshipId);

            // A scholarship is provided by one benefactor, benefactor has many scholarships
            modelBuilder.Entity<Scholarship>()
                .HasOne(s => s.Benefactor)
                .WithMany(b => b.ProvidedScholarships)
                .HasForeignKey(s => s.BenefactorProfileId);

            // A scholarship is managed by one institution, institution has many scholarships
            modelBuilder.Entity<Scholarship>()
                .HasOne(s => s.Institution)
                .WithMany(i => i.ManagedScholarships)
                .HasForeignKey(s => s.InstitutionProfileId);

            // Scraped scholarship configurations
            modelBuilder.Entity<ScrapedScholarship>()
                .Property(s => s.ParsingConfidence)
                .HasPrecision(3, 2);

            modelBuilder.Entity<ScrapedScholarship>()
                .Property(s => s.MonetaryValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ScrapedScholarship>()
                .Property(s => s.MinimumGPA)
                .HasPrecision(3, 2);

            // Scraped scholarship to published scholarship relationship
            modelBuilder.Entity<ScrapedScholarship>()
                .HasOne(ss => ss.PublishedScholarship)
                .WithOne()
                .HasForeignKey<ScrapedScholarship>(ss => ss.PublishedScholarshipId)
                .OnDelete(DeleteBehavior.SetNull);

            // Scraping process logs relationship
            modelBuilder.Entity<ScrapingProcessLog>()
                .HasOne(spl => spl.ScrapedScholarship)
                .WithMany(ss => ss.ProcessingLogs)
                .HasForeignKey(spl => spl.ScrapedScholarshipId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes for performance
            modelBuilder.Entity<ScrapedScholarship>()
                .HasIndex(s => s.Status);

            modelBuilder.Entity<ScrapedScholarship>()
                .HasIndex(s => s.ScrapedAt);

            modelBuilder.Entity<ScrapedScholarship>()
                .HasIndex(s => s.ParsingConfidence);

            modelBuilder.Entity<ScrapingProcessLog>()
                .HasIndex(spl => spl.ProcessedAt);

            // Configure MonthlyStatistics with unique constraint on Year/Month combination
            modelBuilder.Entity<MonthlyStatistics>()
                .HasIndex(ms => new { ms.Year, ms.Month })
                .IsUnique();
        }
    }
}
