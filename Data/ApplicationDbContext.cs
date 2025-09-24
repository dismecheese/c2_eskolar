
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
        public DbSet<ScholarshipType> ScholarshipTypes { get; set; }
        public DbSet<VerificationDocument> VerificationDocuments { get; set; }
        public DbSet<RecentlyViewedScholarship> RecentlyViewedScholarships { get; set; }
        public DbSet<InstitutionBenefactorPartnership> InstitutionBenefactorPartnerships { get; set; }
        public DbSet<InstitutionAdminProfile> InstitutionAdminProfiles { get; set; }
        public DbSet<BenefactorAdminProfile> BenefactorAdminProfiles { get; set; }

        // MODEL CONFIGURATION & RELATIONSHIPS
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // VerificationDocument: User 1-to-many
            modelBuilder.Entity<VerificationDocument>()
                .HasOne(v => v.User)
                .WithMany(u => u.VerificationDocuments)
                .HasForeignKey(v => v.UserId);

            // RecentlyViewedScholarship: Student (User) 1-to-many, Scholarship 1-to-many
            modelBuilder.Entity<RecentlyViewedScholarship>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
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

            // InstitutionAdminProfile: User 1-to-many, Institution 1-to-many
            modelBuilder.Entity<InstitutionAdminProfile>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);
            modelBuilder.Entity<InstitutionAdminProfile>()
                .HasOne(a => a.Institution)
                .WithMany(i => i.AdminProfiles)
                .HasForeignKey(a => a.InstitutionId);

            // BenefactorAdminProfile: User 1-to-many, Benefactor 1-to-many
            modelBuilder.Entity<BenefactorAdminProfile>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);
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
        }
    }
}
