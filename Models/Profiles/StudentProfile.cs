using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    // STUDENT PROFILE MODEL
    public class StudentProfile
    {
        [StringLength(20)]
        public string? EnrollmentStatus { get; set; } = "Regular";
    [Key]
    public Guid StudentProfileId { get; set; }
    public required string UserId { get; set; } // Links to IdentityUser

        // Basic Information
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }

    [StringLength(50)]
    public string? MiddleName { get; set; }

    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }

    public string FullName => $"{FirstName} {MiddleName} {LastName}";

    [StringLength(20)]
    public string? Sex { get; set; }

    [StringLength(50)]
    public string? Nationality { get; set; }

    [StringLength(255)]
    public string? PermanentAddress { get; set; }


    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? InstitutionalEmail { get; set; }

    [Phone]
    [StringLength(15)]
    public string? MobileNumber { get; set; }

        // Personal Details
        public DateTime? BirthDate { get; set; }

    // Removed unused Address and ContactNumber properties

        // Academic Information
        [StringLength(100)]
        public string? UniversityName { get; set; }

        [Range(1, 8)]
        public int? YearLevel { get; set; }

        [StringLength(100)]
        public string? Course { get; set; }

        [StringLength(50)]
        public string? StudentNumber { get; set; }


    // Profile & Verification (from your proposal)
    [StringLength(255)]
    public string? ProfilePicture { get; set; }

    public bool IsVerified { get; set; } = false;
    public string? VerificationStatus { get; set; } = "Pending"; // Pending, Verified, Rejected
    public DateTime? VerificationDate { get; set; }

    // Account lifecycle status
    public string AccountStatus { get; set; } = "Active"; // Active, Deleted, Locked

    // Document Uploads
    [StringLength(255)]
    public string? StudentIdDocumentPath { get; set; }
    [StringLength(255)]
    public string? CorDocumentPath { get; set; }

    // Institution Check
    public bool? IsPartnerInstitution { get; set; }
    [StringLength(100)]
    public string? PartnerInstitutionName { get; set; }

        // Academic Performance (for AI recommendations)
        [Range(1.0, 5.0)]
        public decimal? GPA { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();
        //public ICollection<VerificationDocument> VerificationDocuments { get; set; } = new List<VerificationDocument>();
        //public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}