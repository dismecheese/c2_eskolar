using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    // Represents a student's application for a scholarship
    public class ScholarshipApplication
    {
        public int ScholarshipApplicationId { get; set; }

        // FOREIGN KEYS
        public int StudentProfileId { get; set; }
        public int ScholarshipId { get; set; }

        // APPLICATION TYPE MANAGEMENT
        public bool IsExternalApplication { get; set; } // true = external link, false = internal form

        // EXTERNAL APPLICATIONS (redirects to benefactor's website)
        [Url]
        [StringLength(500)]
        public string? ExternalApplicationUrl { get; set; } // Link to benefactor's application form

        public DateTime? ExternalApplicationDate { get; set; } // When student clicked "Apply Now"
        public bool HasAppliedExternally { get; set; } = false; // Student confirms they applied

        // INTERNAL APPLICATIONS (managed by institution)
        [StringLength(2000)]
        public string? PersonalStatement { get; set; } // Why they deserve the scholarship

        [StringLength(1000)]
        public string? UploadedDocuments { get; set; } // JSON or comma-separated file paths

        // APPLICATION STATUS (mainly for internal applications)
        [StringLength(50)]
        public string Status { get; set; } = "Submitted"; // "Submitted", "Under Review", "Approved", "Rejected", "External"

        // REVIEW INFORMATION (for internal applications)
        [StringLength(1000)]
        public string? ReviewNotes { get; set; } // Admin/institution notes

        public DateTime? ReviewDate { get; set; }
        public string? ReviewedBy { get; set; } // UserId of who reviewed

        // TIMESTAMPS
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // NAVIGATION PROPERTIES
        public StudentProfile Student { get; set; } = null!;
        public Scholarship Scholarship { get; set; } = null!;
    }
}