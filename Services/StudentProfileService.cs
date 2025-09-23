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

        public async Task<StudentProfile?> GetProfileByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);
        }

        public async Task SaveProfileAsync(StudentProfile profile)
        {
            await using var context = _contextFactory.CreateDbContext();
            var existing = await context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == profile.UserId);
            if (existing == null)
            {
                // Ensure required fields are set
                if (string.IsNullOrWhiteSpace(profile.FirstName) || string.IsNullOrWhiteSpace(profile.LastName))
                    throw new ArgumentException("FirstName and LastName are required.");
                context.StudentProfiles.Add(profile);
            }
            else
            {
                // Update fields
                existing.FirstName = profile.FirstName;
                existing.MiddleName = profile.MiddleName;
                existing.LastName = profile.LastName;
                existing.Sex = profile.Sex;
                existing.Nationality = profile.Nationality;
                existing.PermanentAddress = profile.PermanentAddress;
                existing.BirthDate = profile.BirthDate;
                existing.MobileNumber = profile.MobileNumber;
                existing.Email = profile.Email;
                // ...add other fields as needed
            }
            await context.SaveChangesAsync();
        }
    }
}
