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
    }
}
