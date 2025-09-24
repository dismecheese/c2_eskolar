using System.ComponentModel.DataAnnotations;
using System;

namespace c2_eskolar.Models
{
    public class BookmarkedScholarship
    {
        [Key]
        public int BookmarkId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ScholarshipId { get; set; }
        public Scholarship Scholarship { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
