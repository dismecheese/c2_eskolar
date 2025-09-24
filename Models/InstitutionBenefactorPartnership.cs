using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class InstitutionBenefactorPartnership
    {
        [Key]
        public int PartnershipId { get; set; }
    public int InstitutionId { get; set; }
    [ForeignKey("InstitutionId")]
    public Institution Institution { get; set; } = null!;
    public int BenefactorId { get; set; }
    [ForeignKey("BenefactorId")]
    public Benefactor Benefactor { get; set; } = null!;
        [StringLength(50)]
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
