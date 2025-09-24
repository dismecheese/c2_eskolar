
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class AnnouncementRecipient
    {
        [Key]
        public int AnnouncementRecipientId { get; set; }
    [ForeignKey("Announcement")]
    public int AnnouncementId { get; set; }
    public virtual Announcement Announcement { get; set; } = null!;
    [ForeignKey("User")]
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    }
}
