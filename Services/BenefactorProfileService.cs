using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace c2_eskolar.Services
{
    public class BenefactorProfileService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BenefactorProfileService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<BenefactorProfile?> GetProfileByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.BenefactorProfiles.FirstOrDefaultAsync(bp => bp.UserId == userId);
        }

        public async Task SaveProfileAsync(BenefactorProfile profile)
        {
            // Validate profile before saving
            var validationResult = ValidateProfile(profile);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(validationResult.ErrorMessage);
            }

            await using var context = _contextFactory.CreateDbContext();
            var existing = await context.BenefactorProfiles.FirstOrDefaultAsync(bp => bp.UserId == profile.UserId);
            if (existing == null)
            {
                if (string.IsNullOrWhiteSpace(profile.AdminFirstName) || string.IsNullOrWhiteSpace(profile.AdminLastName))
                    throw new ArgumentException("AdminFirstName and AdminLastName are required.");
                
                // Set default values for new profiles submitted for verification
                profile.CreatedAt = DateTime.Now;
                profile.UpdatedAt = DateTime.Now;
                profile.IsVerified = false;
                profile.AccountStatus = "Pending";
                
                context.BenefactorProfiles.Add(profile);
            }
            else
            {
                existing.AdminFirstName = profile.AdminFirstName;
                existing.AdminMiddleName = profile.AdminMiddleName;
                existing.AdminLastName = profile.AdminLastName;
                existing.Sex = profile.Sex;
                existing.Nationality = profile.Nationality;
                existing.BirthDate = profile.BirthDate;
                existing.OrganizationName = profile.OrganizationName;
                existing.Address = profile.Address;
                existing.ContactEmail = profile.ContactEmail;
                existing.ContactNumber = profile.ContactNumber;
                existing.Logo = profile.Logo;
                existing.UpdatedAt = DateTime.Now;
            }
            await context.SaveChangesAsync();
        }

        private (bool IsValid, string ErrorMessage) ValidateProfile(BenefactorProfile profile)
        {
            // Validate AdminFirstName
            if (string.IsNullOrWhiteSpace(profile.AdminFirstName))
                return (false, "First name is required.");
            if (!IsValidName(profile.AdminFirstName))
                return (false, "First name should only contain letters, spaces, hyphens, and apostrophes.");
            if (profile.AdminFirstName.Length > 50)
                return (false, "First name cannot exceed 50 characters.");

            // Validate AdminLastName
            if (string.IsNullOrWhiteSpace(profile.AdminLastName))
                return (false, "Last name is required.");
            if (!IsValidName(profile.AdminLastName))
                return (false, "Last name should only contain letters, spaces, hyphens, and apostrophes.");
            if (profile.AdminLastName.Length > 50)
                return (false, "Last name cannot exceed 50 characters.");

            // Validate AdminMiddleName (optional)
            if (!string.IsNullOrWhiteSpace(profile.AdminMiddleName))
            {
                if (!IsValidName(profile.AdminMiddleName))
                    return (false, "Middle name should only contain letters, spaces, hyphens, and apostrophes.");
                if (profile.AdminMiddleName.Length > 50)
                    return (false, "Middle name cannot exceed 50 characters.");
            }

            // Validate ContactNumber (personal phone)
            if (!string.IsNullOrWhiteSpace(profile.ContactNumber))
            {
                if (!IsValidPhoneNumber(profile.ContactNumber))
                    return (false, "Contact number format is invalid.");
            }

            // Validate Nationality (optional)
            if (!string.IsNullOrWhiteSpace(profile.Nationality))
            {
                if (!IsValidName(profile.Nationality))
                    return (false, "Nationality should only contain letters and spaces.");
                if (profile.Nationality.Length > 50)
                    return (false, "Nationality cannot exceed 50 characters.");
            }

            // Validate Address (optional)
            if (!string.IsNullOrWhiteSpace(profile.Address) && profile.Address.Length > 200)
                return (false, "Address cannot exceed 200 characters.");

            // Validate OrganizationName
            if (!string.IsNullOrWhiteSpace(profile.OrganizationName))
            {
                if (!IsValidOrganizationName(profile.OrganizationName))
                    return (false, "Organization name contains invalid characters.");
                if (profile.OrganizationName.Length > 150)
                    return (false, "Organization name cannot exceed 150 characters.");
            }

            // Validate ContactEmail (organization email)
            if (!string.IsNullOrWhiteSpace(profile.ContactEmail))
            {
                if (!IsValidEmail(profile.ContactEmail))
                    return (false, "Organization email format is invalid.");
                if (profile.ContactEmail.Length > 100)
                    return (false, "Organization email cannot exceed 100 characters.");
            }

            // Validate BirthDate
            if (profile.BirthDate.HasValue)
            {
                var age = DateTime.Now.Year - profile.BirthDate.Value.Year;
                if (DateTime.Now.DayOfYear < profile.BirthDate.Value.DayOfYear) age--;
                
                if (age < 18 || age > 100)
                    return (false, "Age must be between 18 and 100 years for benefactor administrators.");
            }

            return (true, string.Empty);
        }

        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            // Allow letters, spaces, hyphens, apostrophes, and periods
            return Regex.IsMatch(name.Trim(), @"^[a-zA-Z\s\-'\.]+$");
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true;
            // Allow digits, spaces, hyphens, plus, parentheses
            var cleanPhone = Regex.Replace(phone, @"[\s\-\+\(\)]", "");
            return Regex.IsMatch(cleanPhone, @"^\d{7,15}$"); // 7-15 digits
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidOrganizationName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            // Allow letters, numbers, spaces, common punctuation for organization names
            return Regex.IsMatch(name.Trim(), @"^[a-zA-Z0-9\s\-'\.&,()]+$");
        }
    }
}
