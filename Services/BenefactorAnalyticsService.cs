using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace c2_eskolar.Services
{
    public class BenefactorAnalyticsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly OpenAIService _openAIService;

        public BenefactorAnalyticsService(IDbContextFactory<ApplicationDbContext> contextFactory, OpenAIService openAIService)
        {
            _contextFactory = contextFactory;
            _openAIService = openAIService;
        }

        // Get analytics for a benefactor by userId
        public async Task<BenefactorAnalyticsResult> GetAnalyticsAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var benefactor = await context.BenefactorProfiles.FirstOrDefaultAsync(bp => bp.UserId == userId);
            
            var result = new BenefactorAnalyticsResult();
            
            if (benefactor == null)
            {
                result.DebugInfo = "No benefactor profile found for this user.";
                return result;
            }

            // Get all scholarships for this benefactor
            var scholarships = await context.Scholarships
                .Where(s => s.BenefactorProfileId == benefactor.BenefactorProfileId)
                .Include(s => s.Applications)
                .ToListAsync();

            result.DebugInfo = $"Found {scholarships.Count} scholarships for benefactor {benefactor.BenefactorProfileId}";

            // Get all applications directly from ScholarshipApplications table for this benefactor's scholarships
            var scholarshipIds = scholarships.Select(s => s.ScholarshipId).ToList();
            var allApplications = await context.ScholarshipApplications
                .Where(sa => scholarshipIds.Contains(sa.ScholarshipId))
                .Include(sa => sa.Scholarship)
                .ToListAsync();

            result.DebugInfo += $" | Total applications: {allApplications.Count}";

            // Process scholarship applicant counts
            foreach (var scholarship in scholarships)
            {
                var scholarshipApplications = allApplications.Where(a => a.ScholarshipId == scholarship.ScholarshipId).ToList();
                result.ScholarshipApplicantCounts[scholarship.Title] = scholarshipApplications.Count;
                
                var appCounts = scholarshipApplications
                    .GroupBy(a => a.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
                result.ScholarshipStatusCounts[scholarship.Title] = appCounts;
            }

            // Aggregate status counts overall
            result.TotalStatusCounts = allApplications
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Application trends (monthly)
            result.MonthlyApplicationTrends = allApplications
                .GroupBy(a => new { a.ApplicationDate.Year, a.ApplicationDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month:00}",
                    g => g.Count()
                );

            // Application trends (semesterly, assuming 1st: Jan-Jun, 2nd: Jul-Dec)
            result.SemesterlyApplicationTrends = allApplications
                .GroupBy(a => new { a.ApplicationDate.Year, Semester = a.ApplicationDate.Month <= 6 ? 1 : 2 })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Semester)
                .ToDictionary(
                    g => $"{g.Key.Year}-S{g.Key.Semester}",
                    g => g.Count()
                );

            // AI-powered summary (only if there's data)
            if (allApplications.Any())
            {
                result.AISummary = await GetAISummaryAsync(result);
            }
            else
            {
                result.AISummary = "No scholarship applications found for analysis. Create some scholarships and wait for student applications to see analytics here.";
            }

            return result;
        }

        // Use OpenAI to analyze and summarize the analytics data
        private async Task<string> GetAISummaryAsync(BenefactorAnalyticsResult analytics)
        {
            var prompt = $@"Analyze the following scholarship application analytics and provide a summary with confidence insights, trends, and any anomalies detected.
            
            Total Applicants per Scholarship: {string.Join(", ", analytics.ScholarshipApplicantCounts.Select(kv => $"{kv.Key}: {kv.Value}"))}
            Application Status Counts: {string.Join(", ", analytics.TotalStatusCounts.Select(kv => $"{kv.Key}: {kv.Value}"))}
            Monthly Application Trends: {string.Join(", ", analytics.MonthlyApplicationTrends.Select(kv => $"{kv.Key}: {kv.Value}"))}
            Semesterly Application Trends: {string.Join(", ", analytics.SemesterlyApplicationTrends.Select(kv => $"{kv.Key}: {kv.Value}"))}
            
            Give actionable insights and highlight anything unusual.";
            try
            {
                return await _openAIService.GetChatCompletionAsync(prompt);
            }
            catch
            {
                return "AI summary unavailable.";
            }
        }
    }

    public class BenefactorAnalyticsResult
    {
        public Dictionary<string, int> ScholarshipApplicantCounts { get; set; } = new();
        public Dictionary<string, Dictionary<string, int>> ScholarshipStatusCounts { get; set; } = new();
        public Dictionary<string, int> TotalStatusCounts { get; set; } = new();
        public Dictionary<string, int> MonthlyApplicationTrends { get; set; } = new();
        public Dictionary<string, int> SemesterlyApplicationTrends { get; set; } = new();
        public string AISummary { get; set; } = string.Empty;
        public string DebugInfo { get; set; } = string.Empty;
    }
}
