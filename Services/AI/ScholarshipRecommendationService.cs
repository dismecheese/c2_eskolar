using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace c2_eskolar.Services.AI
{
    public class ScholarshipRecommendationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<IdentityUser> _userManager;

        public ScholarshipRecommendationService(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<IdentityUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

        public async Task<List<ScholarshipRecommendation>> GetScholarshipRecommendationsAsync(IdentityUser user)
        {
            if (user == null) return new List<ScholarshipRecommendation>();
            
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Student")) return new List<ScholarshipRecommendation>();

            using var context = _contextFactory.CreateDbContext();
            var studentProfile = await context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (studentProfile == null) return new List<ScholarshipRecommendation>();

            // Get active scholarships
            var activeScholarships = await context.Scholarships
                .Where(s => s.IsActive && s.ApplicationDeadline > DateTime.Now)
                .ToListAsync();

            var recommendations = new List<ScholarshipRecommendation>();

            foreach (var scholarship in activeScholarships)
            {
                var matchScore = CalculateMatchScore(studentProfile, scholarship);
                var matchReasons = GenerateMatchReasons(studentProfile, scholarship);
                
                if (matchScore > 0) // Only include scholarships with some compatibility
                {
                    recommendations.Add(new ScholarshipRecommendation
                    {
                        Scholarship = scholarship,
                        MatchScore = matchScore,
                        MatchReasons = matchReasons
                    });
                }
            }

            return recommendations.OrderByDescending(r => r.MatchScore).Take(10).ToList();
        }

        private int CalculateMatchScore(StudentProfile student, Scholarship scholarship)
        {
            int score = 0;

            // Base score for active scholarship
            score += 10;

            // GPA matching
            if (scholarship.MinimumGPA.HasValue && student.GPA.HasValue)
            {
                if (student.GPA >= scholarship.MinimumGPA)
                {
                    score += 30; // High weight for GPA eligibility
                    // Bonus points for exceeding minimum GPA
                    var gpaExcess = student.GPA.Value - scholarship.MinimumGPA.Value;
                    score += (int)(gpaExcess * 10); // Up to 10 bonus points per GPA point
                }
                else
                {
                    return 0; // Ineligible due to GPA
                }
            }

            // Course/Program matching
            if (!string.IsNullOrEmpty(scholarship.RequiredCourse) && !string.IsNullOrEmpty(student.Course))
            {
                if (scholarship.RequiredCourse.Equals(student.Course, StringComparison.OrdinalIgnoreCase))
                {
                    score += 25; // Exact course match
                }
                else if (scholarship.RequiredCourse.Contains(student.Course, StringComparison.OrdinalIgnoreCase) ||
                         student.Course.Contains(scholarship.RequiredCourse, StringComparison.OrdinalIgnoreCase))
                {
                    score += 15; // Partial course match
                }
            }
            else if (string.IsNullOrEmpty(scholarship.RequiredCourse))
            {
                score += 5; // Open to all courses
            }

            // Year level matching
            if (scholarship.RequiredYearLevel.HasValue && student.YearLevel.HasValue)
            {
                if (student.YearLevel == scholarship.RequiredYearLevel)
                {
                    score += 20; // Exact year level match
                }
                else if (Math.Abs(student.YearLevel.Value - scholarship.RequiredYearLevel.Value) == 1)
                {
                    score += 10; // Close year level match
                }
            }
            else if (!scholarship.RequiredYearLevel.HasValue)
            {
                score += 5; // Open to all year levels
            }

            // University matching
            if (!string.IsNullOrEmpty(scholarship.RequiredUniversity) && !string.IsNullOrEmpty(student.UniversityName))
            {
                if (scholarship.RequiredUniversity.Equals(student.UniversityName, StringComparison.OrdinalIgnoreCase))
                {
                    score += 20; // Exact university match
                }
                else if (scholarship.RequiredUniversity.Contains(student.UniversityName, StringComparison.OrdinalIgnoreCase) ||
                         student.UniversityName.Contains(scholarship.RequiredUniversity, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10; // Partial university match
                }
            }
            else if (string.IsNullOrEmpty(scholarship.RequiredUniversity))
            {
                score += 5; // Open to all universities
            }

            // Deadline urgency (slight bonus for approaching deadlines)
            var daysUntilDeadline = (scholarship.ApplicationDeadline - DateTime.Now).TotalDays;
            if (daysUntilDeadline <= 30)
            {
                score += 5; // Approaching deadline bonus
            }

            return score;
        }

        private List<string> GenerateMatchReasons(StudentProfile student, Scholarship scholarship)
        {
            var reasons = new List<string>();

            // GPA compatibility
            if (scholarship.MinimumGPA.HasValue && student.GPA.HasValue)
            {
                if (student.GPA >= scholarship.MinimumGPA)
                {
                    if (student.GPA > scholarship.MinimumGPA + 0.5m)
                    {
                        reasons.Add($"Your GPA of {student.GPA:F2} exceeds the minimum requirement of {scholarship.MinimumGPA:F2}");
                    }
                    else
                    {
                        reasons.Add($"You meet the minimum GPA requirement of {scholarship.MinimumGPA:F2}");
                    }
                }
            }
            else if (!scholarship.MinimumGPA.HasValue)
            {
                reasons.Add("No specific GPA requirement");
            }

            // Course compatibility
            if (!string.IsNullOrEmpty(scholarship.RequiredCourse) && !string.IsNullOrEmpty(student.Course))
            {
                if (scholarship.RequiredCourse.Equals(student.Course, StringComparison.OrdinalIgnoreCase))
                {
                    reasons.Add($"Perfect match for your {student.Course} program");
                }
                else if (scholarship.RequiredCourse.Contains(student.Course, StringComparison.OrdinalIgnoreCase) ||
                         student.Course.Contains(scholarship.RequiredCourse, StringComparison.OrdinalIgnoreCase))
                {
                    reasons.Add($"Good match for students in {student.Course}");
                }
            }
            else if (string.IsNullOrEmpty(scholarship.RequiredCourse))
            {
                reasons.Add("Open to students from all academic programs");
            }

            // Year level compatibility
            if (scholarship.RequiredYearLevel.HasValue && student.YearLevel.HasValue)
            {
                if (student.YearLevel == scholarship.RequiredYearLevel)
                {
                    reasons.Add($"Designed for Year {student.YearLevel} students like you");
                }
            }
            else if (!scholarship.RequiredYearLevel.HasValue)
            {
                reasons.Add("Available to students at any year level");
            }

            // University compatibility
            if (!string.IsNullOrEmpty(scholarship.RequiredUniversity) && !string.IsNullOrEmpty(student.UniversityName))
            {
                if (scholarship.RequiredUniversity.Equals(student.UniversityName, StringComparison.OrdinalIgnoreCase))
                {
                    reasons.Add($"Specifically for {student.UniversityName} students");
                }
            }
            else if (string.IsNullOrEmpty(scholarship.RequiredUniversity))
            {
                reasons.Add("Open to students from any university");
            }

            // Deadline urgency
            var daysUntilDeadline = (scholarship.ApplicationDeadline - DateTime.Now).TotalDays;
            if (daysUntilDeadline <= 7)
            {
                reasons.Add($"Application deadline is in {Math.Ceiling(daysUntilDeadline)} days - apply soon!");
            }
            else if (daysUntilDeadline <= 30)
            {
                reasons.Add($"Application deadline is {scholarship.ApplicationDeadline:MMMM dd, yyyy}");
            }

            return reasons;
        }
    }

    public class ScholarshipRecommendation
    {
        public Scholarship Scholarship { get; set; } = null!;
        public int MatchScore { get; set; }
        public List<string> MatchReasons { get; set; } = new List<string>();
    }
}