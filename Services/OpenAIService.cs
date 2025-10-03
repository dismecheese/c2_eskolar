
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using c2_eskolar.Models;

namespace c2_eskolar.Services
{
    public class OpenAIService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;
        private readonly ProfileSummaryService _profileSummaryService;
        private readonly ScholarshipRecommendationService _scholarshipRecommendationService;
        private readonly AnnouncementRecommendationService _announcementRecommendationService;

        public OpenAIService(IConfiguration config, ProfileSummaryService profileSummaryService, ScholarshipRecommendationService scholarshipRecommendationService, AnnouncementRecommendationService announcementRecommendationService)
        {
            var apiKey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey");
            var endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
            _deploymentName = config["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName");
            _client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            _profileSummaryService = profileSummaryService;
            _scholarshipRecommendationService = scholarshipRecommendationService;
            _announcementRecommendationService = announcementRecommendationService;
        }

        public async Task<string> GetChatCompletionAsync(string userMessage)
        {
            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new[]
            {
                new UserChatMessage(userMessage)
            };
            var response = await chatClient.CompleteChatAsync(messages);
            return response.Value.Content[0].Text.Trim();
        }

        // New method: includes the user's profile summary in the prompt
        public async Task<string> GetChatCompletionWithProfileAsync(string userMessage, IdentityUser user, bool isFirstMessage = false)
        {
            var profileSummary = await _profileSummaryService.GetProfileSummaryAsync(user);
            var firstName = await _profileSummaryService.GetUserFirstNameAsync(user);
            var scholarshipRecommendations = await _scholarshipRecommendationService.GetScholarshipRecommendationsAsync(user);
            
            // Create a personalized greeting only for the first message
            string greeting = "";
            if (isFirstMessage)
            {
                greeting = !string.IsNullOrEmpty(firstName) 
                    ? $"Hello {firstName}! How can I help you today?\n\n"
                    : "Hello! How can I help you today?\n\n";
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

            string systemPrompt = profileSummary != null
                ? $"You are an AI assistant for eSkolar, a scholarship platform. You are helping a {profileSummary.Role.ToLower()}. " +
                  (isFirstMessage ? $"Start your response with: '{greeting.Trim()}'\n\n" : "") +
                  $"You have access to the user's profile information and should use it to answer their questions. " +
                  $"When they ask about their personal information (like 'what is my name?', 'what is my GPA?', 'what course am I taking?'), " +
                  $"answer using the specific data from their profile below. Be conversational and helpful.\n\n" +
                  $"When they ask about scholarships, recommendations, or what scholarships they're eligible for, " +
                  $"use the scholarship recommendations provided below and explain why each scholarship matches their profile. " +
                  $"Present scholarships in order of best match first.\n\n" +
                  $"When they ask about announcements, news, or updates, use the announcement recommendations provided below " +
                  $"and explain why each announcement is relevant to them. Present announcements by relevance.\n\n" +
                  $"Always be friendly, professional, and specific when referencing their profile data."
                : (isFirstMessage 
                    ? $"You are an AI assistant for eSkolar, a scholarship platform. Start your response with: '{greeting.Trim()}' " +
                      $"I don't have access to your profile information yet, but I'm here to help with general questions about scholarships and the platform."
                    : "You are an AI assistant for eSkolar, a scholarship platform. Answer questions as best you can.");

            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt)
            };

            if (profileSummary != null)
            {
                // Create a more natural profile context
                var profileContext = $"User Profile Information:\n{profileSummary.Summary}";
                messages.Add(new SystemChatMessage(profileContext));

                // Add scholarship recommendations if this is a scholarship-related query or user is a student
                if (profileSummary.Role == "Student" && (isScholarshipQuery || isFirstMessage || scholarshipRecommendations.Any()))
                {
                    var scholarshipContext = GenerateScholarshipContext(scholarshipRecommendations);
                    if (!string.IsNullOrEmpty(scholarshipContext))
                    {
                        messages.Add(new SystemChatMessage(scholarshipContext));
                    }
                }

                // Add announcement recommendations if this is an announcement-related query
                if (profileSummary.Role == "Student" && isAnnouncementQuery && announcementRecommendations.Any())
                {
                    var announcementContext = GenerateAnnouncementContext(announcementRecommendations);
                    if (!string.IsNullOrEmpty(announcementContext))
                    {
                        messages.Add(new SystemChatMessage(announcementContext));
                    }
                }
            }
            
            messages.Add(new UserChatMessage(userMessage));

