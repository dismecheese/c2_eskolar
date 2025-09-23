using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Keep only essential navigation properties for now
        public virtual StudentProfile? StudentProfile { get; set; }
    }
}
