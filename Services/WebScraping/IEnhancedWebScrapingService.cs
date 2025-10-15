using c2_eskolar.Models;
using c2_eskolar.Services;
using System.Text;
using System.Globalization;

namespace c2_eskolar.Services.WebScraping
{
    public interface IEnhancedWebScrapingService
    {
        Task<List<EnhancedScrapedScholarship>> ScrapeAndParseScholarshipsAsync(string sourceUrl);
        Task<List<EnhancedScrapedScholarship>> ScrapeAndParseScholarshipsAsync(string sourceUrl, Action<int, string>? progressCallback = null);
        Task<string> GenerateScholarshipCsvAsync(List<EnhancedScrapedScholarship> scholarships);
        Task<EnhancedScrapedScholarship> ParseScholarshipWithAIAsync(string rawText, string sourceUrl);
    }

    public class EnhancedScrapedScholarship
    {
        // Fields that map directly to Scholarship model
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Benefits { get; set; } = "";
        public decimal? MonetaryValue { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public string? Requirements { get; set; }
        public int? SlotsAvailable { get; set; }
        public decimal? MinimumGPA { get; set; }
        public string? RequiredCourse { get; set; }
        public int? RequiredYearLevel { get; set; }
        public string? RequiredUniversity { get; set; }
        public string? ExternalApplicationUrl { get; set; }
        
        // Additional metadata
        public string SourceUrl { get; set; } = "";
        public string RawText { get; set; } = "";
        public DateTime ScrapedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsInternal { get; set; } = false;
        
        // AI parsing confidence
        public double ParsingConfidence { get; set; } = 0.0;
        public List<string> ParsingNotes { get; set; } = new();
    }
}