using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    // INSTITUTION PROFILE MODEL
    public class InstitutionProfile
    {
        public int InstitutionProfileId { get; set; }
        public required string UserId { get; set; } // Links to Identity User (admin account)

        // Admin Information (person managing this institution's account)
        [Required]
        [StringLength(50)]
        public required string AdminFirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string AdminLastName { get; set; }

        public string AdminFullName => $"{AdminFirstName} {AdminLastName}";

        [StringLength(100)]
        public string? AdminPosition { get; set; } // "Dean of Student Affairs", "Registrar", etc.

        // Institution Information
        [Required]
        [StringLength(150)]
        public required string InstitutionName { get; set; }

        [StringLength(100)]
        public string? InstitutionType { get; set; } // "University", "College", "Technical School", "High School"

        [StringLength(255)]
        public string? Address { get; set; }

        [Phone]
        [StringLength(15)]
        public string? ContactNumber { get; set; }

        [Url]
        [StringLength(100)]
        public string? Website { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? ContactEmail { get; set; }

        // Institution Details
        [StringLength(2000)]
        public string? Mission { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? Logo { get; set; } // Institution logo file path/URL

        // Academic Information
        public int? TotalStudents { get; set; }
        public DateTime? EstablishedDate { get; set; }

        [StringLength(100)]
        public string? Accreditation { get; set; } // "CHED", "TESDA", etc.

        // Verification & Status
        public bool IsVerified { get; set; } = false;
        public string? VerificationStatus { get; set; } = "Pending"; // Pending, Verified, Rejected
        public DateTime? VerificationDate { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<Scholarship> ManagedScholarships { get; set; } = new List<Scholarship>();
        //public ICollection<Partnership> Partnerships { get; set; } = new List<Partnership>();
        //public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        //public ICollection<VerificationDocument> VerificationDocuments { get; set; } = new List<VerificationDocument>();
    }
}