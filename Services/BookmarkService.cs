using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace c2_eskolar.Services
{
    public class BookmarkService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<BookmarkService> _logger;

        public BookmarkService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<BookmarkService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        #region Core Bookmark Operations

        public async Task<BookmarkedScholarship?> AddBookmarkAsync(string userId, int scholarshipId, string reason = "Interested", int priority = 2)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // Check if already bookmarked
                var existing = await context.BookmarkedScholarships
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.ScholarshipId == scholarshipId);
                
                if (existing != null)
                {
                    _logger.LogWarning("Scholarship {ScholarshipId} already bookmarked by user {UserId}", scholarshipId, userId);
                    return existing;
                }

                // Get scholarship for match score calculation
                var scholarship = await context.Scholarships
                    .Include(s => s.Institution)
                    .Include(s => s.Benefactor)
                    .FirstOrDefaultAsync(s => s.ScholarshipId == scholarshipId);

                if (scholarship == null)
                {
                    _logger.LogError("Scholarship {ScholarshipId} not found", scholarshipId);
                    return null;
                }

                var matchScore = await CalculateMatchScoreAsync(userId, scholarship);
                var isUrgent = scholarship.ApplicationDeadline <= DateTime.Now.AddDays(7);

                var bookmark = new BookmarkedScholarship
                {
                    UserId = userId,
                    ScholarshipId = scholarshipId,
                    BookmarkReason = reason,
                    Priority = priority,
                    MatchScore = matchScore,
                    IsUrgent = isUrgent,
                    CreatedAt = DateTime.Now,
                    Status = BookmarkStatus.Bookmarked
                };

                context.BookmarkedScholarships.Add(bookmark);
                await context.SaveChangesAsync();

                _logger.LogInformation("Bookmark created for user {UserId}, scholarship {ScholarshipId}", userId, scholarshipId);
                return bookmark;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding bookmark for user {UserId}, scholarship {ScholarshipId}", userId, scholarshipId);
                return null;
            }
        }

        public async Task<bool> RemoveBookmarkAsync(string userId, int scholarshipId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var bookmark = await context.BookmarkedScholarships
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.ScholarshipId == scholarshipId);

                if (bookmark == null) return false;

                context.BookmarkedScholarships.Remove(bookmark);
                await context.SaveChangesAsync();

                _logger.LogInformation("Bookmark removed for user {UserId}, scholarship {ScholarshipId}", userId, scholarshipId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing bookmark for user {UserId}, scholarship {ScholarshipId}", userId, scholarshipId);
                return false;
            }
        }

        public async Task<bool> RemoveBookmarkByIdAsync(string userId, int bookmarkId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var bookmark = await context.BookmarkedScholarships
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.BookmarkId == bookmarkId);

                if (bookmark == null) return false;

                context.BookmarkedScholarships.Remove(bookmark);
                await context.SaveChangesAsync();

                _logger.LogInformation("Bookmark removed for user {UserId}, bookmark {BookmarkId}", userId, bookmarkId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing bookmark for user {UserId}, bookmark {BookmarkId}", userId, bookmarkId);
                return false;
            }
        }

        public async Task<bool> UpdateBookmarkStatusAsync(string userId, int scholarshipId, BookmarkStatus status)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var bookmark = await context.BookmarkedScholarships
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.ScholarshipId == scholarshipId);

                if (bookmark == null) return false;

                bookmark.Status = status;
                bookmark.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bookmark status for user {UserId}, scholarship {ScholarshipId}", userId, scholarshipId);
                return false;
            }
        }

        #endregion

        #region Organized Bookmark Retrieval

        public async Task<BookmarkOrganization> GetOrganizedBookmarksAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var allBookmarks = await context.BookmarkedScholarships
                    .Include(b => b.Scholarship)
                        .ThenInclude(s => s.Institution)
                    .Include(b => b.Scholarship)
                        .ThenInclude(s => s.Benefactor)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                var now = DateTime.Now;
                
                return new BookmarkOrganization
                {
                    AllBookmarks = allBookmarks,
                    UrgentDeadlines = allBookmarks
                        .Where(b => b.Scholarship.ApplicationDeadline <= now.AddDays(7) && 
                                   b.Scholarship.ApplicationDeadline > now &&
                                   b.Status != BookmarkStatus.Applied)
                        .OrderBy(b => b.Scholarship.ApplicationDeadline)
                        .ToList(),
                    HighMatch = allBookmarks
                        .Where(b => b.MatchScore >= 85)
                        .OrderByDescending(b => b.MatchScore)
                        .ToList(),
                    RecentlyAdded = allBookmarks
                        .Where(b => b.CreatedAt >= now.AddDays(-7))
                        .OrderByDescending(b => b.CreatedAt)
                        .ToList(),
                    InProgress = allBookmarks
                        .Where(b => b.Status == BookmarkStatus.InProgress || 
                                   b.Status == BookmarkStatus.ReadyToApply)
                        .OrderBy(b => b.Scholarship.ApplicationDeadline)
                        .ToList(),
                    Recommended = allBookmarks
                        .Where(b => b.MatchScore >= 80 && 
                                   b.Status == BookmarkStatus.Bookmarked)
                        .OrderByDescending(b => b.MatchScore)
                        .Take(10)
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organized bookmarks for user {UserId}", userId);
                return new BookmarkOrganization();
            }
        }

        #endregion

        #region Analytics and Insights

        public async Task<BookmarkAnalytics> GetBookmarkAnalyticsAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var bookmarks = await context.BookmarkedScholarships
                    .Include(b => b.Scholarship)
                    .Where(b => b.UserId == userId)
                    .ToListAsync();

                var applications = await context.ScholarshipApplications
                    .Include(a => a.Scholarship)
                    .Where(a => a.StudentProfileId == Guid.Parse("temp")) // You'll need to get actual student profile ID
                    .ToListAsync();

                var totalBookmarks = bookmarks.Count;
                var averageMatchScore = bookmarks.Any() ? bookmarks.Average(b => b.MatchScore) : 0;
                var bookmarksThisMonth = bookmarks.Count(b => b.CreatedAt >= DateTime.Now.AddMonths(-1));
                
                // Calculate category insights
                var categoryInsights = bookmarks
                    .GroupBy(b => GetScholarshipCategory(b.Scholarship))
                    .Select(g => new CategoryInsight
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        AverageMatchScore = g.Average(b => b.MatchScore),
                        ApplicationsFromCategory = applications.Count(a => GetScholarshipCategory(a.Scholarship) == g.Key),
                        SuccessRate = CalculateCategorySuccessRate(g.Key, applications)
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList();

                return new BookmarkAnalytics
                {
                    TotalBookmarks = totalBookmarks,
                    AverageMatchScore = averageMatchScore,
                    BookmarksThisMonth = bookmarksThisMonth,
                    ApplicationsFromBookmarks = applications.Count, // Simplified for now
                    CategoryInsights = categoryInsights,
                    TopCategories = categoryInsights.ToDictionary(c => c.Category, c => c.Count)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookmark analytics for user {UserId}", userId);
                return new BookmarkAnalytics();
            }
        }

        #endregion

        #region Smart Features

        public async Task<List<SmartBookmarkRecommendation>> GetSmartRecommendationsAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // Get user's existing bookmarks
                var existingBookmarkIds = await context.BookmarkedScholarships
                    .Where(b => b.UserId == userId)
                    .Select(b => b.ScholarshipId)
                    .ToListAsync();

                // Get active scholarships not yet bookmarked
                var availableScholarships = await context.Scholarships
                    .Include(s => s.Institution)
                    .Include(s => s.Benefactor)
                    .Where(s => s.IsActive && 
                               s.ApplicationDeadline > DateTime.Now &&
                               !existingBookmarkIds.Contains(s.ScholarshipId))
                    .ToListAsync();

                var recommendations = new List<SmartBookmarkRecommendation>();

                foreach (var scholarship in availableScholarships.Take(20)) // Limit for performance
                {
                    var matchScore = await CalculateMatchScoreAsync(userId, scholarship);
                    
                    if (matchScore >= 70) // Only recommend decent matches
                    {
                        var daysUntilDeadline = (scholarship.ApplicationDeadline - DateTime.Now).Days;
                        var urgencyLevel = GetUrgencyLevel(daysUntilDeadline);
                        
                        recommendations.Add(new SmartBookmarkRecommendation
                        {
                            Scholarship = scholarship,
                            MatchScore = matchScore,
                            RecommendationReason = GenerateRecommendationReason(matchScore, daysUntilDeadline),
                            MatchingCriteria = await GetMatchingCriteriaAsync(userId, scholarship),
                            IsAutoBookmarkCandidate = matchScore >= 90,
                            DaysUntilDeadline = daysUntilDeadline,
                            UrgencyLevel = urgencyLevel
                        });
                    }
                }

                return recommendations
                    .OrderByDescending(r => r.MatchScore)
                    .ThenBy(r => r.DaysUntilDeadline)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting smart recommendations for user {UserId}", userId);
                return new List<SmartBookmarkRecommendation>();
            }
        }

        public async Task AutoBookmarkHighMatchesAsync(string userId, decimal threshold = 90)
        {
            try
            {
                var recommendations = await GetSmartRecommendationsAsync(userId);
                var autoBookmarkCandidates = recommendations
                    .Where(r => r.MatchScore >= threshold && r.IsAutoBookmarkCandidate)
                    .ToList();

                foreach (var candidate in autoBookmarkCandidates)
                {
                    await AddBookmarkAsync(userId, candidate.Scholarship.ScholarshipId, 
                        "Auto-bookmarked: High Match", 1);
                }

                _logger.LogInformation("Auto-bookmarked {Count} scholarships for user {UserId}", 
                    autoBookmarkCandidates.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-bookmarking for user {UserId}", userId);
            }
        }

        #endregion

        #region Helper Methods

        private async Task<decimal> CalculateMatchScoreAsync(string userId, Scholarship scholarship)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // Get student profile
                var studentProfile = await context.StudentProfiles
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (studentProfile == null) return 0;

                decimal score = 0;
                int criteria = 0;

                // GPA matching
                if (scholarship.MinimumGPA.HasValue && studentProfile.GPA.HasValue)
                {
                    criteria++;
                    if (studentProfile.GPA >= scholarship.MinimumGPA)
                    {
                        score += 25; // Major factor
                    }
                }

                // Course matching
                if (!string.IsNullOrEmpty(scholarship.RequiredCourse) && !string.IsNullOrEmpty(studentProfile.Course))
                {
                    criteria++;
                    if (studentProfile.Course.Contains(scholarship.RequiredCourse, StringComparison.OrdinalIgnoreCase) ||
                        scholarship.RequiredCourse.Contains(studentProfile.Course, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 30; // High importance
                    }
                }

                // University matching
                if (!string.IsNullOrEmpty(scholarship.RequiredUniversity) && !string.IsNullOrEmpty(studentProfile.UniversityName))
                {
                    criteria++;
                    if (studentProfile.UniversityName.Contains(scholarship.RequiredUniversity, StringComparison.OrdinalIgnoreCase) ||
                        scholarship.RequiredUniversity.Contains(studentProfile.UniversityName, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 25;
                    }
                }

                // Year level matching
                if (scholarship.RequiredYearLevel.HasValue && studentProfile.YearLevel.HasValue)
                {
                    criteria++;
                    if (studentProfile.YearLevel >= scholarship.RequiredYearLevel)
                    {
                        score += 20;
                    }
                }

                // If we have criteria, normalize the score
                if (criteria > 0)
                {
                    score = (score / (criteria * 25)) * 100; // Normalize to 0-100
                }
                else
                {
                    score = 50; // Default score when no specific criteria
                }

                return Math.Min(100, Math.Max(0, score));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating match score for user {UserId}, scholarship {ScholarshipId}", 
                    userId, scholarship.ScholarshipId);
                return 0;
            }
        }

        private string GetScholarshipCategory(Scholarship scholarship)
        {
            if (string.IsNullOrEmpty(scholarship.RequiredCourse)) return "General";
            
            var course = scholarship.RequiredCourse.ToLower();
            
            if (course.Contains("computer") || course.Contains("it") || course.Contains("technology"))
                return "Technology";
            if (course.Contains("medicine") || course.Contains("nursing") || course.Contains("health"))
                return "Healthcare";
            if (course.Contains("business") || course.Contains("management") || course.Contains("finance"))
                return "Business";
            if (course.Contains("engineering"))
                return "Engineering";
            if (course.Contains("education") || course.Contains("teaching"))
                return "Education";
            
            return "General";
        }

        private decimal CalculateCategorySuccessRate(string category, List<ScholarshipApplication> applications)
        {
            var categoryApps = applications.Where(a => GetScholarshipCategory(a.Scholarship) == category).ToList();
            if (!categoryApps.Any()) return 0;
            
            var successful = categoryApps.Count(a => a.Status == "Approved");
            return (decimal)successful / categoryApps.Count * 100;
        }

        private string GetUrgencyLevel(int daysUntilDeadline)
        {
            return daysUntilDeadline switch
            {
                <= 3 => "Critical",
                <= 7 => "High",
                <= 14 => "Normal",
                _ => "Low"
            };
        }

        private string GenerateRecommendationReason(decimal matchScore, int daysUntilDeadline)
        {
            var reasons = new List<string>();
            
            if (matchScore >= 90) reasons.Add("Excellent match for your profile");
            else if (matchScore >= 80) reasons.Add("Good match for your qualifications");
            
            if (daysUntilDeadline <= 7) reasons.Add("Deadline approaching soon");
            
            return string.Join(", ", reasons);
        }

        private async Task<List<string>> GetMatchingCriteriaAsync(string userId, Scholarship scholarship)
        {
            var criteria = new List<string>();
            
            using var context = _contextFactory.CreateDbContext();
            var studentProfile = await context.StudentProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (studentProfile == null) return criteria;

            if (!string.IsNullOrEmpty(scholarship.RequiredCourse) && 
                !string.IsNullOrEmpty(studentProfile.Course) &&
                studentProfile.Course.Contains(scholarship.RequiredCourse, StringComparison.OrdinalIgnoreCase))
            {
                criteria.Add("Course requirement");
            }

            if (scholarship.MinimumGPA.HasValue && 
                studentProfile.GPA.HasValue && 
                studentProfile.GPA >= scholarship.MinimumGPA)
            {
                criteria.Add("GPA requirement");
            }

            return criteria;
        }

        #endregion

        #region Announcement Bookmark Operations

        public async Task<BookmarkedAnnouncement?> AddAnnouncementBookmarkAsync(string userId, Guid announcementId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // Check if already bookmarked
                var existing = await context.BookmarkedAnnouncements
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.AnnouncementId == announcementId);
                
                if (existing != null)
                {
                    _logger.LogWarning("Announcement {AnnouncementId} already bookmarked by user {UserId}", announcementId, userId);
                    return existing;
                }

                var announcement = await context.Announcements
                    .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);

                if (announcement == null)
                {
                    _logger.LogError("Announcement {AnnouncementId} not found", announcementId);
                    return null;
                }

                var bookmark = new BookmarkedAnnouncement
                {
                    UserId = userId,
                    AnnouncementId = announcementId,
                    CreatedAt = DateTime.Now
                };

                context.BookmarkedAnnouncements.Add(bookmark);
                await context.SaveChangesAsync();

                _logger.LogInformation("Announcement bookmarked for user {UserId}, announcement {AnnouncementId}", userId, announcementId);
                return bookmark;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding announcement bookmark for user {UserId}, announcement {AnnouncementId}", userId, announcementId);
                return null;
            }
        }

        public async Task<bool> RemoveAnnouncementBookmarkAsync(string userId, Guid announcementId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var bookmark = await context.BookmarkedAnnouncements
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.AnnouncementId == announcementId);

                if (bookmark == null) return false;

                context.BookmarkedAnnouncements.Remove(bookmark);
                await context.SaveChangesAsync();

                _logger.LogInformation("Announcement bookmark removed for user {UserId}, announcement {AnnouncementId}", userId, announcementId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing announcement bookmark for user {UserId}, announcement {AnnouncementId}", userId, announcementId);
                return false;
            }
        }

        public async Task<List<BookmarkedAnnouncement>> GetUserAnnouncementBookmarksAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                return await context.BookmarkedAnnouncements
                    .Include(b => b.Announcement)
                    .ThenInclude(a => a.Photos) // Include photos for announcements
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting announcement bookmarks for user {UserId}", userId);
                return new List<BookmarkedAnnouncement>();
            }
        }

        public async Task<bool> IsAnnouncementBookmarkedAsync(string userId, Guid announcementId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                return await context.BookmarkedAnnouncements
                    .AnyAsync(b => b.UserId == userId && b.AnnouncementId == announcementId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking announcement bookmark status for user {UserId}, announcement {AnnouncementId}", userId, announcementId);
                return false;
            }
        }

        public async Task<List<Guid>> GetBookmarkedAnnouncementIdsAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                return await context.BookmarkedAnnouncements
                    .Where(b => b.UserId == userId)
                    .Select(b => b.AnnouncementId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookmarked announcement IDs for user {UserId}", userId);
                return new List<Guid>();
            }
        }

        #endregion
    }
}