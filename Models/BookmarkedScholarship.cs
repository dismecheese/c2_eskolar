using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class BookmarkedScholarship
    {
        [Key]
        public int BookmarkId { get; set; }
        
        [Required]
        public required string UserId { get; set; } // Links to IdentityUser
        
        [Required]
        public int ScholarshipId { get; set; }
        public Scholarship Scholarship { get; set; } = null!;
        
        // Enhanced tracking fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastViewedAt { get; set; }
        
        [StringLength(100)]
        public string? BookmarkReason { get; set; } = "Interested"; // "High Match", "Good Fit", "Backup Option", "Interested"
        
        public bool IsUrgent { get; set; } = false; // Auto-calculated based on deadline
        
        [Range(1, 3)]
        public int Priority { get; set; } = 2; // 1=High, 2=Medium, 3=Low
        
        [StringLength(500)]
        public string? Notes { get; set; } // Student's personal notes
        
        public BookmarkStatus Status { get; set; } = BookmarkStatus.Bookmarked;
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal MatchScore { get; set; } = 0; // AI-calculated match percentage
        
        [StringLength(1000)]
        public string? Tags { get; set; } // JSON array of custom student tags
        
        // Notification preferences
        public bool EnableDeadlineReminders { get; set; } = true;
        public int ReminderDaysBefore { get; set; } = 7; // Days before deadline to remind
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    public enum BookmarkStatus
    {
        Bookmarked = 0,
        ReadyToApply = 1,
        InProgress = 2,
        Applied = 3,
        UnderReview = 4,
        Accepted = 5,
        Rejected = 6,
        Expired = 7,
        Withdrawn = 8
    }
}
