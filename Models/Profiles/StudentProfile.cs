using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class StudentProfile
    {
        [Key]
    public int StudentProfileId { get; set; }

        /// <summary>
        /// Links this profile to the IdentityUser.
        /// </summary>
        [Required]
        public required string UserId { get; set; } // Foreign key to ApplicationUser

        // Navigation property
        public virtual ApplicationUser? User { get; set; }

        #region Basic Information

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; }

        /// <summary>
        /// Combines first and last name for display.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        #endregion

        #region Personal Details

        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }

        [StringLength(255)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Phone]
        [StringLength(15)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        /// <summary>
        /// Optional calculated age (nullable if birthdate is not set).
        /// </summary>
        public int? Age => BirthDate.HasValue
            ? (int)((DateTime.Now - BirthDate.Value).TotalDays / 365.25)
            : null;

        #endregion

        #region Academic Information

        [StringLength(100)]
        [Display(Name = "University Name")]
    public string? UniversityName { get; set; }

    // Student biography
    public string? Bio { get; set; }

        [Range(1, 8)]
        [Display(Name = "Year Level")]
        public int? YearLevel { get; set; }

        [StringLength(100)]
        [Display(Name = "Course")]
        public string? Course { get; set; }

        [StringLength(50)]
        [Display(Name = "Student Number")]
        public string? StudentNumber { get; set; }

        [Range(1.0, 5.0)]
        [Display(Name = "GPA")]
        public decimal? GPA { get; set; }

        #endregion

        #region Profile & Verification

        [StringLength(255)]
        [Display(Name = "Profile Picture URL")]
        public string? ProfilePicture { get; set; }

        [Display(Name = "Verified")]
        public bool IsVerified { get; set; } = false;

        [StringLength(20)]
        [Display(Name = "Verification Status")]
        public string? VerificationStatus { get; set; } = "Pending"; // "Pending", "Verified", "Rejected"

        [Display(Name = "Verification Date")]
        public DateTime? VerificationDate { get; set; }

        #endregion

        #region Timestamps

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }

        #endregion

        #region Navigation Properties

        public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();

        // Future relationships (uncomment when ready)
        // public ICollection<VerificationDocument> VerificationDocuments { get; set; } = new List<VerificationDocument>();
        // public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

        #endregion
    }
}