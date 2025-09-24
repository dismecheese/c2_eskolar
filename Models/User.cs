using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace c2_eskolar.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        [StringLength(255)]
        public string Email { get; set; } = "";
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = "";
    [ForeignKey("Role")]
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    public virtual StudentProfile? StudentProfile { get; set; }
    public virtual InstitutionAdminProfile? InstitutionAdminProfile { get; set; }
    public virtual BenefactorAdminProfile? BenefactorAdminProfile { get; set; }
    public virtual ICollection<VerificationDocument> VerificationDocuments { get; set; } = new List<VerificationDocument>();
    }
}
