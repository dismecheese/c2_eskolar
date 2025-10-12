using System.ComponentModel.DataAnnotations;
using System;

namespace c2_eskolar.Models
{
    public class BookmarkedAnnouncement
    {
        [Key]
        public int BookmarkId { get; set; }
        public required string UserId { get; set; } // Links to IdentityUser
        // Removed navigation property to custom User to prevent UserId1 shadow property
        public Guid AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
