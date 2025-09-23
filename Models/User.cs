using System.ComponentModel.DataAnnotations;
using System;
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
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public StudentProfile? StudentProfile { get; set; }
        public InstitutionAdminProfile? InstitutionAdminProfile { get; set; }
        public BenefactorAdminProfile? BenefactorAdminProfile { get; set; }
        public ICollection<VerificationDocument> VerificationDocuments { get; set; } = new List<VerificationDocument>();
    }
}
