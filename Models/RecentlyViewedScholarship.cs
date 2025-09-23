using System.ComponentModel.DataAnnotations;
using System;

namespace c2_eskolar.Models
{
    public class RecentlyViewedScholarship
    {
        [Key]
        public int ViewId { get; set; }
        public int StudentId { get; set; }
        public User Student { get; set; } = null!;
        public int ScholarshipId { get; set; }
        public Scholarship Scholarship { get; set; } = null!;
        public DateTime? ViewedAt { get; set; }
    }
}
