using System.ComponentModel.DataAnnotations;
using System;

namespace c2_eskolar.Models
{
    public class BookmarkedAnnouncement
    {
        [Key]
        public int BookmarkId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
