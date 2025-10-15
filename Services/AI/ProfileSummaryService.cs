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
    public class ProfileSummaryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileSummaryService(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<IdentityUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

        public async Task<ProfileSummaryResult?> GetProfileSummaryAsync(IdentityUser user)
        {
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            string? role = roles.FirstOrDefault();
            if (string.IsNullOrEmpty(role)) return null;

            if (role == "Student")
            {
                using var context = _contextFactory.CreateDbContext();
                var profile = await context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (profile == null) return null;
                return new ProfileSummaryResult("Student", 
                    $"First Name: {profile.FirstName}\n" +
                    $"Middle Name: {profile.MiddleName ?? "Not provided"}\n" +
                    $"Last Name: {profile.LastName}\n" +
                    $"Full Name: {profile.FullName}\n" +
                    $"Email: {profile.Email ?? "Not provided"}\n" +
                    $"Mobile Number: {profile.MobileNumber ?? "Not provided"}\n" +
                    $"Sex: {profile.Sex ?? "Not provided"}\n" +
                    $"Nationality: {profile.Nationality ?? "Not provided"}\n" +
                    $"Birth Date: {profile.BirthDate?.ToString("MMMM dd, yyyy") ?? "Not provided"}\n" +
                    $"Permanent Address: {profile.PermanentAddress ?? "Not provided"}\n" +
                    $"University: {profile.UniversityName ?? "Not provided"}\n" +
                    $"Course: {profile.Course ?? "Not provided"}\n" +
                    $"Year Level: {profile.YearLevel?.ToString() ?? "Not provided"}\n" +
                    $"Student Number: {profile.StudentNumber ?? "Not provided"}\n" +
                    $"GPA: {profile.GPA?.ToString("F2") ?? "Not provided"}\n" +
                    $"Account Status: {profile.AccountStatus ?? "Not provided"}\n" +
                    $"Verified: {profile.IsVerified}");
            }
            else if (role == "Benefactor")
            {
                using var context = _contextFactory.CreateDbContext();
                var profile = await context.BenefactorProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (profile == null) return null;
                return new ProfileSummaryResult("Benefactor", $"Admin: {profile.AdminFullName}\nOrganization: {profile.OrganizationName}\nType: {profile.OrganizationType}\nEmail: {profile.ContactEmail}\nVerified: {profile.IsVerified}");
            }
            else if (role == "Institution")
            {
                using var context = _contextFactory.CreateDbContext();
                var profile = await context.InstitutionProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (profile == null) return null;
                return new ProfileSummaryResult("Institution", $"Admin: {profile.AdminFullName}\nInstitution: {profile.InstitutionName}\nType: {profile.InstitutionType}\nEmail: {profile.ContactEmail}\nVerified: {profile.IsVerified}");
            }
            return null;
        }

        public async Task<string?> GetUserFirstNameAsync(IdentityUser user)
        {
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            string? role = roles.FirstOrDefault();
            if (string.IsNullOrEmpty(role)) return null;

            if (role == "Student")
            {
                using var context = _contextFactory.CreateDbContext();
                var profile = await context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                return profile?.FirstName;
            }
            else if (role == "Benefactor")
            {
                using var context = _contextFactory.CreateDbContext();
                var profile = await context.BenefactorProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                // Extract first name from AdminFullName if available
                var fullName = profile?.AdminFullName;
                return fullName?.Split(' ').FirstOrDefault();
            }
            else if (role == "Institution")
            {
                using var context = _contextFactory.CreateDbContext();
                var profile = await context.InstitutionProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                // Extract first name from AdminFullName if available
                var fullName = profile?.AdminFullName;
                return fullName?.Split(' ').FirstOrDefault();
            }
            return null;
        }


    }

    public class ProfileSummaryResult
    {
        public string Role { get; set; }
        public string Summary { get; set; }

        public ProfileSummaryResult(string role, string summary)
        {
            Role = role;
            Summary = summary;
        }
    }
}