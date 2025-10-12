using System;
using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class Photo
    {
        [Key]
        public Guid PhotoId { get; set; }
    public int? ScholarshipId { get; set; }
    public Guid? AnnouncementId { get; set; }
        public string Url { get; set; } = string.Empty;
        // SortOrder controls display order for photos within an announcement
        public int SortOrder { get; set; } = 0;
        public string? Caption { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public Scholarship? Scholarship { get; set; }
        public Announcement? Announcement { get; set; }
    }
}