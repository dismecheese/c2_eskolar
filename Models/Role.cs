using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace c2_eskolar.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = "";
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
