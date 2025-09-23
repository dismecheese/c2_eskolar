
using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Data
{
    // APPLICATION DATABASE CONTEXT
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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

        // MODEL CONFIGURATION & RELATIONSHIPS
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call base method to apply Identity model configurations
            base.OnModelCreating(modelBuilder);

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
