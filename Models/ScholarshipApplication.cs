using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class ScholarshipApplication
    {
        public int ScholarshipApplicationId { get; set; }
        
        // Foreign Keys
        public int StudentProfileId { get; set; }
        public int ScholarshipId { get; set; }
        
        // Application Type Management
        public bool IsExternalApplication { get; set; } // true = external link, false = internal form
        
        // For External Applications (redirects to benefactor's website)
        [Url]
        [StringLength(500)]
        public string? ExternalApplicationUrl { get; set; } // Link to benefactor's application form
        
        public DateTime? ExternalApplicationDate { get; set; } // When student clicked "Apply Now"
        public bool HasAppliedExternally { get; set; } = false; // Student confirms they applied
        
        // For Internal Applications (managed by institution)
        [StringLength(2000)]
        public string? PersonalStatement { get; set; } // Why they deserve the scholarship
        
        [StringLength(1000)]
        public string? UploadedDocuments { get; set; } // JSON or comma-separated file paths
        
        // Application Status (mainly for internal applications)
        [StringLength(50)]
        public string Status { get; set; } = "Submitted"; // "Submitted", "Under Review", "Approved", "Rejected", "External"
        
        // Review Information (for internal applications)
        [StringLength(1000)]
        public string? ReviewNotes { get; set; } // Admin/institution notes
        
        public DateTime? ReviewDate { get; set; }
        public string? ReviewedBy { get; set; } // UserId of who reviewed
        
        // ✅ ADD THIS PROPERTY
        public string ApplicationReference { get; set; } = $"APP-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        
        // Timestamps
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public StudentProfile Student { get; set; } = null!;
        public Scholarship Scholarship { get; set; } = null!;
    }
}