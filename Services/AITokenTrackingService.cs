using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OpenAI.Chat;

namespace c2_eskolar.Services
{
    /// <summary>
    /// Service for tracking Azure OpenAI token usage and costs
    /// </summary>
    public class AITokenTrackingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        
        // Azure OpenAI pricing per 1K tokens (as of October 2024)
        private readonly Dictionary<string, (decimal InputPrice, decimal OutputPrice)> _pricingTiers = new()
        {
            // GPT-4o models
            { "gpt-4o", (0.0025m, 0.010m) },
            { "gpt-4o-mini", (0.00015m, 0.0006m) },
            
            // GPT-4 models  
            { "gpt-4", (0.03m, 0.06m) },
            { "gpt-4-32k", (0.06m, 0.12m) },
            
            // GPT-3.5 models
            { "gpt-35-turbo", (0.0015m, 0.002m) },
            { "gpt-35-turbo-16k", (0.003m, 0.004m) },
            
            // Default fallback
            { "default", (0.002m, 0.004m) }
        };

        public AITokenTrackingService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IConfiguration configuration)
        {
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Tracks token usage from an Azure OpenAI chat completion response
        /// </summary>
        public async Task TrackChatCompletionUsageAsync(
            ChatCompletion completion,
            string operation,
            string? userId = null,
            string? additionalDetails = null,
            TimeSpan? requestDuration = null)
        {
            try
            {
                if (completion?.Usage == null)
                {
                    Console.WriteLine("Warning: ChatCompletion response has no usage information");
                    return;
                }

                var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "unknown";
                var region = _configuration["AzureOpenAI:Region"] ?? "unknown";
                var modelName = completion.Model ?? "unknown";

                // Extract actual token counts from response using reflection-safe approach
                var usage = completion.Usage;
                int promptTokens = 0;
                int completionTokens = 0;
                int totalTokens = 0;

                // Try different property names that might exist in the SDK
                try
                {
                    var usageType = usage.GetType();
                    Console.WriteLine($"DEBUG: Usage object type: {usageType.Name}");
                    
                    // List all properties for debugging
                    var properties = usageType.GetProperties();
                    Console.WriteLine($"DEBUG: Available properties: {string.Join(", ", properties.Select(p => $"{p.Name}:{p.PropertyType.Name}"))}");
                    
                    // Try common property names
                    var promptProp = usageType.GetProperty("PromptTokens") ?? 
                                   usageType.GetProperty("InputTokens") ?? 
                                   usageType.GetProperty("PromptTokenCount");
                    
                    var completionProp = usageType.GetProperty("CompletionTokens") ?? 
                                       usageType.GetProperty("OutputTokens") ?? 
                                       usageType.GetProperty("CompletionTokenCount");
                    
                    var totalProp = usageType.GetProperty("TotalTokens") ?? 
                                  usageType.GetProperty("TotalTokenCount");

                    if (promptProp != null)
                    {
                        promptTokens = (int)(promptProp.GetValue(usage) ?? 0);
                        Console.WriteLine($"DEBUG: Extracted PromptTokens = {promptTokens} using property {promptProp.Name}");
                    }
                    else
                    {
                        Console.WriteLine("DEBUG: No prompt tokens property found");
                    }
                    
                    if (completionProp != null)
                    {
                        completionTokens = (int)(completionProp.GetValue(usage) ?? 0);
                        Console.WriteLine($"DEBUG: Extracted CompletionTokens = {completionTokens} using property {completionProp.Name}");
                    }
                    else
                    {
                        Console.WriteLine("DEBUG: No completion tokens property found");
                    }
                    
                    if (totalProp != null)
                    {
                        totalTokens = (int)(totalProp.GetValue(usage) ?? 0);
                        Console.WriteLine($"DEBUG: Extracted TotalTokens = {totalTokens} using property {totalProp.Name}");
                    }
                    else
                    {
                        totalTokens = promptTokens + completionTokens; // Fallback calculation
                        Console.WriteLine($"DEBUG: Calculated TotalTokens = {totalTokens} (prompt + completion)");
                    }
                        completionTokens = (int)(completionProp.GetValue(usage) ?? 0);
                        
                    if (totalProp != null)
                        totalTokens = (int)(totalProp.GetValue(usage) ?? 0);
                    else
                        totalTokens = promptTokens + completionTokens; // Fallback calculation
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not extract token counts from usage object: {ex.Message}");
                    // Use placeholder values if we can't extract them
                    promptTokens = 100; // Reasonable placeholder
                    completionTokens = 50; // Reasonable placeholder  
                    totalTokens = 150;
                }

                // Calculate cost based on model and token usage
                var estimatedCost = CalculateCost(modelName, promptTokens, completionTokens);

                // Create detailed request information
                var requestDetails = new
                {
                    ModelUsed = modelName,
                    FinishReason = completion.FinishReason.ToString(),
                    MessageCount = 1, // Single completion
                    HasSystemMessage = true, // Typically true for our chat completions
                    AdditionalInfo = additionalDetails
                };

                var tokenUsage = new AITokenUsage
                {
                    Operation = operation,
                    Model = modelName,
                    DeploymentName = deploymentName,
                    Region = region,
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    EstimatedCost = estimatedCost,
                    RequestDurationMs = requestDuration?.Milliseconds,
                    UserId = userId,
                    RequestDetails = JsonSerializer.Serialize(requestDetails),
                    IsSuccessful = true,
                    CreatedAt = DateTime.UtcNow
                };

                await SaveTokenUsageAsync(tokenUsage);

                Console.WriteLine($"Tracked AI usage: {operation} - {totalTokens} tokens (${estimatedCost:F4}) for user {userId ?? "system"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error tracking token usage: {ex.Message}");
                
                // Still try to save a basic record even if detailed tracking fails
                await SaveBasicTokenUsageAsync(operation, userId, ex.Message);
            }
        }

        /// <summary>
        /// Tracks failed AI requests for monitoring purposes
        /// </summary>
        public async Task TrackFailedRequestAsync(
            string operation,
            string errorMessage,
            string? userId = null,
            TimeSpan? requestDuration = null)
        {
            try
            {
                var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "unknown";
                var region = _configuration["AzureOpenAI:Region"] ?? "unknown";

                var tokenUsage = new AITokenUsage
                {
                    Operation = operation,
                    Model = "failed-request",
                    DeploymentName = deploymentName,
                    Region = region,
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    EstimatedCost = 0,
                    RequestDurationMs = requestDuration?.Milliseconds,
                    UserId = userId,
                    IsSuccessful = false,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow
                };

                await SaveTokenUsageAsync(tokenUsage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error tracking failed request: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets total token usage statistics
        /// </summary>
        public async Task<TokenUsageStats> GetUsageStatsAsync(DateTime? since = null)
        {
            using var context = _dbContextFactory.CreateDbContext();
            
            var query = context.AITokenUsages.AsQueryable();
            
            if (since.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= since.Value);
            }

            var usageData = await query.ToListAsync();

            return new TokenUsageStats
            {
                TotalTokens = usageData.Sum(u => u.TotalTokens),
                TotalPromptTokens = usageData.Sum(u => u.PromptTokens),
                TotalCompletionTokens = usageData.Sum(u => u.CompletionTokens),
                TotalCost = usageData.Sum(u => u.EstimatedCost),
                RequestCount = usageData.Count,
                SuccessfulRequests = usageData.Count(u => u.IsSuccessful),
                FailedRequests = usageData.Count(u => !u.IsSuccessful),
                AverageTokensPerRequest = usageData.Any() ? usageData.Average(u => u.TotalTokens) : 0,
                MostUsedOperation = usageData.GroupBy(u => u.Operation)
                    .OrderByDescending(g => g.Sum(u => u.TotalTokens))
                    .FirstOrDefault()?.Key ?? "None"
            };
        }

        private decimal CalculateCost(string modelName, int promptTokens, int completionTokens)
        {
            // Find the best matching pricing tier
            var pricing = _pricingTiers.FirstOrDefault(p => 
                modelName.Contains(p.Key, StringComparison.OrdinalIgnoreCase)).Value;
            
            if (pricing == default)
            {
                pricing = _pricingTiers["default"];
            }

            // Calculate cost: (tokens / 1000) * price_per_1k_tokens
            var promptCost = (promptTokens / 1000.0m) * pricing.InputPrice;
            var completionCost = (completionTokens / 1000.0m) * pricing.OutputPrice;
            
            return promptCost + completionCost;
        }

        private async Task SaveTokenUsageAsync(AITokenUsage tokenUsage)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AITokenUsages.Add(tokenUsage);
            await context.SaveChangesAsync();
        }

        private async Task SaveBasicTokenUsageAsync(string operation, string? userId, string errorMessage)
        {
            try
            {
                var tokenUsage = new AITokenUsage
                {
                    Operation = operation,
                    Model = "error-fallback",
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    EstimatedCost = 0,
                    UserId = userId,
                    IsSuccessful = false,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow
                };

                await SaveTokenUsageAsync(tokenUsage);
            }
            catch
            {
                // Silently fail if we can't even save basic tracking
            }
        }
    }

    /// <summary>
    /// Statistics about token usage
    /// </summary>
    public class TokenUsageStats
    {
        public int TotalTokens { get; set; }
        public int TotalPromptTokens { get; set; }
        public int TotalCompletionTokens { get; set; }
        public decimal TotalCost { get; set; }
        public int RequestCount { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double AverageTokensPerRequest { get; set; }
        public string MostUsedOperation { get; set; } = string.Empty;
    }
}