using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using c2_eskolar.Data;
using c2_eskolar.Models;

namespace c2_eskolar.Services.AI
{
    public class StudentContextService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public StudentContextService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<string> GenerateStudentContextAsync(string userId, string userMessage, bool isProgressQuery, bool isDocumentQuery, bool isDeadlineQuery, bool isFinancialQuery)
        {
            await using var context = _contextFactory.CreateDbContext();
            
            // Get student profile
            var studentProfile = await context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (studentProfile == null)
            {
                return "Student profile not found. Please complete your student profile to access personalized scholarship features.";
            }

            var contextInfo = $"Student: {studentProfile.FirstName} {studentProfile.LastName}\n";
            if (!string.IsNullOrEmpty(studentProfile.UniversityName))
                contextInfo += $"University: {studentProfile.UniversityName}\n";
            if (!string.IsNullOrEmpty(studentProfile.Course))
                contextInfo += $"Course: {studentProfile.Course}\n";
            if (studentProfile.YearLevel.HasValue)
                contextInfo += $"Year Level: {studentProfile.YearLevel}\n";
            contextInfo += "\n";

            // Get student's scholarship applications if asking about progress
            if (isProgressQuery)
            {
                contextInfo += await GenerateApplicationProgressContext(context, studentProfile);
            }

            // Get upcoming deadlines if asking about deadlines
            if (isDeadlineQuery)
            {
                contextInfo += await GenerateDeadlineContext(context);
            }

            // Provide document guidance if asking about requirements
            if (isDocumentQuery)
            {
                contextInfo += GenerateDocumentGuidanceContext();
            }

            // Provide financial guidance if asking about financial needs
            if (isFinancialQuery)
            {
                contextInfo += GenerateFinancialGuidanceContext(studentProfile);
            }

            contextInfo += GenerateHelpMenuContext();

            return contextInfo;
        }

        private async Task<string> GenerateApplicationProgressContext(ApplicationDbContext context, StudentProfile studentProfile)
        {
            var applications = await context.ScholarshipApplications
                .Include(sa => sa.Scholarship)
                .Where(sa => sa.StudentProfileId == studentProfile.StudentProfileId)
                .OrderByDescending(sa => sa.ApplicationDate)
                .ToListAsync();

            var contextInfo = "";

            if (applications.Any())
            {
                contextInfo += "Your Scholarship Applications:\n";
                foreach (var app in applications.Take(10)) // Limit to prevent token overflow
                {
                    contextInfo += $"- **{app.Scholarship?.Title ?? "Unknown Scholarship"}**\n";
                    contextInfo += $"  Status: {app.Status}\n";
                    contextInfo += $"  Applied: {app.ApplicationDate:MMM dd, yyyy}\n";
                    
                    if (app.Scholarship != null)
                    {
                        contextInfo += $"  Deadline: {app.Scholarship.ApplicationDeadline:MMM dd, yyyy}\n";
                        
                        // Check if deadline is approaching
                        var daysUntilDeadline = (app.Scholarship.ApplicationDeadline - DateTime.Now).Days;
                        if (daysUntilDeadline > 0 && daysUntilDeadline <= 7)
                            contextInfo += $"  ⚠️ Deadline in {daysUntilDeadline} days!\n";
                        else if (daysUntilDeadline < 0)
                            contextInfo += $"  ❌ Deadline passed\n";
                    }

                    if (!string.IsNullOrEmpty(app.ReviewNotes))
                        contextInfo += $"  Notes: {app.ReviewNotes}\n";
                    
                    contextInfo += "\n";
                }
            }
            else
            {
                contextInfo += "You haven't submitted any scholarship applications yet. Let me help you find suitable scholarships!\n\n";
            }

            return contextInfo;
        }

        private async Task<string> GenerateDeadlineContext(ApplicationDbContext context)
        {
            var upcomingDeadlines = await context.Scholarships
                .Where(s => s.IsActive && s.ApplicationDeadline > DateTime.Now)
                .OrderBy(s => s.ApplicationDeadline)
                .Take(10)
                .ToListAsync();

            var contextInfo = "";

            if (upcomingDeadlines.Any())
            {
                contextInfo += "Upcoming Scholarship Deadlines:\n";
                foreach (var scholarship in upcomingDeadlines)
                {
                    var daysLeft = (scholarship.ApplicationDeadline - DateTime.Now).Days;
                    var urgency = daysLeft <= 7 ? "🔴 URGENT" : daysLeft <= 14 ? "🟡 SOON" : "🟢";
                    
                    contextInfo += $"- **{scholarship.Title}** {urgency}\n";
                    contextInfo += $"  Deadline: {scholarship.ApplicationDeadline:MMM dd, yyyy} ({daysLeft} days left)\n";
                    
                    if (!string.IsNullOrEmpty(scholarship.Benefits))
                        contextInfo += $"  Benefits: {scholarship.Benefits}\n";
                    
                    contextInfo += "\n";
                }
            }

            return contextInfo;
        }

        private string GenerateDocumentGuidanceContext()
        {
            return "Common Scholarship Document Requirements:\n" +
                   "✓ **Academic Transcript** - Most recent grades/GWA\n" +
                   "✓ **Certificate of Enrollment** - Proof of current enrollment\n" +
                   "✓ **Personal Statement/Essay** - Why you deserve the scholarship\n" +
                   "✓ **Recommendation Letters** - From teachers/professors\n" +
                   "✓ **Income Certificate** - For need-based scholarships\n" +
                   "✓ **Valid ID** - Government-issued identification\n" +
                   "✓ **Birth Certificate** - Proof of age and citizenship\n\n" +
                   
                   "📝 **Application Tips:**\n" +
                   "• Start preparing documents early\n" +
                   "• Keep digital copies organized\n" +
                   "• Check specific requirements for each scholarship\n" +
                   "• Proofread your personal statement multiple times\n" +
                   "• Follow submission guidelines exactly\n\n";
        }

        private string GenerateFinancialGuidanceContext(StudentProfile studentProfile)
        {
            var contextInfo = "💰 **Financial Aid Guidance:**\n" +
                             "• **Need-Based Scholarships** - Based on family income and financial situation\n" +
                             "• **Merit-Based Scholarships** - Based on academic performance and achievements\n" +
                             "• **Course-Specific Scholarships** - For particular fields of study\n" +
                             "• **Government Scholarships** - CHED, DOST, and other agencies\n" +
                             "• **Private Foundation Scholarships** - From benefactors and organizations\n\n";
            
            if (studentProfile.GPA.HasValue)
            {
                var gpaAdvice = studentProfile.GPA >= 3.5m 
                    ? "Your GPA qualifies you for merit-based scholarships!"
                    : studentProfile.GPA >= 3.0m
                    ? "Consider both merit and need-based scholarships."
                    : "Focus on need-based and community service scholarships.";
                
                contextInfo += $"Based on your GPA ({studentProfile.GPA:F2}): {gpaAdvice}\n\n";
            }

            return contextInfo;
        }

        private string GenerateHelpMenuContext()
        {
            return "💡 **How I can help you:**\n" +
                   "• Find scholarships that match your profile\n" +
                   "• Track your application progress and deadlines\n" +
                   "• Guide you through application requirements\n" +
                   "• Provide tips for improving your application\n" +
                   "• Alert you about urgent deadlines\n" +
                   "• Help you understand financial aid options\n";
        }
    }
}