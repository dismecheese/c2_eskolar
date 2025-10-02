using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace c2_eskolar.Services
{
    public class ProfileSummaryService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileSummaryService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ProfileSummaryResult> GetProfileSummaryAsync(IdentityUser user)
        {
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault();
            if (string.IsNullOrEmpty(role)) return null;

            if (role == "Student")
            {
                var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (profile == null) return null;
                return new ProfileSummaryResult
                {
                    Role = "Student",
                    Summary = $"Name: {profile.FullName}\nEmail: {profile.Email}\nUniversity: {profile.UniversityName}\nCourse: {profile.Course}\nYear Level: {profile.YearLevel}\nGPA: {profile.GPA}\nVerified: {profile.IsVerified}"
                };
            }
            else if (role == "Benefactor")
            {
                var profile = await _context.BenefactorProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (profile == null) return null;
                return new ProfileSummaryResult
                {
                    Role = "Benefactor",
                    Summary = $"Admin: {profile.AdminFullName}\nOrganization: {profile.OrganizationName}\nType: {profile.OrganizationType}\nEmail: {profile.ContactEmail}\nVerified: {profile.IsVerified}"
                };
            }
            else if (role == "Institution")
            {
                var profile = await _context.InstitutionProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (profile == null) return null;
                return new ProfileSummaryResult
                {
                    Role = "Institution",
                    Summary = $"Admin: {profile.AdminFullName}\nInstitution: {profile.InstitutionName}\nType: {profile.InstitutionType}\nEmail: {profile.ContactEmail}\nVerified: {profile.IsVerified}"
                };
            }
            return null;
        }
    }

    public class ProfileSummaryResult
    {
        public string Role { get; set; }
        public string Summary { get; set; }
    }
}