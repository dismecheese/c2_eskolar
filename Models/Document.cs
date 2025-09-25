using System;
using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models
{
    public class Document
    {
        [Key]
        public Guid DocumentId { get; set; }
        public Guid ScholarshipApplicationId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public ScholarshipApplication ScholarshipApplication { get; set; } = null!;
    }
}