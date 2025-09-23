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
    }
}
