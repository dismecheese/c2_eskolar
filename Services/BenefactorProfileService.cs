using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public class BenefactorProfileService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BenefactorProfileService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<BenefactorProfile?> GetProfileByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.BenefactorProfiles.FirstOrDefaultAsync(bp => bp.UserId == userId);
        }

        public async Task SaveProfileAsync(BenefactorProfile profile)
        {
            await using var context = _contextFactory.CreateDbContext();
            var existing = await context.BenefactorProfiles.FirstOrDefaultAsync(bp => bp.UserId == profile.UserId);
            if (existing == null)
            {
                if (string.IsNullOrWhiteSpace(profile.AdminFirstName) || string.IsNullOrWhiteSpace(profile.AdminLastName))
                    throw new ArgumentException("AdminFirstName and AdminLastName are required.");
                context.BenefactorProfiles.Add(profile);
            }
            else
            {
                existing.AdminFirstName = profile.AdminFirstName;
                existing.AdminLastName = profile.AdminLastName;
                existing.OrganizationName = profile.OrganizationName;
                existing.Address = profile.Address;
                existing.ContactEmail = profile.ContactEmail;
                existing.ContactNumber = profile.ContactNumber;
                // ...add other fields as needed
            }
            await context.SaveChangesAsync();
        }
    }
}