            var response = await chatClient.CompleteChatAsync(messages);
            return response.Value.Content[0].Text.Trim();
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

        private string GenerateScholarshipContext(List<ScholarshipRecommendation> recommendations)
        {
            if (!recommendations.Any())
            {
                return "No scholarship recommendations available at this time. The user should check if their profile is complete and if there are active scholarships in the system.";
            }

            var context = "Scholarship Recommendations (ranked by compatibility):\n\n";
            
            for (int i = 0; i < Math.Min(recommendations.Count, 5); i++) // Limit to top 5 to avoid token limits
            {
                var rec = recommendations[i];
                var scholarship = rec.Scholarship;
                
                context += $"{i + 1}. **{scholarship.Title}**\n";
                context += $"   - Match Score: {rec.MatchScore}%\n";
                context += $"   - Benefits: {scholarship.Benefits}\n";
                context += $"   - Deadline: {scholarship.ApplicationDeadline:MMMM dd, yyyy}\n";
                
                if (scholarship.MinimumGPA.HasValue)
                    context += $"   - Minimum GPA: {scholarship.MinimumGPA:F2}\n";
                
                if (!string.IsNullOrEmpty(scholarship.RequiredCourse))
                    context += $"   - Required Course: {scholarship.RequiredCourse}\n";
                
                if (scholarship.RequiredYearLevel.HasValue)
                    context += $"   - Year Level: {scholarship.RequiredYearLevel}\n";
                
                if (!string.IsNullOrEmpty(scholarship.RequiredUniversity))
                    context += $"   - University: {scholarship.RequiredUniversity}\n";
                
                if (rec.MatchReasons.Any())
                {
                    context += $"   - Why it matches: {string.Join("; ", rec.MatchReasons)}\n";
                }
                
                if (!string.IsNullOrEmpty(scholarship.Description))
                {
                    var shortDesc = scholarship.Description.Length > 200 
                        ? scholarship.Description.Substring(0, 200) + "..." 
                        : scholarship.Description;
                    context += $"   - Description: {shortDesc}\n";
                }
                
                context += "\n";
            }

            if (recommendations.Count > 5)
            {
                context += $"... and {recommendations.Count - 5} more scholarships available.\n\n";
            }

            context += "When presenting these scholarships, explain why each one is a good match for the user based on their profile and the match reasons provided.";
            
            return context;
        }

        private string GenerateAnnouncementContext(List<AnnouncementRecommendation> recommendations)
        {
            if (!recommendations.Any())
            {
                return "No relevant announcements found at this time. Check back later for updates.";
            }

            var context = "Relevant Announcements (ranked by relevance):\n\n";

            for (int i = 0; i < Math.Min(recommendations.Count, 5); i++) // Limit to top 5 to avoid token limits
            {
                var rec = recommendations[i];
                var announcement = rec.Announcement;

                context += $"{i + 1}. **{announcement.Title}**\n";
                context += $"   - Author: {announcement.AuthorName} ({announcement.AuthorType})\n";
                context += $"   - Posted: {announcement.CreatedAt:MMMM dd, yyyy}\n";
                context += $"   - Relevance Score: {rec.RelevanceScore}\n";

                if (!string.IsNullOrEmpty(announcement.Category))
                    context += $"   - Category: {announcement.Category}\n";

                if (announcement.IsPinned)
                    context += $"   - Status: PINNED (Important)\n";

                if (announcement.Priority > AnnouncementPriority.Normal)
                    context += $"   - Priority: {announcement.Priority}\n";

                if (!string.IsNullOrEmpty(announcement.OrganizationName))
                    context += $"   - Organization: {announcement.OrganizationName}\n";

                if (rec.MatchReasons.Any())
                {
                    context += $"   - Why it's relevant: {string.Join("; ", rec.MatchReasons)}\n";
                }

                // Add announcement summary or truncated content
                var content = !string.IsNullOrEmpty(announcement.Summary) 
                    ? announcement.Summary 
                    : announcement.Content;
                
                if (!string.IsNullOrEmpty(content))
                {
                    var shortContent = content.Length > 200 
                        ? content.Substring(0, 200) + "..." 
                        : content;
                    context += $"   - Summary: {shortContent}\n";
                }

                context += "\n";
            }

            if (recommendations.Count > 5)
            {
                context += $"... and {recommendations.Count - 5} more announcements available.\n\n";
            }

            context += "When presenting these announcements, explain why each one is relevant to the user and highlight the key information they should know.";

            return context;
        }
    }
}