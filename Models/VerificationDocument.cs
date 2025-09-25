using System.ComponentModel.DataAnnotations;
using System;

namespace c2_eskolar.Models
{
    public class VerificationDocument
    {
        [Key]
        public int DocumentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
        [StringLength(100)]
        public string? DocumentType { get; set; }
        [StringLength(255)]
        public string? FilePath { get; set; }
        public string? OCRExtractedData { get; set; }
        public DateTime? UploadedAt { get; set; }
        [StringLength(50)]
        public string? Status { get; set; }
    }
}
