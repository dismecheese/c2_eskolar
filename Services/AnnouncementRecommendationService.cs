using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace c2_eskolar.Services
{
    public class AnnouncementRecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AnnouncementRecommendationService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<AnnouncementRecommendation>> GetAnnouncementRecommendationsAsync(IdentityUser user, string query)
        {
            if (user == null) return new List<AnnouncementRecommendation>();
            
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Student")) return new List<AnnouncementRecommendation>();

            var studentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (studentProfile == null) return new List<AnnouncementRecommendation>();

            // Get active announcements
            var activeAnnouncements = await _context.Announcements
                .Where(a => a.IsActive && 
                           (a.PublishDate == null || a.PublishDate <= DateTime.UtcNow) &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .ToListAsync();

            var recommendations = new List<AnnouncementRecommendation>();
            var queryType = DetermineQueryType(query);

            foreach (var announcement in activeAnnouncements)
            {
                var relevanceScore = CalculateRelevanceScore(announcement, studentProfile, query, queryType);
                var matchReasons = GenerateMatchReasons(announcement, studentProfile, queryType);
                
                if (relevanceScore > 0)
                {
                    recommendations.Add(new AnnouncementRecommendation
                    {
                        Announcement = announcement,
                        RelevanceScore = relevanceScore,
                        MatchReasons = matchReasons,
                        QueryType = queryType
                    });
                }
            }

            return recommendations.OrderByDescending(r => r.RelevanceScore).Take(20).ToList();
        }

        private AnnouncementQueryType DetermineQueryType(string query)
        {
            var lowerQuery = query.ToLower();

            // Institution/School announcements
            if (ContainsKeywords(lowerQuery, new[] { "institution", "school", "university", "college", "my school", "my university", "institutional" }))
            {
                return AnnouncementQueryType.Institution;
            }

            // Application-related
            if (ContainsKeywords(lowerQuery, new[] { "application", "apply", "applications", "deadline", "requirements", "submit", "application process" }))
            {
                return AnnouncementQueryType.Application;
            }

            // Grants and scholarships
            if (ContainsKeywords(lowerQuery, new[] { "grant", "grants", "scholarship", "scholarships", "funding", "financial aid", "award", "money" }))
            {
                return AnnouncementQueryType.Grants;
            }

            // Pinned announcements
            if (ContainsKeywords(lowerQuery, new[] { "pinned", "important", "urgent", "priority", "featured", "highlighted" }))
            {
                return AnnouncementQueryType.Pinned;
            }

            // Benefactor announcements
            if (ContainsKeywords(lowerQuery, new[] { "benefactor", "sponsor", "donor", "organization", "company", "corporate", "foundation" }))
            {
                return AnnouncementQueryType.Benefactor;
            }

            // All announcements
            if (ContainsKeywords(lowerQuery, new[] { "all", "everything", "any", "latest", "recent", "news", "updates" }))
            {
                return AnnouncementQueryType.All;
            }

            // Default to general search
            return AnnouncementQueryType.General;
        }

        private bool ContainsKeywords(string text, string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword));
        }

        private int CalculateRelevanceScore(Announcement announcement, StudentProfile student, string query, AnnouncementQueryType queryType)
        {
            int score = 0;

            // Base score for active announcement
            score += 10;

            // Query type specific scoring
            switch (queryType)
            {
                case AnnouncementQueryType.Institution:
                    if (announcement.AuthorType == UserRole.Institution)
                    {
                        score += 50;
                        // Higher score if it's from the student's university
                        if (!string.IsNullOrEmpty(student.UniversityName) && 
                            !string.IsNullOrEmpty(announcement.OrganizationName) &&
                            announcement.OrganizationName.Contains(student.UniversityName, StringComparison.OrdinalIgnoreCase))
                        {
                            score += 30;
                        }
                    }
                    break;

                case AnnouncementQueryType.Application:
                    if (ContainsKeywords(announcement.Title.ToLower(), new[] { "application", "deadline", "requirements", "submit" }) ||
                        ContainsKeywords(announcement.Content.ToLower(), new[] { "application", "deadline", "requirements", "submit" }) ||
                        announcement.Category?.ToLower().Contains("application") == true)
                    {
                        score += 40;
                    }
                    break;

                case AnnouncementQueryType.Grants:
                    if (ContainsKeywords(announcement.Title.ToLower(), new[] { "grant", "scholarship", "funding", "award" }) ||
                        ContainsKeywords(announcement.Content.ToLower(), new[] { "grant", "scholarship", "funding", "award" }) ||
                        announcement.Category?.ToLower().Contains("grant") == true ||
                        announcement.Category?.ToLower().Contains("scholarship") == true)
                    {
                        score += 40;
                    }
                    break;

                case AnnouncementQueryType.Pinned:
                    if (announcement.IsPinned)
                    {
                        score += 50;
                    }
                    break;

                case AnnouncementQueryType.Benefactor:
                    if (announcement.AuthorType == UserRole.Benefactor)
                    {
                        score += 40;
                    }
                    break;

                case AnnouncementQueryType.All:
                    score += 20; // Base relevance for all announcements
                    break;

                case AnnouncementQueryType.General:
                    // Text search in title and content
                    var queryWords = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in queryWords)
                    {
                        if (announcement.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
                            score += 15;
                        if (announcement.Content.Contains(word, StringComparison.OrdinalIgnoreCase))
                            score += 10;
                        if (announcement.Tags?.Contains(word, StringComparison.OrdinalIgnoreCase) == true)
                            score += 8;
                        if (announcement.Category?.Contains(word, StringComparison.OrdinalIgnoreCase) == true)
                            score += 12;
                    }
                    break;
            }

            // Priority boost
            score += (int)announcement.Priority * 5;

            // Pinned boost (for all query types)
            if (announcement.IsPinned)
            {
                score += 15;
            }

            // Recent announcement boost
            var daysSinceCreated = (DateTime.UtcNow - announcement.CreatedAt).TotalDays;
            if (daysSinceCreated <= 7)
            {
                score += 10; // Recent announcements get priority
            }
            else if (daysSinceCreated <= 30)
            {
                score += 5;
            }

            // Public visibility check
            if (!announcement.IsPublic)
            {
                // Check if student matches target audience
                if (string.IsNullOrEmpty(announcement.TargetAudience))
                {
                    score = 0; // Not public and no target audience means not visible
                }
                else
                {
                    // Simple target audience check (could be enhanced with JSON parsing)
                    var targetAudience = announcement.TargetAudience.ToLower();
                    if (!string.IsNullOrEmpty(student.Course) && 
                        targetAudience.Contains(student.Course.ToLower()))
                    {
                        score += 10; // Targeted content bonus
                    }
                    else if (!string.IsNullOrEmpty(student.UniversityName) && 
                             targetAudience.Contains(student.UniversityName.ToLower()))
                    {
                        score += 10; // University-specific content bonus
                    }
                    else
                    {
                        score = 0; // Not in target audience
                    }
                }
            }

            return score;
        }

        private List<string> GenerateMatchReasons(Announcement announcement, StudentProfile student, AnnouncementQueryType queryType)
        {
            var reasons = new List<string>();

            switch (queryType)
            {
                case AnnouncementQueryType.Institution:
                    if (announcement.AuthorType == UserRole.Institution)
                    {
                        if (!string.IsNullOrEmpty(student.UniversityName) && 
                            !string.IsNullOrEmpty(announcement.OrganizationName) &&
                            announcement.OrganizationName.Contains(student.UniversityName, StringComparison.OrdinalIgnoreCase))
                        {
                            reasons.Add($"Announcement from your university: {announcement.OrganizationName}");
                        }
                        else
                        {
                            reasons.Add($"Institutional announcement from {announcement.OrganizationName ?? announcement.AuthorName}");
                        }
                    }
                    break;

                case AnnouncementQueryType.Application:
                    reasons.Add("Contains application or deadline information");
                    if (!string.IsNullOrEmpty(announcement.Category))
                    {
                        reasons.Add($"Category: {announcement.Category}");
                    }
                    break;

                case AnnouncementQueryType.Grants:
                    reasons.Add("Related to grants, scholarships, or funding opportunities");
                    break;

                case AnnouncementQueryType.Pinned:
                    if (announcement.IsPinned)
                    {
                        reasons.Add("This is a pinned important announcement");
                    }
                    break;

                case AnnouncementQueryType.Benefactor:
                    if (announcement.AuthorType == UserRole.Benefactor)
                    {
                        reasons.Add($"Announcement from benefactor: {announcement.AuthorName}");
                    }
                    break;

                case AnnouncementQueryType.All:
                    reasons.Add($"Recent announcement from {announcement.AuthorName}");
                    break;
            }

            // Common reasons
            if (announcement.IsPinned && queryType != AnnouncementQueryType.Pinned)
            {
                reasons.Add("Pinned as important");
            }

            if (announcement.Priority > AnnouncementPriority.Normal)
            {
                reasons.Add($"High priority ({announcement.Priority})");
            }

            var daysSinceCreated = (DateTime.UtcNow - announcement.CreatedAt).TotalDays;
            if (daysSinceCreated <= 7)
            {
                reasons.Add("Posted recently this week");
            }

            if (!announcement.IsPublic && !string.IsNullOrEmpty(announcement.TargetAudience))
            {
                reasons.Add("Specifically targeted to students like you");
            }

            return reasons;
        }
    }

    public class AnnouncementRecommendation
    {
        public Announcement Announcement { get; set; } = null!;
        public int RelevanceScore { get; set; }
        public List<string> MatchReasons { get; set; } = new List<string>();
        public AnnouncementQueryType QueryType { get; set; }
    }

    public enum AnnouncementQueryType
    {
        Institution,
        Application,
        Grants,
        Pinned,
        Benefactor,
        All,
        General
    }
}