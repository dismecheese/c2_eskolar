using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    /// <summary>
    /// Stores pre-aggregated monthly statistics for historical analytics
    /// This table is populated automatically at the end of each month
    /// </summary>
    public class MonthlyStatistics
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        // Application Statistics
        public int TotalApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int PendingApplications { get; set; }
        public int RejectedApplications { get; set; }

        // User Statistics
        public int TotalUsers { get; set; }
        public int NewUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalBenefactors { get; set; }
        public int TotalInstitutions { get; set; }
        public int VerifiedUsers { get; set; }

        // Scholarship Statistics
        public int TotalScholarships { get; set; }
        public int ActiveScholarships { get; set; }

        // Financial Statistics
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalScholarshipValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DistributedValue { get; set; }

        // Performance Metrics
        public double BenefactorSuccessRate { get; set; }
        public double InstitutionSuccessRate { get; set; }
        public double AverageProcessingDays { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Unique constraint to prevent duplicate entries for the same month/year
        /// </summary>
        public string MonthYearKey => $"{Year}-{Month:D2}";
    }
}
