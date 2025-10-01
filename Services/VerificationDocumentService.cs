using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace c2_eskolar.Services
{
    public class VerificationDocumentService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public VerificationDocumentService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

    public async Task<List<VerificationDocument>> GetDocumentsByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.VerificationDocuments
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }
    }
}
