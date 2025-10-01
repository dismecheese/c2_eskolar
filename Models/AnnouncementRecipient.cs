
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class AnnouncementRecipient
    {
        [Key]
        public int AnnouncementRecipientId { get; set; }
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
    // Removed navigation property to custom User. Use IdentityUser if needed.
    }
}
