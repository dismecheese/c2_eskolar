using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public class InstitutionProfileService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public InstitutionProfileService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<InstitutionProfile?> GetProfileByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.InstitutionProfiles.FirstOrDefaultAsync(ip => ip.UserId == userId);
        }

        public async Task SaveProfileAsync(InstitutionProfile profile)
        {
            // Server-side validation (similar to StudentProfileService)
            ValidateProfile(profile);
            
            await using var context = _contextFactory.CreateDbContext();
            var existing = await context.InstitutionProfiles.FirstOrDefaultAsync(ip => ip.UserId == profile.UserId);
            if (existing == null)
            {
                if (string.IsNullOrWhiteSpace(profile.AdminFirstName) || string.IsNullOrWhiteSpace(profile.AdminLastName))
                    throw new ArgumentException("AdminFirstName and AdminLastName are required.");
                
                // Set default values for new profiles submitted for verification
                profile.CreatedAt = DateTime.Now;
                profile.UpdatedAt = DateTime.Now;
                profile.IsVerified = false;
                profile.VerificationStatus = "Pending";
                profile.AccountStatus = "Pending";
                
                context.InstitutionProfiles.Add(profile);
            }
            else
            {
                // Admin Information
                existing.AdminFirstName = profile.AdminFirstName;
                existing.AdminMiddleName = profile.AdminMiddleName;
                existing.AdminLastName = profile.AdminLastName;
                existing.AdminPosition = profile.AdminPosition;
                
                // Institution Information
                existing.InstitutionName = profile.InstitutionName;
                existing.InstitutionType = profile.InstitutionType;
                existing.Address = profile.Address;
                existing.ContactNumber = profile.ContactNumber;
                existing.ContactEmail = profile.ContactEmail;
                existing.Website = profile.Website;
                existing.Mission = profile.Mission;
                existing.Description = profile.Description;
                existing.Logo = profile.Logo;
                existing.ProfilePicture = profile.ProfilePicture;
                
                // Academic Information
                existing.TotalStudents = profile.TotalStudents;
                existing.EstablishedDate = profile.EstablishedDate;
                existing.Accreditation = profile.Accreditation;
                
                // Verification & Status
                existing.IsVerified = profile.IsVerified;
                existing.VerificationStatus = profile.VerificationStatus;
                existing.VerificationDate = profile.VerificationDate;
                
                // Update timestamp
                existing.UpdatedAt = DateTime.Now;
            }
            await context.SaveChangesAsync();
        }
        
        private void ValidateProfile(InstitutionProfile profile)
        {
            var errors = new List<string>();
            
            // Required field validation
            if (string.IsNullOrWhiteSpace(profile.AdminFirstName))
                errors.Add("First Name is required.");
            if (string.IsNullOrWhiteSpace(profile.AdminLastName))
                errors.Add("Last Name is required.");
            if (string.IsNullOrWhiteSpace(profile.InstitutionName))
                errors.Add("Institution Name is required.");
                
            // Name validation (letters, spaces, hyphens, apostrophes only)
            if (!string.IsNullOrWhiteSpace(profile.AdminFirstName) && !IsValidName(profile.AdminFirstName))
                errors.Add("First Name contains invalid characters.");
            if (!string.IsNullOrWhiteSpace(profile.AdminMiddleName) && !IsValidName(profile.AdminMiddleName))
                errors.Add("Middle Name contains invalid characters.");
            if (!string.IsNullOrWhiteSpace(profile.AdminLastName) && !IsValidName(profile.AdminLastName))
                errors.Add("Last Name contains invalid characters.");
            if (!string.IsNullOrWhiteSpace(profile.Accreditation) && !IsValidName(profile.Accreditation))
                errors.Add("Nationality contains invalid characters.");
                
            // Institution name validation
            if (!string.IsNullOrWhiteSpace(profile.InstitutionName) && !IsValidInstitutionName(profile.InstitutionName))
                errors.Add("Institution name contains invalid characters.");
                
            // Phone number validation (stored in AdminPosition temporarily)
            if (!string.IsNullOrWhiteSpace(profile.AdminPosition) && !IsValidPhoneNumber(profile.AdminPosition))
                errors.Add("Personal contact number format is invalid.");
            if (!string.IsNullOrWhiteSpace(profile.ContactNumber) && !IsValidPhoneNumber(profile.ContactNumber))
                errors.Add("Institution contact number format is invalid.");
                
            // Email validation
            if (!string.IsNullOrWhiteSpace(profile.ContactEmail) && !IsValidEmail(profile.ContactEmail))
                errors.Add("Institution email format is invalid.");
                
            // Age validation for birth date (stored in EstablishedDate temporarily)
            if (profile.EstablishedDate.HasValue)
            {
                var age = DateTime.Now.Year - profile.EstablishedDate.Value.Year;
                if (DateTime.Now.DayOfYear < profile.EstablishedDate.Value.DayOfYear) age--;
                
                if (age < 18 || age > 100)
                    errors.Add("Birth date is invalid. Age must be between 18 and 100 years for institution administrators.");
            }
            
            // Length validation
            if (!string.IsNullOrWhiteSpace(profile.AdminFirstName) && profile.AdminFirstName.Length > 50)
                errors.Add("First Name cannot exceed 50 characters.");
            if (!string.IsNullOrWhiteSpace(profile.AdminMiddleName) && profile.AdminMiddleName.Length > 50)
                errors.Add("Middle Name cannot exceed 50 characters.");
            if (!string.IsNullOrWhiteSpace(profile.AdminLastName) && profile.AdminLastName.Length > 50)
                errors.Add("Last Name cannot exceed 50 characters.");
            if (!string.IsNullOrWhiteSpace(profile.InstitutionName) && profile.InstitutionName.Length > 150)
                errors.Add("Institution name cannot exceed 150 characters.");
            if (!string.IsNullOrWhiteSpace(profile.Address) && profile.Address.Length > 255)
                errors.Add("Institution address cannot exceed 255 characters.");
            if (!string.IsNullOrWhiteSpace(profile.Mission) && profile.Mission.Length > 200)
                errors.Add("Personal address cannot exceed 200 characters.");
            if (!string.IsNullOrWhiteSpace(profile.ContactNumber) && profile.ContactNumber.Length > 15)
                errors.Add("Institution contact number cannot exceed 15 characters.");
            if (!string.IsNullOrWhiteSpace(profile.ContactEmail) && profile.ContactEmail.Length > 100)
                errors.Add("Institution email cannot exceed 100 characters.");
                
            if (errors.Any())
                throw new ArgumentException(string.Join(" ", errors));
        }
        
        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(name.Trim(), @"^[a-zA-Z\s\-'\.]+$");
        }
        
        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true;
            var cleanPhone = System.Text.RegularExpressions.Regex.Replace(phone, @"[\s\-\+\(\)]", "");
            return System.Text.RegularExpressions.Regex.IsMatch(cleanPhone, @"^\d{7,15}$");
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
        
        private bool IsValidInstitutionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            return System.Text.RegularExpressions.Regex.IsMatch(name.Trim(), @"^[a-zA-Z0-9\s\-'\.&,()]+$");
        }
    }
}
