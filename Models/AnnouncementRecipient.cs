using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class AnnouncementRecipient
    {
        [Key]
        public int AnnouncementRecipientId { get; set; }
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
