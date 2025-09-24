using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    public class VerificationDocument
    {
        [Key]
        public int DocumentId { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
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
