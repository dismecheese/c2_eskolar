using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace c2_eskolar.Models
{
    public class Benefactor
    {
        [Key]
        public int BenefactorId { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = "";
        [StringLength(255)]
        public string? Address { get; set; }
        [StringLength(255)]
        public string? Email { get; set; }
        [StringLength(50)]
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
        [StringLength(255)]
        public string? Logo { get; set; }
        public ICollection<BenefactorAdminProfile> AdminProfiles { get; set; } = new List<BenefactorAdminProfile>();
        public ICollection<InstitutionBenefactorPartnership> Partnerships { get; set; } = new List<InstitutionBenefactorPartnership>();
    }
}
