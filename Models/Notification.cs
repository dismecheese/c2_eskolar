using System;
namespace c2_eskolar.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string UserId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "info";
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }
}
