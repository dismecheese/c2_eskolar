using System.ComponentModel.DataAnnotations;
using c2_eskolar.Models.Enums;

namespace c2_eskolar.Models
{
    // Represents announcements posted by institutions or benefactors
    public class Announcement
    {
    // Primary key for the announcement
    public Guid AnnouncementId { get; set; }

         // Title of the announcement (required, max 200 characters)
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        // Full announcement content/body (required, max 5000 characters)
        [Required]
        [StringLength(5000)]
        public required string Content { get; set; }

        // Optional short summary for preview cards (max 500 characters)
        [StringLength(500)]
        public string? Summary { get; set; } // Short description for cards

        // AUTHOR INFORMATION

        // ID of the user who created the announcement
        public required string AuthorId { get; set; } // UserId of who created this

        // Display name of the author
        public required string AuthorName { get; set; } // Display name

        // Type of author (Institution, Benefactor)
        public required UserRole AuthorType { get; set; } // Institution, Benefactor

        // ORGANIZATION/INSTITUTION DETAILS

        // Optional: University/Organization name (separate from AuthorName)
        [StringLength(200)]
        public string? OrganizationName { get; set; } // University/Organization name (separate from AuthorName)

        // CATEGORIZATION

        // Announcement category (e.g., "Scholarship Update", "Deadline Reminder")
        [StringLength(100)]
        public string? Category { get; set; } // "Scholarship Update", "Deadline Reminder", etc.

        // Priority level (Low, Normal, High, Urgent)
        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;


        // TARGETING
        
        // Whether the announcement is visible to all students
        public bool IsPublic { get; set; } = true; // All students can see

        // Criteria for targeting specific audience (e.g., JSON array of course/department)
        public string? TargetAudience { get; set; } // JSON array of specific criteria

    // MEDIA
    // Multiple images/photos associated with the announcement
    public ICollection<Photo> Photos { get; set; } = new List<Photo>();

        // Optional attachment link (e.g., PDF, document)
        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        // STATUS & TIMING

        // Whether the announcement is currently active
        public bool IsActive { get; set; } = true;

        // Whether the announcement is pinned to the top
        public bool IsPinned { get; set; } = false;

    // Creation timestamp
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Last updated timestamp (nullable)
        public DateTime? UpdatedAt { get; set; }

        // Scheduled publish date (nullable)
        public DateTime? PublishDate { get; set; } // Can schedule for future

        // Expiration date after which the announcement auto-hides (nullable)
        public DateTime? ExpiryDate { get; set; } // Auto-hide after this date

        // ENGAGEMENT

        // Number of views the announcement has received
        public int ViewCount { get; set; } = 0;

        // Comma-separated tags for searching/filtering
        public string? Tags { get; set; } // Comma-separated for search
    }
    
    // Defines priority levels for announcements
    public enum AnnouncementPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }
}
