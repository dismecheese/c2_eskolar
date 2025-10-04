using System;
using System.Linq;

namespace c2_eskolar.Services.AI
{
    public class QueryClassificationService
    {
        public bool IsScholarshipRelatedQuery(string userMessage)
        {
            var scholarshipKeywords = new[] 
            { 
                "scholarship", "scholarships", "recommend", "recommendation", "eligible", 
                "apply", "funding", "financial aid", "grant", "grants", "money", 
                "tuition", "study", "education", "degree", "program", "course",
                "what scholarships", "find scholarship", "search scholarship", 
                "scholarship for me", "scholarship opportunities", "available scholarships",
                "match", "suitable", "qualify", "qualified", "apply for", "application"
            };
            
            return scholarshipKeywords.Any(keyword => 
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsProgressTrackingQuery(string userMessage)
        {
            var progressKeywords = new[]
            {
                "application status", "my applications", "application progress", "track application",
                "progress", "status", "applied", "submitted", "pending", "approved", "rejected",
                "application history", "my submissions", "application timeline", "where is my application",
                "application update", "check status", "follow up", "application tracking"
            };

            return progressKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsDocumentGuidanceQuery(string userMessage)
        {
            var documentKeywords = new[]
            {
                "requirements", "documents", "upload", "submit", "prepare documents",
                "what documents", "document checklist", "required documents", "documentation",
                "forms", "application form", "essay", "personal statement", "transcript",
                "grade", "certificate", "recommendation letter", "how to apply", "application process",
                "steps to apply", "application guide", "submission guidelines"
            };

            return documentKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsDeadlineQuery(string userMessage)
        {
            var deadlineKeywords = new[]
            {
                "deadline", "deadlines", "due date", "application deadline", "when to apply",
                "last date", "closing date", "submission date", "expiry", "expires",
                "time left", "how long", "urgent", "soon to expire", "ending soon"
            };

            return deadlineKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsFinancialNeedsQuery(string userMessage)
        {
            var financialKeywords = new[]
            {
                "financial need", "financial assistance", "need based", "income", "family income",
                "financial situation", "cannot afford", "expensive", "tuition cost", "financial help",
                "low income", "financial support", "economic hardship", "financial difficulty",
                "budget", "cost", "expenses", "money problems", "financial constraint"
            };

            return financialKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsAnnouncementRelatedQuery(string userMessage)
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

        public bool IsBenefactorQuery(string userMessage)
        {
            var benefactorKeywords = new[]
            {
                "my scholars", "scholar list", "scholarship recipients", "granted scholarships",
                "scholar progress", "scholar information", "scholarship performance", "scholar details",
                "funding impact", "investment", "disbursed", "scholarship analytics", "my funding",
                "sponsored students", "beneficiary list", "scholarship grants", "fund utilization",
                "scholarship effectiveness", "my scholarship programs", "funding results",
                "scholar applications", "approved applications", "scholarship statistics",
                "total funding", "scholarship budget", "organization scholarships", "my donations"
            };

            return benefactorKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsInstitutionQuery(string userMessage)
        {
            var institutionKeywords = new[]
            {
                "student applications", "scholarship applications", "application review", "applicant list",
                "scholarship management", "internal scholarships", "institutional scholarships",
                "student information", "applicant details", "application status", "pending applications",
                "approved students", "rejected applications", "scholarship criteria", "eligibility",
                "partnership", "external scholarships", "benefactor partnerships", "collaboration",
                "institution statistics", "student data", "academic performance", "enrollment",
                "scholarship programs", "available scholarships", "funding opportunities",
                "application deadlines", "scholarship requirements", "student eligibility"
            };

            return institutionKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsNameQuery(string userMessage)
        {
            var nameKeywords = new[]
            {
                "my name", "what is my name", "tell me my name", "who am I", "what's my name",
                "my full name", "full name", "complete name", "first name", "last name", "middle name",
                "name is", "called", "introduce myself", "about my name", "my identity"
            };

            return nameKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsFullNameQuery(string userMessage)
        {
            var fullNameKeywords = new[]
            {
                "full name", "complete name", "entire name", "whole name", "my full name",
                "complete name", "all my names", "full identity"
            };

            return fullNameKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}