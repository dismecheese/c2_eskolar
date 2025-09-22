using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class ScholarshipInputModel
    {
        [Required(ErrorMessage = "Scholarship name is required.")]
        [StringLength(100, ErrorMessage = "Scholarship name can't exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Provider is required.")]
        [StringLength(100, ErrorMessage = "Provider name can't exceed 100 characters.")]
        public string Provider { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description can't exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Eligibility criteria are required.")]
        [StringLength(1000, ErrorMessage = "Eligibility text can't exceed 1000 characters.")]
        public string Eligibility { get; set; } = string.Empty;

        [Required(ErrorMessage = "Deadline is required.")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; } = DateTime.Today;
    }
}
