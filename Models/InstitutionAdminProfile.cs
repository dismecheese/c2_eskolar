using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class InstitutionAdminProfile
    {
        [Key]
        public int InstitutionAdminProfileId { get; set; }
    public required string UserId { get; set; } // Links to IdentityUser
        [StringLength(255)]
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        [StringLength(255)]
        public string? Address { get; set; }
        [StringLength(50)]
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
