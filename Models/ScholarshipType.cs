using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace c2_eskolar.Models
{
    public class ScholarshipType
    {
        [Key]
        public int ScholarshipTypeId { get; set; }
        [Required]
        [StringLength(100)]
        public string TypeName { get; set; } = "";
        public ICollection<Scholarship> Scholarships { get; set; } = new List<Scholarship>();
    }
}
