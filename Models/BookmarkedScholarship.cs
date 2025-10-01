using System.ComponentModel.DataAnnotations;
using System;

namespace c2_eskolar.Models
{
    public class BookmarkedScholarship
    {
        [Key]
        public int BookmarkId { get; set; }
    public required string UserId { get; set; } // Links to IdentityUser
    // Removed navigation property to custom User to prevent UserId1 shadow property
        public int ScholarshipId { get; set; }
        public Scholarship Scholarship { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
