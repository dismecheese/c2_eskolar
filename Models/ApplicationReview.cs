using System.ComponentModel.DataAnnotations;
using System;

namespace c2_eskolar.Models
{
    public class ApplicationReview
    {
        [Key]
        public int ReviewId { get; set; }
        public int ApplicationId { get; set; }
        public ScholarshipApplication Application { get; set; } = null!;
    public int ReviewerUserId { get; set; }
    // Removed navigation property to custom User. Use IdentityUser if needed.
        public int Score { get; set; }
        public string? Comments { get; set; }
        public DateTime? ReviewDate { get; set; }
    }
}
