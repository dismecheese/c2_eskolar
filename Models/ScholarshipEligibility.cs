using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class ScholarshipEligibility
    {
        [Key]
        public int EligibilityId { get; set; }
        public int ScholarshipId { get; set; }
        public Scholarship Scholarship { get; set; } = null!;
    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
    public decimal? MinGPA { get; set; }
        [StringLength(255)]
        public string? RequiredCourse { get; set; }
        public int? YearLevel { get; set; }
        public string? OtherCriteria { get; set; }
    }
}
