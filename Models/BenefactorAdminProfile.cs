using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class BenefactorAdminProfile
    {
        [Key]
        public int BenefactorAdminProfileId { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
        [StringLength(255)]
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        [StringLength(255)]
        public string? Address { get; set; }
        [StringLength(50)]
        public string? ContactNumber { get; set; }
    public int BenefactorId { get; set; }
    [ForeignKey("BenefactorId")]
    public Benefactor Benefactor { get; set; } = null!;
        [StringLength(100)]
        public string? Position { get; set; }
        [StringLength(255)]
        public string? ProfilePicture { get; set; }
    }
}
