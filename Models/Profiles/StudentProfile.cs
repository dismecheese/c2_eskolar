using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class StudentProfile
    {
        public int StudentProfileId { get; set; }
        public required string UserId { get; set; } // Links to Identity User
        
        // Basic Information
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }
        
        public string FullName => $"{FirstName} {LastName}";
        
        // Personal Details
        public DateTime? BirthDate { get; set; }
        
        [StringLength(255)]
        public string? Address { get; set; }
        
        [Phone]
        [StringLength(15)]
        public string? ContactNumber { get; set; }
        
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