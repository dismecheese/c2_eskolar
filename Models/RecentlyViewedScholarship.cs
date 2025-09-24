using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class RecentlyViewedScholarship
    {
        [Key]
        public int ViewId { get; set; }
    public int StudentId { get; set; }
    [ForeignKey("StudentId")]
    public User Student { get; set; } = null!;
    public int ScholarshipId { get; set; }
    [ForeignKey("ScholarshipId")]
    public Scholarship Scholarship { get; set; } = null!;
        public DateTime? ViewedAt { get; set; }
    }
}
