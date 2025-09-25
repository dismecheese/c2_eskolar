using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public class StudentProfileService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public StudentProfileService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public async Task<List<StudentProfile>> GetAllProfilesAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.StudentProfiles.ToListAsync();
        }

        public async Task<StudentProfile?> GetProfileByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);
        }

        public async Task SaveProfileAsync(StudentProfile profile, string? identityEmail = null)
        {
            await using var context = _contextFactory.CreateDbContext();
            var existing = await context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == profile.UserId);
            if (existing == null)
            {
                // Ensure required fields are set
                if (string.IsNullOrWhiteSpace(profile.FirstName) || string.IsNullOrWhiteSpace(profile.LastName))
                    throw new ArgumentException("FirstName and LastName are required.");
                // Always set email from Identity if provided
                if (!string.IsNullOrWhiteSpace(identityEmail))
                    profile.Email = identityEmail;
                context.StudentProfiles.Add(profile);
            }
            else
            {
                // Update ALL fields
                existing.FirstName = profile.FirstName;
                existing.MiddleName = profile.MiddleName;
                existing.LastName = profile.LastName;
                existing.Sex = profile.Sex;
                existing.Nationality = profile.Nationality;
                existing.PermanentAddress = profile.PermanentAddress;
                existing.BirthDate = profile.BirthDate;
                existing.MobileNumber = profile.MobileNumber;
                // Always set email from Identity if provided
                if (!string.IsNullOrWhiteSpace(identityEmail))
                    existing.Email = identityEmail;
                else
                    existing.Email = profile.Email;

                existing.UniversityName = profile.UniversityName;
                existing.StudentNumber = profile.StudentNumber;
                existing.Course = profile.Course;
                existing.YearLevel = profile.YearLevel;
                existing.VerificationStatus = profile.VerificationStatus;
                existing.ProfilePicture = profile.ProfilePicture;
                existing.IsVerified = profile.IsVerified;
                existing.VerificationDate = profile.VerificationDate;
                existing.GPA = profile.GPA;
                existing.CreatedAt = profile.CreatedAt;
                existing.UpdatedAt = DateTime.Now;
                // Add any other fields as needed
            }
            await context.SaveChangesAsync();
        }
    }
}
