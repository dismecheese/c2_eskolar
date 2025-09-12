using System.ComponentModel.DataAnnotations;
using c2_eskolar.Models.Enums;

namespace c2_eskolar.Models
{
    public class Announcement
    {
        public int AnnouncementId { get; set; }
        
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }
        
        [Required]
        [StringLength(5000)]
        public required string Content { get; set; }
        
        [StringLength(500)]
        public string? Summary { get; set; } // Short description for cards
        
        // Author Information
        public required string AuthorId { get; set; } // UserId of who created this
        public required string AuthorName { get; set; } // Display name
        public required UserRole AuthorType { get; set; } // Institution, Benefactor
        
        // Organization/Institution Details
        [StringLength(200)]
        public string? OrganizationName { get; set; } // University/Organization name (separate from AuthorName)
        
        // Categorization
        [StringLength(100)]
        public string? Category { get; set; } // "Scholarship Update", "Deadline Reminder", etc.
        
        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;
        
        // Targeting (who should see this)
        public bool IsPublic { get; set; } = true; // All students can see
        public string? TargetAudience { get; set; } // JSON array of specific criteria
        
        // Media
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [StringLength(500)]
        public string? AttachmentUrl { get; set; }
        
        // Status & Timing
        public bool IsActive { get; set; } = true;
        public bool IsPinned { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishDate { get; set; } // Can schedule for future
        public DateTime? ExpiryDate { get; set; } // Auto-hide after this date
        
        // Engagement
        public int ViewCount { get; set; } = 0;
        public string? Tags { get; set; } // Comma-separated for search
    }
    
    public enum AnnouncementPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }
}
