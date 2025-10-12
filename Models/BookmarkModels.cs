using System;
using System.Collections.Generic;

namespace c2_eskolar.Models
{
    // Organization model for grouped bookmarks
    public class BookmarkOrganization
    {
        public List<BookmarkedScholarship> UrgentDeadlines { get; set; } = new();
        public List<BookmarkedScholarship> HighMatch { get; set; } = new();
        public List<BookmarkedScholarship> RecentlyAdded { get; set; } = new();
        public List<BookmarkedScholarship> InProgress { get; set; } = new();
        public List<BookmarkedScholarship> Recommended { get; set; } = new();
        public List<BookmarkedScholarship> AllBookmarks { get; set; } = new();
    }
    
    // Analytics model for bookmark insights
    public class BookmarkAnalytics
    {
        public int TotalBookmarks { get; set; }
        public decimal AverageMatchScore { get; set; }
        public decimal ApplicationSuccessRate { get; set; }
        public Dictionary<string, int> TopCategories { get; set; } = new();
        public int AverageTimeToApplication { get; set; } // in days
        public int BookmarksThisMonth { get; set; }
        public int ApplicationsFromBookmarks { get; set; }
        public List<BookmarkTrend> MonthlyTrends { get; set; } = new();
        public List<CategoryInsight> CategoryInsights { get; set; } = new();
    }
    
    // Trend data for charts
    public class BookmarkTrend
    {
        public string Month { get; set; } = "";
        public int BookmarksAdded { get; set; }
        public int ApplicationsSubmitted { get; set; }
        public decimal AverageMatchScore { get; set; }
    }
    
    // Category insights
    public class CategoryInsight
    {
        public string Category { get; set; } = "";
        public int Count { get; set; }
        public decimal AverageMatchScore { get; set; }
        public int ApplicationsFromCategory { get; set; }
        public decimal SuccessRate { get; set; }
    }
    
    // Smart recommendation model
    public class SmartBookmarkRecommendation
    {
        public Scholarship Scholarship { get; set; } = null!;
        public decimal MatchScore { get; set; }
        public string RecommendationReason { get; set; } = "";
        public List<string> MatchingCriteria { get; set; } = new();
        public bool IsAutoBookmarkCandidate { get; set; }
        public int DaysUntilDeadline { get; set; }
        public string UrgencyLevel { get; set; } = "Normal"; // Low, Normal, High, Critical
    }
    
    // Notification preferences
    public class BookmarkNotificationSettings
    {
        public string UserId { get; set; } = "";
        public bool EmailReminders { get; set; } = true;
        public bool InAppNotifications { get; set; } = true;
        public bool WeeklyDigest { get; set; } = true;
        public List<int> ReminderDays { get; set; } = new() { 7, 3, 1 }; // Days before deadline
        public bool AutoBookmarkHighMatches { get; set; } = false;
        public decimal AutoBookmarkThreshold { get; set; } = 90; // Minimum match score for auto-bookmark
    }
    
    // Calendar integration model
    public class BookmarkCalendarEvent
    {
        public int BookmarkId { get; set; }
        public string Title { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Description { get; set; } = "";
        public string EventType { get; set; } = "Deadline"; // Deadline, Reminder, Follow-up
        public bool IsAllDay { get; set; } = true;
        public string CalendarId { get; set; } = "";
    }
}