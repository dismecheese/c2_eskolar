
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using c2_eskolar.Models;
using c2_eskolar.Services.AI;

namespace c2_eskolar.Services
{
    public class OpenAIService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;
        private readonly ProfileSummaryService _profileSummaryService;
        private readonly ScholarshipRecommendationService _scholarshipRecommendationService;
        private readonly AnnouncementRecommendationService _announcementRecommendationService;
        private readonly ContextGenerationService _contextGenerationService;
        private readonly DisplayContextAwarenessService _displayContextAwarenessService;
        private readonly AITokenTrackingService _tokenTrackingService;

        public OpenAIService(IConfiguration config, ProfileSummaryService profileSummaryService, ScholarshipRecommendationService scholarshipRecommendationService, AnnouncementRecommendationService announcementRecommendationService, ContextGenerationService contextGenerationService, DisplayContextAwarenessService displayContextAwarenessService, AITokenTrackingService tokenTrackingService)
        {
            var apiKey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey");
            var endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
            _deploymentName = config["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName");
            _client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            _profileSummaryService = profileSummaryService;
            _scholarshipRecommendationService = scholarshipRecommendationService;
            _announcementRecommendationService = announcementRecommendationService;
            _contextGenerationService = contextGenerationService;
            _displayContextAwarenessService = displayContextAwarenessService;
            _tokenTrackingService = tokenTrackingService;
        }

