using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    // Represents a scholarship opportunity offered by benefactors or institutions
    public class Scholarship
    {
        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
        public int ScholarshipId { get; set; }

        // BASIC INFORMATION
        [Required]
        [StringLength(150)]
        public required string Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Benefits { get; set; } // "₱50,000 tuition + laptop + mentorship program"

        // Optional: For AI filtering/matching (extract main monetary value)
        [Range(0, 10000000)]
        public decimal? MonetaryValue { get; set; }

        [Required]
        public DateTime ApplicationDeadline { get; set; }

        [StringLength(3000)]
        public string? Requirements { get; set; }


        [Range(1, 1000)]
        public int? SlotsAvailable { get; set; }

        // ELIGIBILITY CRITERIA (for AI matching)
        [Range(1.0, 5.0)]
        public decimal? MinimumGPA { get; set; }

        [StringLength(100)]
        public string? RequiredCourse { get; set; }

        [Range(1, 8)]
        public int? RequiredYearLevel { get; set; }

        [StringLength(100)]
        public string? RequiredUniversity { get; set; }

        // STATUS AND MANAGEMENT
        public bool IsActive { get; set; } = true;
        public bool IsInternal { get; set; } = false; // true = institutional, false = open

        // APPLICATION MANAGEMENT
        [Url]
        [StringLength(500)]
        public string? ExternalApplicationUrl { get; set; } // For external scholarships (benefactor's website)

        public bool RequiresExternalApplication => !string.IsNullOrEmpty(ExternalApplicationUrl);

        // TIMESTAMPS
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // FOREIGN KEYS - Who created/manages the scholarship
    public Guid? BenefactorProfileId { get; set; } // For external scholarships
    public Guid? InstitutionProfileId { get; set; } // For institutional scholarships

        // NAVIGATION PROPERTIES
        public BenefactorProfile? Benefactor { get; set; }
        public InstitutionProfile? Institution { get; set; }
        public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();
    }
}