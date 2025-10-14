using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public class StudentProfileService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public StudentProfileService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public async Task<List<StudentProfile>> GetAllProfilesAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.StudentProfiles.ToListAsync();
        }

    public async Task<StudentProfile?> GetProfileByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);
        }

        public async Task SaveProfileAsync(StudentProfile profile, string? identityEmail = null)
        {
            // Server-side validation
            ValidateProfile(profile);
            
            await using var context = _contextFactory.CreateDbContext();
            var existing = await context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == profile.UserId);
            if (existing == null)
            {
                // Ensure required fields are set
                if (string.IsNullOrWhiteSpace(profile.FirstName) || string.IsNullOrWhiteSpace(profile.LastName))
                    throw new ArgumentException("FirstName and LastName are required.");
                
                // Always set email from Identity if provided
                if (!string.IsNullOrWhiteSpace(identityEmail))
                    profile.Email = identityEmail;
                
                // Set status to Pending for new profiles submitted for verification
                profile.AccountStatus = "Pending";
                profile.IsVerified = false;
                profile.CreatedAt = DateTime.Now;
                profile.UpdatedAt = DateTime.Now;
                
                context.StudentProfiles.Add(profile);
            }
            else
            {
                // Update ALL fields for existing profiles
                existing.FirstName = profile.FirstName;
                existing.MiddleName = profile.MiddleName;
                existing.LastName = profile.LastName;
                existing.Sex = profile.Sex;
                existing.Nationality = profile.Nationality;
                existing.PermanentAddress = profile.PermanentAddress;
                existing.BirthDate = profile.BirthDate;
                existing.MobileNumber = profile.MobileNumber;
                
                // Always set email from Identity if provided
                if (!string.IsNullOrWhiteSpace(identityEmail))
                    existing.Email = identityEmail;
                else if (!string.IsNullOrWhiteSpace(profile.Email))
                    existing.Email = profile.Email;

                existing.UniversityName = profile.UniversityName;
                existing.StudentNumber = profile.StudentNumber;
                existing.Course = profile.Course;
                existing.YearLevel = profile.YearLevel;
                
                // Only update account status if it's being changed from outside (admin/institution)
                // For student profile updates, keep status as Pending if it was already verified
                if (existing.AccountStatus != "Verified")
                {
                    existing.AccountStatus = "Pending"; // Reset to pending when profile is updated
                    existing.IsVerified = false;
                }
                
                existing.ProfilePicture = profile.ProfilePicture;
                existing.GPA = profile.GPA;
                existing.UpdatedAt = DateTime.Now;
                
                // Update additional fields if they exist
                if (profile.StudentIdDocumentPath != null)
                    existing.StudentIdDocumentPath = profile.StudentIdDocumentPath;
                if (profile.CorDocumentPath != null)
                    existing.CorDocumentPath = profile.CorDocumentPath;
                if (profile.IsPartnerInstitution.HasValue)
                    existing.IsPartnerInstitution = profile.IsPartnerInstitution;
                if (!string.IsNullOrWhiteSpace(profile.PartnerInstitutionName))
                    existing.PartnerInstitutionName = profile.PartnerInstitutionName;
            }
            await context.SaveChangesAsync();
        }
        
        private void ValidateProfile(StudentProfile profile)
        {
            var errors = new List<string>();
            
            // Required field validation
            if (string.IsNullOrWhiteSpace(profile.FirstName))
                errors.Add("First Name is required.");
            if (string.IsNullOrWhiteSpace(profile.LastName))
                errors.Add("Last Name is required.");
                
            // Name validation (letters, spaces, hyphens, apostrophes only)
            if (!string.IsNullOrWhiteSpace(profile.FirstName) && !IsValidName(profile.FirstName))
                errors.Add("First Name contains invalid characters.");
            if (!string.IsNullOrWhiteSpace(profile.MiddleName) && !IsValidName(profile.MiddleName))
                errors.Add("Middle Name contains invalid characters.");
            if (!string.IsNullOrWhiteSpace(profile.LastName) && !IsValidName(profile.LastName))
                errors.Add("Last Name contains invalid characters.");
            if (!string.IsNullOrWhiteSpace(profile.Nationality) && !IsValidName(profile.Nationality))
                errors.Add("Nationality contains invalid characters.");
                
            // Phone number validation
            if (!string.IsNullOrWhiteSpace(profile.MobileNumber) && !IsValidPhoneNumber(profile.MobileNumber))
                errors.Add("Phone number format is invalid.");
                
            // Student number validation
            if (!string.IsNullOrWhiteSpace(profile.StudentNumber) && !IsValidStudentNumber(profile.StudentNumber))
                errors.Add("Student number contains invalid characters.");
                
            // University and program validation
            if (!string.IsNullOrWhiteSpace(profile.UniversityName) && !IsValidInstitutionName(profile.UniversityName))
                errors.Add("University name contains invalid characters.");
            if (!string.IsNullOrWhiteSpace(profile.Course) && !IsValidInstitutionName(profile.Course))
                errors.Add("Program name contains invalid characters.");
                
            // Age validation
            if (profile.BirthDate.HasValue)
            {
                var age = DateTime.Now.Year - profile.BirthDate.Value.Year;
                if (DateTime.Now.DayOfYear < profile.BirthDate.Value.DayOfYear) age--;
                
                if (age < 13 || age > 100)
                    errors.Add("Birth date is invalid. Age must be between 13 and 100 years.");
            }
            
            // Length validation
            if (!string.IsNullOrWhiteSpace(profile.FirstName) && profile.FirstName.Length > 50)
                errors.Add("First Name cannot exceed 50 characters.");
            if (!string.IsNullOrWhiteSpace(profile.MiddleName) && profile.MiddleName.Length > 50)
                errors.Add("Middle Name cannot exceed 50 characters.");
            if (!string.IsNullOrWhiteSpace(profile.LastName) && profile.LastName.Length > 50)
                errors.Add("Last Name cannot exceed 50 characters.");
            if (!string.IsNullOrWhiteSpace(profile.MobileNumber) && profile.MobileNumber.Length > 20)
                errors.Add("Phone number cannot exceed 20 characters.");
            if (!string.IsNullOrWhiteSpace(profile.StudentNumber) && profile.StudentNumber.Length > 20)
                errors.Add("Student number cannot exceed 20 characters.");
            if (!string.IsNullOrWhiteSpace(profile.UniversityName) && profile.UniversityName.Length > 100)
                errors.Add("University name cannot exceed 100 characters.");
            if (!string.IsNullOrWhiteSpace(profile.Course) && profile.Course.Length > 100)
                errors.Add("Program name cannot exceed 100 characters.");
            if (!string.IsNullOrWhiteSpace(profile.PermanentAddress) && profile.PermanentAddress.Length > 200)
                errors.Add("Address cannot exceed 200 characters.");
                
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
        
        private bool IsValidStudentNumber(string studentNum)
        {
            if (string.IsNullOrWhiteSpace(studentNum)) return true;
            return System.Text.RegularExpressions.Regex.IsMatch(studentNum.Trim(), @"^[a-zA-Z0-9\-]+$");
        }
        
        private bool IsValidInstitutionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            return System.Text.RegularExpressions.Regex.IsMatch(name.Trim(), @"^[a-zA-Z0-9\s\-'\.&,()]+$");
        }
    }
}
