
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class Scholarship
    {
        public int ScholarshipId { get; set; }
        
        // Basic Information
        [Required]
        [StringLength(150)]
        public required string Title { get; set; }
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(1000)]
        public required string Benefits { get; set; } // "₱50,000 tuition + laptop + mentorship program"
        
        // Optional: For AI filtering/matching (extract main monetary value)
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal? MonetaryValue { get; set; }
        
        [Required]
        public DateTime ApplicationDeadline { get; set; }
        
        [StringLength(3000)]
        public string? Requirements { get; set; }
        
        // Scholarship Details
        [StringLength(50)]
        public string? ScholarshipType { get; set; } // "Academic", "Athletic", "Need-based", "Merit-based"
        
        [Range(1, 1000)]
        public int? SlotsAvailable { get; set; }
        
        // Eligibility Criteria (for AI matching)
    [Range(1.0, 5.0)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinimumGPA { get; set; }
        
        [StringLength(100)]
        public string? RequiredCourse { get; set; }
        
        [Range(1, 8)]
        public int? RequiredYearLevel { get; set; }
        
        [StringLength(100)]
        public string? RequiredUniversity { get; set; }
        
        // Status and Management
        public bool IsActive { get; set; } = true;
        public bool IsInternal { get; set; } = false; // true = institutional, false = open

        // Add this field to your Scholarship.cs model:

        // Application Management
        [Url]
        [StringLength(500)]
        public string? ExternalApplicationUrl { get; set; } // For external scholarships (benefactor's website)

        public bool RequiresExternalApplication => !string.IsNullOrEmpty(ExternalApplicationUrl);
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign Keys - Who created/manages this scholarship
        public int? BenefactorProfileId { get; set; } // For external scholarships
        public int? InstitutionProfileId { get; set; } // For institutional scholarships
        public int ScholarshipTypeId { get; set; } // FK to ScholarshipType
        
        // Navigation Properties
        public BenefactorProfile? Benefactor { get; set; }
        public InstitutionProfile? Institution { get; set; }
        public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();
    }
}