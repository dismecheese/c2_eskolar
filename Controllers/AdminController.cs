using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using c2_eskolar.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using c2_eskolar.Services;

namespace c2_eskolar.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogger<AdminController> _logger;
        private readonly BlobStorageService _blobService;

        public AdminController(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<AdminController> logger, BlobStorageService blobService)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _blobService = blobService;
        }

        [HttpGet("system-report")]
        public async Task<IActionResult> GetSystemReport()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();

                var totalUsers = await db.StudentProfiles.CountAsync() + await db.InstitutionProfiles.CountAsync();
                var totalScholarships = await db.ScrapedScholarships.CountAsync();
                var applicationsToday = await db.ScholarshipApplications
                    .Where(a => a.ApplicationDate >= DateTime.Today && a.ApplicationDate < DateTime.Today.AddDays(1))
                    .CountAsync();

                var tokenUsage = await db.AITokenUsages.SumAsync(t => t.PromptTokens + t.CompletionTokens);
                var monthlyCost = await db.AITokenUsages.Where(t => t.CreatedAt >= DateTime.Now.AddDays(-30)).SumAsync(t => t.EstimatedCost);

                var photosCount = await _blobService.GetPhotosCountAsync();
                var docsCount = await _blobService.GetDocumentsCountAsync();

                var report = new
                {
                    ExportDate = DateTime.Now,
                    TotalUsers = totalUsers,
                    TotalScholarships = totalScholarships,
                    ApplicationsToday = applicationsToday,
                    TokenUsage = tokenUsage,
                    MonthlyCost = monthlyCost,
                    PhotosCount = photosCount,
                    DocumentsCount = docsCount
                };

                var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                var bytes = Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", $"system-report-{DateTime.Now:yyyy-MM-dd-HH-mm}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate system report");
                return StatusCode(500, "Failed to generate report");
            }
        }

        [HttpGet("recent-logs")]
        public IActionResult GetRecentLogs()
        {
            // Simple placeholder: return startup log message. For a real system integrate Application Insights or a logging table
            return Ok(new { Message = "Logs are available in Application Insights or the server log files." });
        }
    }
}
