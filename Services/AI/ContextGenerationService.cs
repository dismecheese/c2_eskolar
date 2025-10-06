using System;
using System.Collections.Generic;
using System.Linq;
using c2_eskolar.Models;

namespace c2_eskolar.Services.AI
{
    public class ContextGenerationService
    {
        public string GenerateScholarshipContext(List<ScholarshipRecommendation> recommendations)
        {
            if (!recommendations.Any())
            {
                return "No scholarship recommendations available at this time. The user should check if their profile is complete and if there are active scholarships in the system.";
            }

            var context = "🎓 SCHOLARSHIP RECOMMENDATIONS (Ranked by Compatibility)\n\n";
            context += "FORMATTING INSTRUCTIONS: Present scholarships with clear titles and organized details:\n\n";
            
            for (int i = 0; i < Math.Min(recommendations.Count, 5); i++) // Limit to top 5 to avoid token limits
            {
                var rec = recommendations[i];
                var scholarship = rec.Scholarship;
                
                // Calculate urgency for deadline
                var daysUntilDeadline = (scholarship.ApplicationDeadline - DateTime.Now).Days;
                var urgencyEmoji = daysUntilDeadline <= 7 ? "🚨" : daysUntilDeadline <= 30 ? "⚠️" : "📅";
                
                context += $"{i + 1}. {scholarship.Title}\n";
                context += $"\uD83D\uDCCA **Match Score:** {rec.MatchScore}% {new string('★', rec.MatchScore / 20)}\n";
                context += $"\uD83D\uDCB0 **Benefits:** {scholarship.Benefits}\n";
                context += $"{urgencyEmoji} **Deadline:** {scholarship.ApplicationDeadline:MMMM dd, yyyy}";
                if (daysUntilDeadline <= 30)
                {
                    context += $" ({daysUntilDeadline} days remaining!)";
                }
                context += "\n";
                if (scholarship.MinimumGPA.HasValue)
                    context += $"\uD83D\uDD09 **Min GPA:** {scholarship.MinimumGPA:F2}\n";
                if (!string.IsNullOrEmpty(scholarship.RequiredCourse))
                    context += $"\uD83D\uDCD6 **Course:** {scholarship.RequiredCourse}\n";
                if (scholarship.RequiredYearLevel.HasValue)
                    context += $"\uD83C\uDFAF **Year Level:** {scholarship.RequiredYearLevel}\n";
                if (!string.IsNullOrEmpty(scholarship.RequiredUniversity))
                    context += $"\uD83C\uDFEB **University:** {scholarship.RequiredUniversity}\n";
                if (rec.MatchReasons.Any())
                {
                    context += $"✅ **Why It Matches:** {string.Join(", ", rec.MatchReasons)}\n";
                }
                if (!string.IsNullOrEmpty(scholarship.Description))
                {
                    var shortDesc = scholarship.Description.Length > 150 
                        ? scholarship.Description.Substring(0, 150) + "..." 
                        : scholarship.Description;
                    context += $"\uD83D\uDCDD **Description:** {shortDesc}\n";
                }
                context += "----\n";
            }

            if (recommendations.Count > 5)
            {
                context += $"📋 Plus {recommendations.Count - 5} more scholarships available!";
            }

            context += "PRESENTATION GUIDELINES:\n";
            context += "• Use simple bullet points and clear spacing\n";
            context += "• Highlight urgent deadlines with warning emojis\n";
            context += "• Explain match reasons clearly\n";
            context += "• Group related information together\n";
            context += "• Use emojis to make information scannable\n";
            context += "• Present in order of best match first";
            
            return context;
        }

        public string GenerateAnnouncementContext(List<AnnouncementRecommendation> recommendations)
        {
            if (!recommendations.Any())
            {
                return "No relevant announcements found at this time. Check back later for updates.";
            }

            var context = "📢 LATEST ANNOUNCEMENTS (Ranked by Relevance)\n\n";
            context += "FORMATTING INSTRUCTIONS: Present announcements with clear titles and organized details:\n\n";

            for (int i = 0; i < Math.Min(recommendations.Count, 5); i++) // Limit to top 5 to avoid token limits
            {
                var rec = recommendations[i];
                var announcement = rec.Announcement;

                // Determine announcement importance
                var importanceEmoji = announcement.IsPinned ? "📌" : 
                                    announcement.Priority > AnnouncementPriority.Normal ? "❗" : "📋";
                
                // Calculate recency
                var daysAgo = (DateTime.Now - announcement.CreatedAt).Days;
                var recencyIndicator = daysAgo == 0 ? "🆕 TODAY" : 
                                     daysAgo <= 3 ? $"🔥 {daysAgo} days ago" : 
                                     $"📅 {daysAgo} days ago";

                context += $"{i + 1}. {announcement.Title}\n";
                context += $"👤 **Author:** {announcement.AuthorName} ({announcement.AuthorType})\n";
                context += $"🕒 **Posted:** {recencyIndicator} ({announcement.CreatedAt:MMM dd, yyyy})\n";
                context += $"🎯 **Relevance:** {rec.RelevanceScore}% {new string('⭐', rec.RelevanceScore / 20)}\n";

                if (!string.IsNullOrEmpty(announcement.Category))
                    context += $"🏷️ **Category:** {announcement.Category}\n";

                if (announcement.IsPinned)
                    context += $"📌 **Status:** PINNED (Important)\n";

                if (announcement.Priority > AnnouncementPriority.Normal)
                    context += $"⚠️ **Priority:** {announcement.Priority}\n";

                if (!string.IsNullOrEmpty(announcement.OrganizationName))
                    context += $"🏢 **Organization:** {announcement.OrganizationName}\n";

                if (rec.MatchReasons.Any())
                {
                    context += $"✅ **Why It's Relevant:** {string.Join(", ", rec.MatchReasons)}\n";
                }

                // Add announcement summary or truncated content
                var content = !string.IsNullOrEmpty(announcement.Summary) 
                    ? announcement.Summary 
                    : announcement.Content;
                
                if (!string.IsNullOrEmpty(content))
                {
                    var shortContent = content.Length > 150 
                        ? content.Substring(0, 150) + "..." 
                        : content;
                    context += $"📄 **Summary:** {shortContent}\n";
                }

                context += "\n---\n\n";
            }

            if (recommendations.Count > 5)
            {
                context += $"📋 Plus {recommendations.Count - 5} more announcements available!\n\n";
            }

            context += "PRESENTATION GUIDELINES:\n";
            context += "• Use clear headings and bullet points\n";
            context += "• Highlight pinned/important announcements\n";
            context += "• Show recency with appropriate emphasis\n";
            context += "• Group information logically\n";
            context += "• Use emojis to improve readability\n";
            context += "• Explain relevance to the user clearly";

            return context;
        }
    }
}