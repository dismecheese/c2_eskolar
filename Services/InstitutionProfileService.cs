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
    }
}
