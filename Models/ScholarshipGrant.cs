using System.ComponentModel.DataAnnotations;
using System;

namespace c2_eskolar.Models
{
    public class ScholarshipGrant
    {
        [Key]
        public int GrantId { get; set; }
        public int ScholarshipId { get; set; }
        public Scholarship Scholarship { get; set; } = null!;
        public int StudentId { get; set; }
        public User Student { get; set; } = null!;
        public DateTime? AwardedDate { get; set; }
        [StringLength(50)]
        public string? Status { get; set; }
    }
}
