using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for your eSkolar models
        // DbSets for consolidated schema
        public new DbSet<Role> Roles { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<BenefactorProfile> BenefactorProfiles { get; set; }
        public DbSet<InstitutionProfile> InstitutionProfiles { get; set; }
        public DbSet<Scholarship> Scholarships { get; set; }
        public DbSet<ScholarshipApplication> ScholarshipApplications { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<VerificationDocument> VerificationDocuments { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Benefactor> Benefactors { get; set; }
        public DbSet<InstitutionAdminProfile> InstitutionAdminProfiles { get; set; }
        public DbSet<BenefactorAdminProfile> BenefactorAdminProfiles { get; set; }
        public DbSet<InstitutionBenefactorPartnership> InstitutionBenefactorPartnerships { get; set; }
        public DbSet<ScholarshipType> ScholarshipTypes { get; set; }
        public DbSet<ScholarshipEligibility> ScholarshipEligibilities { get; set; }
        public DbSet<ScholarshipGrant> ScholarshipGrants { get; set; }
        public DbSet<RecentlyViewedScholarship> RecentlyViewedScholarships { get; set; }
        public DbSet<AnnouncementRecipient> AnnouncementRecipients { get; set; }
        public DbSet<BookmarkedScholarship> BookmarkedScholarships { get; set; }
        public DbSet<BookmarkedAnnouncement> BookmarkedAnnouncements { get; set; }
        public DbSet<ApplicationReview> ApplicationReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
                // Add relationships for new models as needed
                // Example: Role-User
                modelBuilder.Entity<User>()
                    .HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId);

                // Example: User-VerificationDocument
                modelBuilder.Entity<VerificationDocument>()
                    .HasOne(v => v.User)
                    .WithMany(u => u.VerificationDocuments)
                    .HasForeignKey(v => v.UserId);

                // Example: InstitutionAdminProfile-Institution
                modelBuilder.Entity<InstitutionAdminProfile>()
                    .HasOne(iap => iap.Institution)
                    .WithMany(i => i.AdminProfiles)
                    .HasForeignKey(iap => iap.InstitutionId);

                // Example: BenefactorAdminProfile-Benefactor
                modelBuilder.Entity<BenefactorAdminProfile>()
                    .HasOne(bap => bap.Benefactor)
                    .WithMany(b => b.AdminProfiles)
                    .HasForeignKey(bap => bap.BenefactorId);

                // Example: InstitutionBenefactorPartnership
                modelBuilder.Entity<InstitutionBenefactorPartnership>()
                    .HasOne(p => p.Institution)
                    .WithMany(i => i.Partnerships)
                    .HasForeignKey(p => p.InstitutionId);
                modelBuilder.Entity<InstitutionBenefactorPartnership>()
                    .HasOne(p => p.Benefactor)
                    .WithMany(b => b.Partnerships)
                    .HasForeignKey(p => p.BenefactorId);

                // Example: ScholarshipType-Scholarship
                modelBuilder.Entity<Scholarship>()
                    .HasOne<ScholarshipType>()
                    .WithMany(st => st.Scholarships)
                    .HasForeignKey(s => s.ScholarshipTypeId);

                // Example: ScholarshipEligibility-Scholarship
                modelBuilder.Entity<ScholarshipEligibility>()
                    .HasOne(se => se.Scholarship)
                    .WithMany()
                    .HasForeignKey(se => se.ScholarshipId);

                // Example: ScholarshipGrant-Scholarship/User
                modelBuilder.Entity<ScholarshipGrant>()
                    .HasOne(sg => sg.Scholarship)
                    .WithMany()
                    .HasForeignKey(sg => sg.ScholarshipId);
                modelBuilder.Entity<ScholarshipGrant>()
                    .HasOne(sg => sg.Student)
                    .WithMany()
                    .HasForeignKey(sg => sg.StudentId);

                // Example: RecentlyViewedScholarship
                modelBuilder.Entity<RecentlyViewedScholarship>()
                    .HasOne(rv => rv.Student)
                    .WithMany()
                    .HasForeignKey(rv => rv.StudentId);
                modelBuilder.Entity<RecentlyViewedScholarship>()
                    .HasOne(rv => rv.Scholarship)
                    .WithMany()
                    .HasForeignKey(rv => rv.ScholarshipId);

                // Example: AnnouncementRecipient
                modelBuilder.Entity<AnnouncementRecipient>()
                    .HasOne(ar => ar.Announcement)
                    .WithMany()
                    .HasForeignKey(ar => ar.AnnouncementId);
                modelBuilder.Entity<AnnouncementRecipient>()
                    .HasOne(ar => ar.User)
                    .WithMany()
                    .HasForeignKey(ar => ar.UserId);

                // Example: BookmarkedScholarship
                modelBuilder.Entity<BookmarkedScholarship>()
                    .HasOne(bs => bs.User)
                    .WithMany()
                    .HasForeignKey(bs => bs.UserId);
                modelBuilder.Entity<BookmarkedScholarship>()
                    .HasOne(bs => bs.Scholarship)
                    .WithMany()
                    .HasForeignKey(bs => bs.ScholarshipId);

                // Example: BookmarkedAnnouncement
                modelBuilder.Entity<BookmarkedAnnouncement>()
                    .HasOne(ba => ba.User)
                    .WithMany()
                    .HasForeignKey(ba => ba.UserId);
                modelBuilder.Entity<BookmarkedAnnouncement>()
                    .HasOne(ba => ba.Announcement)
                    .WithMany()
                    .HasForeignKey(ba => ba.AnnouncementId);

                // Example: ApplicationReview
                modelBuilder.Entity<ApplicationReview>()
                    .HasOne(ar => ar.Application)
                    .WithMany()
                    .HasForeignKey(ar => ar.ApplicationId);
                modelBuilder.Entity<ApplicationReview>()
                    .HasOne(ar => ar.Reviewer)
                    .WithMany()
                    .HasForeignKey(ar => ar.ReviewerUserId);
        }
    }
}