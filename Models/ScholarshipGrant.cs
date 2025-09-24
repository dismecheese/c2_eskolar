using System.ComponentModel.DataAnnotations;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class ScholarshipGrant
    {
        [Key]
        public int GrantId { get; set; }
    [ForeignKey("Scholarship")]
    public int ScholarshipId { get; set; }
    public virtual Scholarship Scholarship { get; set; } = null!;
    [ForeignKey("Student")]
    public int StudentId { get; set; }
    public virtual User Student { get; set; } = null!;
        public DateTime? AwardedDate { get; set; }
        [StringLength(50)]
        public string? Status { get; set; }
    }
}
