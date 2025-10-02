using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public class InstitutionProfileService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public InstitutionProfileService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

    public async Task<InstitutionProfile?> GetProfileByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.InstitutionProfiles.FirstOrDefaultAsync(ip => ip.UserId == userId);
        }

        public async Task SaveProfileAsync(InstitutionProfile profile)
        {
            await using var context = _contextFactory.CreateDbContext();
            var existing = await context.InstitutionProfiles.FirstOrDefaultAsync(ip => ip.UserId == profile.UserId);
            if (existing == null)
            {
                if (string.IsNullOrWhiteSpace(profile.AdminFirstName) || string.IsNullOrWhiteSpace(profile.AdminLastName))
                    throw new ArgumentException("AdminFirstName and AdminLastName are required.");
                context.InstitutionProfiles.Add(profile);
            }
            else
            {
                existing.AdminFirstName = profile.AdminFirstName;
                existing.AdminLastName = profile.AdminLastName;
                existing.InstitutionName = profile.InstitutionName;
                existing.Address = profile.Address;
                existing.ContactEmail = profile.ContactEmail;
                existing.ContactNumber = profile.ContactNumber;
                existing.ProfilePicture = profile.ProfilePicture;
                // ...add other fields as needed
            }
            await context.SaveChangesAsync();
        }
    }
}
