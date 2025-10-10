using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace c2_eskolar.Models
{
    /// <summary>
    /// Tracks AI token usage across the application for cost monitoring and analytics
    /// </summary>
    public class AITokenUsage
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(100)]
        public string Operation { get; set; } = ""; // "WebScraping", "Chatbot", "DocumentAnalysis", etc.
        
        [Required]
        [StringLength(50)]
        public string Model { get; set; } = ""; // "gpt-4o-mini", "gpt-35-turbo", etc.
        
        /// <summary>
        /// Azure OpenAI deployment name (e.g., "gpt-4o-mini-deployment")
        /// </summary>
        [StringLength(200)]
        public string? DeploymentName { get; set; }
        
        /// <summary>
        /// Azure region where the request was processed (e.g., "eastus", "westeurope")
        /// </summary>
        [StringLength(50)]
        public string? Region { get; set; }
        
        [Required]
        public int PromptTokens { get; set; }
        
        [Required]
        public int CompletionTokens { get; set; }
        
        public int TotalTokens => PromptTokens + CompletionTokens;
        
        [Column(TypeName = "decimal(10,4)")]
        public decimal EstimatedCost { get; set; } // In USD
        
        /// <summary>
        /// Request duration in milliseconds
        /// </summary>
        public int? RequestDurationMs { get; set; }
        
        [StringLength(500)]
        public string? RequestDetails { get; set; } // Optional details about the request
        
        [StringLength(200)]
        public string? UserId { get; set; } // Track which user initiated the request
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsSuccessful { get; set; } = true;
        
        public string? ErrorMessage { get; set; }
    }
}