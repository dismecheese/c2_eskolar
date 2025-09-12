using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<ScholarshipApplication>()
                .HasOne(sa => sa.Student)
                .WithMany(s => s.Applications)
                .HasForeignKey(sa => sa.StudentProfileId);

            modelBuilder.Entity<ScholarshipApplication>()
                .HasOne(sa => sa.Scholarship)
                .WithMany(s => s.Applications)
                .HasForeignKey(sa => sa.ScholarshipId);

            modelBuilder.Entity<Scholarship>()
                .HasOne(s => s.Benefactor)
                .WithMany(b => b.ProvidedScholarships)
                .HasForeignKey(s => s.BenefactorProfileId);

            modelBuilder.Entity<Scholarship>()
                .HasOne(s => s.Institution)
                .WithMany(i => i.ManagedScholarships)
                .HasForeignKey(s => s.InstitutionProfileId);
        }
    }
}