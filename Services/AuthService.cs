using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Models.Enums;
using c2_eskolar.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace c2_eskolar.Services
{
    // Handles user authentication, registration, roles, and profiles
    public class AuthService
    {
    // ASP.NET Identity managers for user operations and login handling
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

        // Database context for saving related user profile info
        private readonly ApplicationDbContext _context;

        // Constructor injects dependencies (DI for Identity + DbContext)
    public AuthService(UserManager<IdentityUser> userManager,
               SignInManager<IdentityUser> signInManager,
                           ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // Register a new user and create profile based on selected role
        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            // Create new IdentityUser object from registration info
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            // Save user with hashed password
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign role (Student, Institution, Benefactor)
                await _userManager.AddToRoleAsync(user, model.UserRole.ToString());
                // Create matching profile entry in database
                await CreateUserProfileAsync(user.Id, model);
            }

            return result;
        }

        // Authenticate user with email + password and start session
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );
        }

        // Validates credentials without signing in - useful for API auth or password verification
        public async Task<(bool IsValid, string Role)> ValidateCredentialsAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return (false, string.Empty);
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return (false, string.Empty);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Student";

            return (true, role);
        }

        // Check if an account exists with the given email
        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        // Get the role of a user by email, fallback to "Student"
        public async Task<string> GetUserRoleAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return "Student"; // fallback

            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? "Student";
        }

        // Sign out currently logged-in user
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // Create corresponding profile entry (Student, Benefactor, or Institution)
        private async Task CreateUserProfileAsync(string userId, RegisterViewModel model)
        {
            switch (model.UserRole)
            {
                case UserRole.Student:
                    _context.StudentProfiles.Add(new StudentProfile
                    {
                        UserId = userId,
                        FirstName = model.FirstName,
                        MiddleName = model.MiddleName,
                        LastName = model.LastName,
                        Sex = model.Sex,
                        Nationality = model.Nationality,
                        PermanentAddress = model.PermanentAddress,
                        BirthDate = model.DateOfBirth,
                        MobileNumber = model.MobileNumber,
                        Email = model.Email
                    });
                    break;

                case UserRole.Benefactor:
                    _context.BenefactorProfiles.Add(new BenefactorProfile
                    {
                        UserId = userId,
                        AdminFirstName = model.FirstName,
                        AdminLastName = model.LastName,
                        OrganizationName = "To be updated"
                    });
                    break;

                case UserRole.Institution:
                    _context.InstitutionProfiles.Add(new InstitutionProfile
                    {
                        UserId = userId,
                        AdminFirstName = model.FirstName,
                        AdminLastName = model.LastName,
                        InstitutionName = "To be updated"
                    });
                    break;
            }

            // Commit profile changes to database
            await _context.SaveChangesAsync();
        }
    }
}