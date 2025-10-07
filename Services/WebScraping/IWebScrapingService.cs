using c2_eskolar.Models;

namespace c2_eskolar.Services.WebScraping
{
    public interface IWebScrapingService
    {
        Task<List<ScrapedScholarship>> ScrapeScholarshipsAsync(string sourceUrl);
        Task<InstitutionVerificationResult> VerifyInstitutionAsync(string institutionName, string website);
        Task<List<ScholarshipNews>> ScrapeScholarshipNewsAsync();
        Task<bool> VerifyOrganizationAsync(string organizationName, string website);
    }

    public class ScrapedScholarship
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Eligibility { get; set; } = "";
        public string ApplicationDeadline { get; set; } = "";
        public string Amount { get; set; } = "";
        public string SourceUrl { get; set; } = "";
        public string ContactInfo { get; set; } = "";
        public DateTime ScrapedAt { get; set; } = DateTime.Now;
    }

    public class InstitutionVerificationResult
    {
        public bool IsVerified { get; set; }
        public string OfficialName { get; set; } = "";
        public string AccreditationStatus { get; set; } = "";
        public string Location { get; set; } = "";
        public List<string> Programs { get; set; } = new();
    }

    public class ScholarshipNews
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string SourceUrl { get; set; } = "";
        public DateTime PublishedDate { get; set; }
        public DateTime ScrapedAt { get; set; } = DateTime.Now;
    }
}