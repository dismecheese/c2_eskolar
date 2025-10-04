using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using c2_eskolar.Data;
using c2_eskolar.Models;

namespace c2_eskolar.Services.AI
{
    public class BenefactorContextService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BenefactorContextService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<string> GenerateBenefactorContextAsync(string userId, string userMessage)
        {
            await using var context = _contextFactory.CreateDbContext();
            
            // Get benefactor profile
            var benefactorProfile = await context.BenefactorProfiles
                .FirstOrDefaultAsync(bp => bp.UserId == userId);

            if (benefactorProfile == null)
            {
                return "Benefactor profile not found. Please complete your organization profile to access scholarship management features.";
            }

            var contextInfo = $"Organization: {benefactorProfile.OrganizationName}\n";
            contextInfo += $"Admin: {benefactorProfile.AdminFullName} ({benefactorProfile.AdminPosition})\n\n";

            // Get scholarships provided by this benefactor
            var scholarships = await context.Scholarships
                .Include(s => s.Applications)
                    .ThenInclude(a => a.Student)
                .Where(s => s.BenefactorProfileId == benefactorProfile.BenefactorProfileId)
                .ToListAsync();

            if (scholarships.Any())
            {
                contextInfo += GenerateScholarshipProgramsContext(scholarships);
                contextInfo += GenerateStatisticsContext(scholarships);
            }
            else
            {
                contextInfo += "You haven't created any scholarship programs yet. Consider creating scholarships to support students in need.\n";
            }

            contextInfo += GenerateBenefactorHelpContext();

            return contextInfo;
        }

        private string GenerateScholarshipProgramsContext(List<Scholarship> scholarships)
        {
            var contextInfo = "Your Scholarship Programs:\n";
            
            foreach (var scholarship in scholarships)
            {
                contextInfo += $"- **{scholarship.Title}**\n";
                contextInfo += $"  Applications: {scholarship.Applications.Count}\n";
                contextInfo += $"  Deadline: {scholarship.ApplicationDeadline:MMM dd, yyyy}\n";
                contextInfo += $"  Status: {(scholarship.IsActive ? "Active" : "Inactive")}\n";
                contextInfo += $"  Benefits: {scholarship.Benefits}\n";

                if (scholarship.SlotsAvailable.HasValue)
                    contextInfo += $"  Available Slots: {scholarship.SlotsAvailable}\n";

                // Get approved/granted scholars
                var approvedApplicants = scholarship.Applications
                    .Where(a => a.Status == "Approved" || a.Status == "Granted")
                    .ToList();

                if (approvedApplicants.Any())
                {
                    contextInfo += $"  Current Scholars ({approvedApplicants.Count}):\n";
                    foreach (var applicant in approvedApplicants.Take(5)) // Limit to prevent token overflow
                    {
                        var student = applicant.Student;
                        if (student != null)
                        {
                            contextInfo += $"    • {student.FirstName} {student.LastName}";
                            if (applicant.GWA.HasValue)
                                contextInfo += $" (GWA: {applicant.GWA:F2})";
                            contextInfo += "\n";
                        }
                    }
                    if (approvedApplicants.Count > 5)
                        contextInfo += $"    ... and {approvedApplicants.Count - 5} more scholars\n";
                }

                contextInfo += "\n";
            }

            return contextInfo;
        }

        private string GenerateStatisticsContext(List<Scholarship> scholarships)
        {
            var totalApplications = scholarships.Sum(s => s.Applications.Count);
            var totalApproved = scholarships.Sum(s => s.Applications.Count(a => a.Status == "Approved" || a.Status == "Granted"));
            var totalFunding = scholarships.Where(s => s.MonetaryValue.HasValue).Sum(s => s.MonetaryValue!.Value);

            var contextInfo = $"Overall Statistics:\n";
            contextInfo += $"- Total Applications Received: {totalApplications}\n";
            contextInfo += $"- Current Scholars: {totalApproved}\n";
            if (totalFunding > 0)
                contextInfo += $"- Total Scholarship Value: ₱{totalFunding:N0}\n";

            return contextInfo + "\n";
        }

        private string GenerateBenefactorHelpContext()
        {
            return "As a benefactor, you can ask about:\n" +
                   "- Your scholarship programs and their performance\n" +
                   "- Information about your current scholars\n" +
                   "- Application statistics and trends\n" +
                   "- Impact of your funding and suggestions for improvement\n";
        }
    }
}