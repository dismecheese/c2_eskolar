using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class InstitutionAdminProfile
    {
        [Key]
        public int InstitutionAdminProfileId { get; set; }
    public string UserId { get; set; } = string.Empty; // Links to IdentityUser
        public string? ContactNumber { get; set; }
    public int InstitutionId { get; set; }
    [ForeignKey("InstitutionId")]
    public Institution Institution { get; set; } = null!;
        [StringLength(100)]
        public string? Position { get; set; }
        [StringLength(255)]
        public string? ProfilePicture { get; set; }
    }
}
