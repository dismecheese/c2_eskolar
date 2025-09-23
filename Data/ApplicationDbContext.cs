using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // =====================
    // DbSets
    // =====================

    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<BenefactorProfile> BenefactorProfiles { get; set; }
    public DbSet<InstitutionProfile> InstitutionProfiles { get; set; }
    public DbSet<Scholarship> Scholarships { get; set; }
    public DbSet<ScholarshipApplication> ScholarshipApplications { get; set; }
    public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================
            // Relationships Config
            // =====================

            // ApplicationUser ↔ StudentProfile (1-to-1)
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.StudentProfile)
                .WithOne(p => p.User)
                .HasForeignKey<StudentProfile>(p => p.UserId);

            // ScholarshipApplication ↔ StudentProfile (many-to-1)
            modelBuilder.Entity<ScholarshipApplication>()
                .HasOne(sa => sa.Student)
                .WithMany(s => s.Applications)
                .HasForeignKey(sa => sa.StudentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // ScholarshipApplication ↔ Scholarship (many-to-1)
            modelBuilder.Entity<ScholarshipApplication>()
                .HasOne(sa => sa.Scholarship)
                .WithMany(s => s.Applications)
                .HasForeignKey(sa => sa.ScholarshipId);

            // Scholarship ↔ BenefactorProfile (many-to-1)
            modelBuilder.Entity<Scholarship>()
                .HasOne(s => s.Benefactor)
                .WithMany(b => b.ProvidedScholarships)
                .HasForeignKey(s => s.BenefactorProfileId);

            // Scholarship ↔ InstitutionProfile (many-to-1)
            modelBuilder.Entity<Scholarship>()
                .HasOne(s => s.Institution)
                .WithMany(i => i.ManagedScholarships)
                .HasForeignKey(s => s.InstitutionProfileId);
        }
    }
}