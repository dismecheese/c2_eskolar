using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using c2_eskolar.Data;
using c2_eskolar.Models;

namespace c2_eskolar.Services.AI
{
    public class InstitutionContextService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public InstitutionContextService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<string> GenerateInstitutionContextAsync(string userId, string userMessage)
        {
            await using var context = _contextFactory.CreateDbContext();
            
            // Get institution profile
            var institutionProfile = await context.InstitutionProfiles
                .FirstOrDefaultAsync(ip => ip.UserId == userId);

            if (institutionProfile == null)
            {
                return "Institution profile not found. Please complete your institutional profile to access scholarship management features.";
            }

            var contextInfo = $"Institution: {institutionProfile.InstitutionName}\n";
            contextInfo += $"Admin: {institutionProfile.AdminFullName} ({institutionProfile.AdminPosition})\n";
            if (!string.IsNullOrEmpty(institutionProfile.InstitutionType))
                contextInfo += $"Type: {institutionProfile.InstitutionType}\n";
            contextInfo += "\n";

            // Get internal scholarships managed by this institution
            var internalScholarships = await context.Scholarships
                .Include(s => s.Applications)
                    .ThenInclude(a => a.Student)
                .Where(s => s.InstitutionProfileId == institutionProfile.InstitutionProfileId)
                .ToListAsync();

            // Get all scholarship applications from students of this institution
            var studentApplications = await context.ScholarshipApplications
                .Include(sa => sa.Scholarship)
                .Include(sa => sa.Student)
                .Where(sa => sa.Student != null && 
                           sa.Student.UniversityName == institutionProfile.InstitutionName)
                .ToListAsync();

            if (internalScholarships.Any())
            {
                contextInfo += GenerateInternalScholarshipContext(internalScholarships);
            }

            if (studentApplications.Any())
            {
                contextInfo += GenerateStudentApplicationsContext(studentApplications);
            }

            // Get available external scholarships (from benefactors)
            var externalScholarships = await context.Scholarships
                .Where(s => s.BenefactorProfileId != null && s.IsActive)
                .Take(5)
                .ToListAsync();

            if (externalScholarships.Any())
            {
                contextInfo += GenerateExternalScholarshipsContext(externalScholarships);
            }

            contextInfo += GenerateInstitutionHelpContext();

            return contextInfo;
        }

        private string GenerateInternalScholarshipContext(List<Scholarship> internalScholarships)
        {
            var contextInfo = "Your Internal Scholarship Programs:\n";
            
            foreach (var scholarship in internalScholarships)
            {
                contextInfo += $"- **{scholarship.Title}**\n";
                contextInfo += $"  Applications: {scholarship.Applications.Count}\n";
                contextInfo += $"  Deadline: {scholarship.ApplicationDeadline:MMM dd, yyyy}\n";
                contextInfo += $"  Status: {(scholarship.IsActive ? "Active" : "Inactive")}\n";

                var pendingCount = scholarship.Applications.Count(a => a.Status == "Submitted" || a.Status == "Under Review");
                var approvedCount = scholarship.Applications.Count(a => a.Status == "Approved");
                var rejectedCount = scholarship.Applications.Count(a => a.Status == "Rejected");

                contextInfo += $"  Pending: {pendingCount}, Approved: {approvedCount}, Rejected: {rejectedCount}\n\n";
            }

            return contextInfo;
        }

        private string GenerateStudentApplicationsContext(List<ScholarshipApplication> studentApplications)
        {
            var contextInfo = "Student Applications Overview:\n";
            var groupedByScholarship = studentApplications
                .GroupBy(sa => sa.Scholarship?.Title ?? "Unknown Scholarship")
                .ToList();

            foreach (var group in groupedByScholarship.Take(10)) // Limit to prevent token overflow
            {
                contextInfo += $"- **{group.Key}**: {group.Count()} applications\n";
                
                var pending = group.Count(a => a.Status == "Submitted" || a.Status == "Under Review");
                var approved = group.Count(a => a.Status == "Approved" || a.Status == "Granted");
                
                contextInfo += $"  Pending: {pending}, Approved: {approved}\n";
            }

            var totalStudentApps = studentApplications.Count;
            var totalApproved = studentApplications.Count(a => a.Status == "Approved" || a.Status == "Granted");
            var totalPending = studentApplications.Count(a => a.Status == "Submitted" || a.Status == "Under Review");

            contextInfo += $"\nYour Students' Application Summary:\n";
            contextInfo += $"- Total Applications: {totalStudentApps}\n";
            contextInfo += $"- Approved/Granted: {totalApproved}\n";
            contextInfo += $"- Pending Review: {totalPending}\n\n";

            return contextInfo;
        }

        private string GenerateExternalScholarshipsContext(List<Scholarship> externalScholarships)
        {
            var contextInfo = $"Available External Scholarships ({externalScholarships.Count} shown):\n";
            
            foreach (var scholarship in externalScholarships)
            {
                contextInfo += $"- {scholarship.Title} (Deadline: {scholarship.ApplicationDeadline:MMM dd})\n";
            }

            return contextInfo + "\n";
        }

        private string GenerateInstitutionHelpContext()
        {
            return "As an institution, you can ask about:\n" +
                   "- Your students' scholarship applications and status\n" +
                   "- Available scholarships for your students\n" +
                   "- Internal scholarship program management\n" +
                   "- Partnership opportunities with benefactors\n" +
                   "- Application trends and student performance data\n";
        }
    }
}