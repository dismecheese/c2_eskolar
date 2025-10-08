using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    /// <summary>
    /// Represents a scraped scholarship record with approval workflow
    /// Enhanced to include EskoBot Intelligence attribution and external URL data
    /// </summary>
    public class ScrapedScholarship
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }
        public string? Benefits { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonetaryValue { get; set; }
        
        public DateTime? ApplicationDeadline { get; set; }
        public string? Requirements { get; set; }
        public int? SlotsAvailable { get; set; }
        
        [Column(TypeName = "decimal(3,2)")]
        public decimal? MinimumGPA { get; set; }
        
        public string? RequiredCourse { get; set; }
        public int? RequiredYearLevel { get; set; }
        public string? RequiredUniversity { get; set; }

        // Enhanced external data fields
        public string? ExternalApplicationUrl { get; set; }
        public string? ExternalImageUrl { get; set; }
        public string? ExternalContactInfo { get; set; }
        public string? ExternalEligibilityDetails { get; set; }

        [Required]
        public string SourceUrl { get; set; } = "";
        
        [Required]
        public DateTime ScrapedAt { get; set; } = DateTime.Now;
        
        [Range(0.0, 1.0)]
        public double ParsingConfidence { get; set; }
        
        [Required]
        public ScrapingStatus Status { get; set; } = ScrapingStatus.Scraped;
        
        /// <summary>
        /// Indicates if this scholarship was enhanced with external URL data
        /// </summary>
        public bool IsEnhanced { get; set; }
        
        /// <summary>
        /// Raw extracted text from the web scraping process
        /// </summary>
        public string RawText { get; set; } = "";
        
        /// <summary>
        /// AI parsing notes and processing information
        /// </summary>
        public string ParsingNotes { get; set; } = "";

        // Approval workflow fields
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        // Database integration
        public int? PublishedScholarshipId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string AuthorAttribution { get; set; } = "EskoBot Intelligence";
        
        // AI Enhancement tracking
        public string? AiModel { get; set; } = "GPT-4.1 Mini";
        public string? AiPromptVersion { get; set; }
        public DateTime? EnhancedAt { get; set; }
        
        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "System";
        public string? UpdatedBy { get; set; }

        // Navigation properties for related data
        public virtual List<ScrapingProcessLog> ProcessingLogs { get; set; } = new();
        public virtual Scholarship? PublishedScholarship { get; set; }
    }

    /// <summary>
    /// Status enumeration for scraped scholarships in approval workflow
    /// </summary>
    public enum ScrapingStatus
    {
        Scraped = 1,      // Just scraped, awaiting review
        UnderReview = 2,  // Currently being reviewed by admin
        Approved = 3,     // Approved for publication
        Rejected = 4,     // Rejected, will not be published
        Published = 5,    // Successfully published to main system
        Archived = 6      // Archived/historical record
    }

    /// <summary>
    /// Tracks the processing history of scraped scholarships
    /// </summary>
    public class ScrapingProcessLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string ScrapedScholarshipId { get; set; } = "";
        
        [Required]
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(50)]
        public string ProcessType { get; set; } = ""; // Scraped, Enhanced, Reviewed, Approved, etc.
        
        public string? ProcessDetails { get; set; }
        public string? ProcessedBy { get; set; }
        
        [Range(0.0, 1.0)]
        public double? ConfidenceScore { get; set; }
        
        public string? Notes { get; set; }
        
        // Navigation property
        public virtual ScrapedScholarship? ScrapedScholarship { get; set; }
    }

    /// <summary>
    /// Configuration for web scraping sources and AI processing
    /// </summary>
    public class ScrapingConfiguration
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(200)]
        public string SourceName { get; set; } = "";
        
        [Required]
        public string BaseUrl { get; set; } = "";
        
        public string? SelectorRules { get; set; }
        public string? ExcludePatterns { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool EnableExternalUrlScraping { get; set; } = true;
        
        public int RateLimitDelayMs { get; set; } = 1000;
        public int MaxRetryAttempts { get; set; } = 3;
        
        // AI Configuration
        public string? CustomPromptTemplate { get; set; }
        public double MinConfidenceThreshold { get; set; } = 0.6;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "System";
    }

    /// <summary>
    /// Bulk operation tracking for administrative actions
    /// </summary>
    public class BulkOperationRecord
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(50)]
        public string OperationType { get; set; } = ""; // Approve, Reject, Archive, Delete, Export
        
        [Required]
        public DateTime ExecutedAt { get; set; } = DateTime.Now;
        
        [Required]
        public string ExecutedBy { get; set; } = "";
        
        public int TotalItemsProcessed { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        
        public string? FilterCriteria { get; set; }
        public string? ExecutionNotes { get; set; }
        public string? ErrorLog { get; set; }
        
        // JSON list of affected scholarship IDs
        public string ProcessedScholarshipIds { get; set; } = "[]";
    }
}