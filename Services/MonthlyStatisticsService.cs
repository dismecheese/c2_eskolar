using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace c2_eskolar.Services
{
    /// <summary>
    /// Service for managing monthly statistics aggregation and retrieval
    /// </summary>
    public class MonthlyStatisticsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<MonthlyStatisticsService> _logger;

        public MonthlyStatisticsService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<MonthlyStatisticsService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Aggregates data for a specific month and saves to database
        /// </summary>
        public async Task<MonthlyStatistics> AggregateMonthlyDataAsync(int year, int month)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            try
            {
                _logger.LogInformation($"Starting monthly aggregation for {year}-{month:D2}");

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                // Check if record already exists
                var existing = await context.MonthlyStatistics
                    .FirstOrDefaultAsync(ms => ms.Year == year && ms.Month == month);

                if (existing != null)
                {
                    _logger.LogInformation($"Updating existing record for {year}-{month:D2}");
                    // Update existing record
                    await PopulateStatistics(context, existing, startDate, endDate);
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _logger.LogInformation($"Creating new record for {year}-{month:D2}");
                    // Create new record
                    existing = new MonthlyStatistics
                    {
                        Year = year,
                        Month = month,
                        CreatedAt = DateTime.UtcNow
                    };
                    await PopulateStatistics(context, existing, startDate, endDate);
                    context.MonthlyStatistics.Add(existing);
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Successfully aggregated data for {year}-{month:D2}");
                
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error aggregating monthly data for {year}-{month:D2}");
                throw;
            }
        }

        /// <summary>
        /// Populates a MonthlyStatistics object with aggregated data
        /// </summary>
        private async Task PopulateStatistics(
            ApplicationDbContext context,
            MonthlyStatistics stats,
            DateTime startDate,
            DateTime endDate)
        {
            // Application Statistics
            var applications = await context.ScholarshipApplications
                .Where(sa => sa.ApplicationDate >= startDate && sa.ApplicationDate < endDate)
                .ToListAsync();

            stats.TotalApplications = applications.Count;
            stats.AcceptedApplications = applications.Count(a => a.Status == "Accepted");
            stats.PendingApplications = applications.Count(a => a.Status == "Pending");
            stats.RejectedApplications = applications.Count(a => a.Status == "Rejected");

            // User Statistics (total at end of month)
            var allStudents = await context.StudentProfiles.CountAsync(sp => sp.CreatedAt < endDate);
            var allBenefactors = await context.BenefactorProfiles.CountAsync(bp => bp.CreatedAt < endDate);
            var allInstitutions = await context.InstitutionProfiles.CountAsync(ip => ip.CreatedAt < endDate);

            stats.TotalStudents = allStudents;
            stats.TotalBenefactors = allBenefactors;
            stats.TotalInstitutions = allInstitutions;
            stats.TotalUsers = allStudents + allBenefactors + allInstitutions;

            // New users in this month
            stats.NewUsers = await context.StudentProfiles
                .CountAsync(sp => sp.CreatedAt >= startDate && sp.CreatedAt < endDate)
                + await context.BenefactorProfiles
                .CountAsync(bp => bp.CreatedAt >= startDate && bp.CreatedAt < endDate)
                + await context.InstitutionProfiles
                .CountAsync(ip => ip.CreatedAt >= startDate && ip.CreatedAt < endDate);

            // Verified users (at end of month)
            stats.VerifiedUsers = await context.StudentProfiles
                .CountAsync(sp => sp.CreatedAt < endDate && sp.AccountStatus == "Verified")
                + await context.BenefactorProfiles
                .CountAsync(bp => bp.CreatedAt < endDate && bp.AccountStatus == "Verified")
                + await context.InstitutionProfiles
                .CountAsync(ip => ip.CreatedAt < endDate && ip.AccountStatus == "Verified");

            // Scholarship Statistics (active at end of month)
            stats.TotalScholarships = await context.Scholarships
                .CountAsync(s => s.CreatedAt < endDate);
            
            stats.ActiveScholarships = await context.Scholarships
                .CountAsync(s => s.CreatedAt < endDate && s.ApplicationDeadline >= endDate);

            // Financial Statistics
            var scholarships = await context.Scholarships
                .Where(s => s.CreatedAt < endDate)
                .ToListAsync();

            stats.TotalScholarshipValue = scholarships.Any() ? scholarships.Sum(s => s.MonetaryValue ?? 0m) : 0m;
            
            var acceptedApps = await context.ScholarshipApplications
                .Where(sa => sa.Status == "Accepted" && sa.ApplicationDate < endDate)
                .Include(sa => sa.Scholarship)
                .ToListAsync();
            
            stats.DistributedValue = acceptedApps.Any() ? acceptedApps.Sum(a => a.Scholarship?.MonetaryValue ?? 0m) : 0m;

            // Performance Metrics
            if (stats.TotalApplications > 0)
            {
                var benefactorApps = applications
                    .Where(a => a.Scholarship?.BenefactorProfileId != null);
                stats.BenefactorSuccessRate = benefactorApps.Any() 
                    ? (double)benefactorApps.Count(a => a.Status == "Accepted") / benefactorApps.Count() * 100 
                    : 0;

                var institutionApps = applications
                    .Where(a => a.Scholarship?.InstitutionProfileId != null);
                stats.InstitutionSuccessRate = institutionApps.Any() 
                    ? (double)institutionApps.Count(a => a.Status == "Accepted") / institutionApps.Count() * 100 
                    : 0;

                var processedApps = applications
                    .Where(a => a.Status != "Pending" && a.UpdatedAt.HasValue);
                stats.AverageProcessingDays = processedApps.Any()
                    ? processedApps.Average(a => (a.UpdatedAt!.Value - a.ApplicationDate).TotalDays)
                    : 0;
            }
        }

        /// <summary>
        /// Gets monthly statistics for a specific period
        /// </summary>
        public async Task<List<MonthlyStatistics>> GetMonthlyStatisticsAsync(
            int monthsBack = 6,
            int? specificYear = null,
            int? specificMonth = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            if (specificYear.HasValue && specificMonth.HasValue)
            {
                // Get specific month
                var stat = await context.MonthlyStatistics
                    .FirstOrDefaultAsync(ms => ms.Year == specificYear && ms.Month == specificMonth);
                return stat != null ? new List<MonthlyStatistics> { stat } : new List<MonthlyStatistics>();
            }
            else
            {
                // Get last N months
                var cutoffDate = DateTime.Now.AddMonths(-monthsBack);
                return await context.MonthlyStatistics
                    .Where(ms => new DateTime(ms.Year, ms.Month, 1) >= cutoffDate)
                    .OrderBy(ms => ms.Year)
                    .ThenBy(ms => ms.Month)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Aggregates data for the previous month (called by background job)
        /// </summary>
        public async Task AggregatePreviousMonthAsync()
        {
            var lastMonth = DateTime.Now.AddMonths(-1);
            await AggregateMonthlyDataAsync(lastMonth.Year, lastMonth.Month);
        }

        /// <summary>
        /// Backfills historical data for all months with application data
        /// </summary>
        public async Task BackfillHistoricalDataAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                _logger.LogInformation("Starting historical data backfill");

                // Find the earliest application date
                var earliestApp = await context.ScholarshipApplications
                    .OrderBy(sa => sa.ApplicationDate)
                    .FirstOrDefaultAsync();

                if (earliestApp == null)
                {
                    _logger.LogWarning("No applications found, skipping backfill");
                    return;
                }

                var startDate = new DateTime(earliestApp.ApplicationDate.Year, earliestApp.ApplicationDate.Month, 1);
                var currentDate = DateTime.Now;
                var monthsToProcess = new List<(int Year, int Month)>();

                // Generate list of months to process
                var date = startDate;
                while (date < currentDate)
                {
                    monthsToProcess.Add((date.Year, date.Month));
                    date = date.AddMonths(1);
                }

                _logger.LogInformation($"Backfilling {monthsToProcess.Count} months of historical data");

                // Process each month
                foreach (var (year, month) in monthsToProcess)
                {
                    await AggregateMonthlyDataAsync(year, month);
                }

                _logger.LogInformation("Historical data backfill completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during historical data backfill");
                throw;
            }
        }
    }
}