        public async Task<string> GetChatCompletionAsync(string userMessage)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);
                var messages = new[]
                {
                    new UserChatMessage(userMessage)
                };
                
                var response = await chatClient.CompleteChatAsync(messages);
                stopwatch.Stop();
                
                // Track token usage
                await _tokenTrackingService.TrackChatCompletionUsageAsync(
                    completion: response.Value,
                    operation: "BasicChatCompletion",
                    userId: null, // No user context in basic method
                    additionalDetails: $"Message length: {userMessage.Length}",
                    requestDuration: stopwatch.Elapsed
                );
                
                return response.Value.Content[0].Text.Trim();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Track failed request
                await _tokenTrackingService.TrackFailedRequestAsync(
                    operation: "BasicChatCompletion",
                    errorMessage: ex.Message,
                    userId: null,
                    requestDuration: stopwatch.Elapsed
                );
                
                throw; // Re-throw the exception
            }
        }

        // Enhanced method: includes user profile and current page context
        public async Task<string> GetChatCompletionWithProfileAndContextAsync(string userMessage, IdentityUser user, ChatContext? context = null, bool isFirstMessage = false)
        {
            var profileSummary = await _profileSummaryService.GetProfileSummaryAsync(user);
            var firstName = await _profileSummaryService.GetUserFirstNameAsync(user);
            var scholarshipRecommendations = await _scholarshipRecommendationService.GetScholarshipRecommendationsAsync(user);
            
            // Create a personalized greeting only for the first message
            string greeting = "";
            if (isFirstMessage)
            {
                greeting = !string.IsNullOrEmpty(firstName) 
                    ? $"Hello {firstName}! How can I help you today?"
                    : "Hello! How can I help you today?";
            }

            // Check if user is asking about scholarships, announcements, or similar queries
            var isScholarshipQuery = IsScholarshipRelatedQuery(userMessage);
            var isAnnouncementQuery = IsAnnouncementRelatedQuery(userMessage);
            
            // Get announcement recommendations if this is an announcement query
            List<AnnouncementRecommendation> announcementRecommendations = new();
            if (isAnnouncementQuery)
            {
                announcementRecommendations = await _announcementRecommendationService.GetAnnouncementRecommendationsAsync(user, userMessage);
            }

            // For greeting requests, return simple greeting without complex system prompts
            if (isFirstMessage && (userMessage.Contains("welcome", StringComparison.OrdinalIgnoreCase) || 
                                  userMessage.Contains("brief", StringComparison.OrdinalIgnoreCase) ||
                                  userMessage.Contains("hello", StringComparison.OrdinalIgnoreCase)))
            {
                return greeting;
            }

            // Generate context-aware system prompt
            string contextInfo = context != null ? _displayContextAwarenessService.GeneratePageContext(context) : "";
            
            string systemPrompt = profileSummary != null
                ? $"You are an AI assistant for eSkolar, a scholarship platform. You are helping a {profileSummary.Role.ToLower()}. " +
                  $"You have access to the user's profile information and should use it to answer their questions. " +
                  $"When they ask about their personal information (like 'what is my name?', 'what is my GPA?', 'what course am I taking?'), " +
                  $"answer using the specific data from their profile below. Be conversational and helpful.\n\n" +
                  
                  contextInfo +
                  
                  $"FORMATTING GUIDELINES (Apply to ALL content - scholarships, announcements, profiles, etc.):\n" +
                  $"• When listing items, use NUMBERED format: 1. Title, 2. Title, 3. Title\n" +
                  $"• DO NOT use bullet points (•) for main titles\n" +
                  $"• DO NOT put ** around main titles (but DO use ** for ALL property labels like **Author:**, **Match Score:**, **Posted:**)\n" +
                  $"• Use numbered lists (1. 2. 3.) for step-by-step instructions\n" +
                  $"• Add relevant emojis to make content easy to scan: 🎓📚💰📅⚠️📢✅❗👤🕒🏷️📌🏢📄\n" +
                  $"• Use ---- as separators between different items or sections\n" +
                  $"• Keep formatting clean and readable in a chat interface\n" +
                  $"• Use minimal line breaks between details to keep content compact\n" +
                  $"• Emphasize urgent deadlines, important announcements, and key information\n" +
                  $"• For ALL content types, use **Label:** format for properties (Author, Date, Category, etc.)\n\n" +
                  
                  $"When they ask about scholarships, recommendations, or what scholarships they're eligible for, " +
                  $"use the scholarship recommendations provided below and explain why each scholarship matches their profile. " +
                  $"Present scholarships in order of best match first.\n\n" +
                  $"When they ask about announcements, news, or updates, use the announcement recommendations provided below " +
                  $"and explain why each announcement is relevant to them. Present announcements by relevance.\n\n" +
                  $"Always be friendly, professional, and specific when referencing their profile data. Use emojis to make responses engaging and readable."
                
                : $"You are an AI assistant for eSkolar, a scholarship platform. " +
                  $"I don't have access to your profile information yet, but I'm here to help with general questions about scholarships and the platform. " +
                  $"Please use clear formatting with emojis and bullet points when presenting information.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt)
            };

            if (profileSummary != null)
            {
                // Create a more natural profile context with better formatting instructions
                var profileContext = $"📋 USER PROFILE INFORMATION:\n{profileSummary.Summary}\n\n" +
                                   $"FORMATTING INSTRUCTIONS FOR PROFILE DATA:\n" +
                                   $"• When presenting personal information, use clear labels and emojis\n" +
                                   $"• Format academic info with 📚, contact info with 📧📱, dates with 📅\n" +
                                   $"• Use simple bullet points for multiple items\n" +
                                   $"• Highlight important details like GPA, verification status\n" +
                                   $"• Present information in a conversational but organized way\n";
                messages.Add(new SystemChatMessage(profileContext));

                // Add scholarship recommendations if this is a scholarship-related query or user is a student
                if (profileSummary.Role == "Student" && (isScholarshipQuery || isFirstMessage || scholarshipRecommendations.Any()))
                {
                    var scholarshipContext = _contextGenerationService.GenerateScholarshipContext(scholarshipRecommendations);
                    if (!string.IsNullOrEmpty(scholarshipContext))
                    {
                        messages.Add(new SystemChatMessage(scholarshipContext));
                    }
                }

                // Add announcement recommendations if this is an announcement-related query
                if (profileSummary.Role == "Student" && isAnnouncementQuery && announcementRecommendations.Any())
                {
                    var announcementContext = _contextGenerationService.GenerateAnnouncementContext(announcementRecommendations);
                    if (!string.IsNullOrEmpty(announcementContext))
                    {
                        messages.Add(new SystemChatMessage(announcementContext));
                    }
                }
            }
            
            messages.Add(new UserChatMessage(userMessage));

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);
                var response = await chatClient.CompleteChatAsync(messages);
                stopwatch.Stop();
                
                // Track token usage with enhanced context
                var operationDetails = new
                {
                    HasProfile = profileSummary != null,
                    UserRole = profileSummary?.Role ?? "Unknown",
                    IsScholarshipQuery = isScholarshipQuery,
                    IsAnnouncementQuery = isAnnouncementQuery,
                    IsFirstMessage = isFirstMessage,
                    MessageCount = messages.Count,
                    UserMessageLength = userMessage.Length,
                    ScholarshipRecommendations = scholarshipRecommendations.Count,
                    AnnouncementRecommendations = announcementRecommendations.Count
                };
                
                await _tokenTrackingService.TrackChatCompletionUsageAsync(
                    completion: response.Value,
                    operation: "EnhancedChatCompletion",
                    userId: user?.Id,
                    additionalDetails: System.Text.Json.JsonSerializer.Serialize(operationDetails),
                    requestDuration: stopwatch.Elapsed
                );
                
                string aiResponse = response.Value.Content[0].Text.Trim();
                
                // Combine greeting with AI response if this is the first message
                if (isFirstMessage && !string.IsNullOrEmpty(greeting))
                {
                    return $"{greeting}\n\n{aiResponse}";
                }
                
                return aiResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Track failed request
                await _tokenTrackingService.TrackFailedRequestAsync(
                    operation: "EnhancedChatCompletion",
                    errorMessage: ex.Message,
                    userId: user?.Id,
                    requestDuration: stopwatch.Elapsed
                );
                
                throw; // Re-throw the exception
            }
        }


        private bool IsScholarshipRelatedQuery(string userMessage)
        {
            var scholarshipKeywords = new[] 
            { 
                "scholarship", "scholarships", "recommend", "recommendation", "eligible", 
                "apply", "funding", "financial aid", "grant", "grants", "money", 
                "tuition", "study", "education", "degree", "program", "course",
                "what scholarships", "find scholarship", "search scholarship", 
                "scholarship for me", "scholarship opportunities", "available scholarships",
                "match", "suitable", "qualify", "qualified"
            };
            
            return scholarshipKeywords.Any(keyword => 
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsAnnouncementRelatedQuery(string userMessage)
        {
            var announcementKeywords = new[]
            {
                "announcement", "announcements", "news", "updates", "latest", "recent",
                "what's new", "notifications", "alerts", "bulletin", "notice",
                "institution announcement", "school announcement", "university announcement",
                "application announcement", "deadline announcement", "requirement announcement",
                "grant announcement", "scholarship announcement", "funding announcement",
                "pinned announcement", "important announcement", "urgent announcement",
                "benefactor announcement", "sponsor announcement", "organization announcement",
                "all announcements", "any announcements", "show announcements"
            };

            return announcementKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private string GetGeneralFormattingInstructions()
        {
            return "💡 GENERAL FORMATTING GUIDELINES:\n" +
                   "• Use clear headings with emojis for different sections\n" +
                   "• Organize information in bullet points or numbered lists\n" +
                   "• Use visual separators (───) between different items or sections\n" +
                   "• Include relevant emojis to make content scannable and engaging\n" +
                   "• Use consistent indentation and spacing\n" +
                   "• Group related information together\n" +
                   "• Make urgent or important items stand out with warning emojis\n" +
                   "• Keep descriptions concise but informative\n" +
                   "• Always explain why information is relevant to the user\n";
        }
    }
}