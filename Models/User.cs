using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace c2_eskolar.Models
{
    public class User
    {
    [Key]
    public string UserId { get; set; } = string.Empty;
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
    // Removed navigation properties to prevent UserId1 shadow property
    }
}
