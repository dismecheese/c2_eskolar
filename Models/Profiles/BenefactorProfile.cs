using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    // BENEFACTOR PROFILE MODEL
    public class BenefactorProfile
    {
    [Key]
    public Guid BenefactorProfileId { get; set; }
    public required string UserId { get; set; } // Links to IdentityUser

        // Admin Information (person managing this organization's account)
        [Required]
        [StringLength(50)]
        public required string AdminFirstName { get; set; }

        [StringLength(50)]
        public string? AdminMiddleName { get; set; }

        [Required]
        [StringLength(50)]
        public required string AdminLastName { get; set; }

        public string AdminFullName => $"{AdminFirstName} {AdminMiddleName} {AdminLastName}";

        [StringLength(10)]
        public string? Sex { get; set; }

        [StringLength(50)]
        public string? Nationality { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(100)]
        public string? AdminPosition { get; set; } // "Scholarship Coordinator", "HR Manager", etc.

        // Organization Information
        [Required]
        [StringLength(150)]
        public required string OrganizationName { get; set; }

        [StringLength(100)]
        public string? OrganizationType { get; set; } // "Foundation", "Corporation", "Government"

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

        // Organization Details
        [StringLength(2000)]
        public string? Mission { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? Logo { get; set; } // Organization logo file path/URL

        // Verification & Status
    public bool IsVerified { get; set; } = false;
    public string? VerificationStatus { get; set; } = "Pending"; // Deprecated: Use AccountStatus instead
    public DateTime? VerificationDate { get; set; }

    // Account lifecycle status: Unverified → Pending → Verified → Archived  
    public string AccountStatus { get; set; } = "Unverified"; // Unverified, Pending, Verified, Archived

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<Scholarship> ProvidedScholarships { get; set; } = new List<Scholarship>();
        //public ICollection<Partnership> Partnerships { get; set; } = new List<Partnership>();
        //public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        //public ICollection<VerificationDocument> VerificationDocuments { get; set; } = new List<VerificationDocument>();
    }
}