using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Models.Enums;
using c2_eskolar.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace c2_eskolar.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AuthService(UserManager<ApplicationUser> userManager, 
                           SignInManager<ApplicationUser> signInManager,
                           ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var username = $"{model.FirstName}.{model.LastName}".ToLower().Replace(" ", "");

            var user = new ApplicationUser
            {
                UserName = username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.UserRole.ToString());
                await CreateUserProfileAsync(user.Id, model);
            }

            return result;
        }

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

        // Check if user exists by email
        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<string> GetUserRoleAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return "Student"; // fallback

            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? "Student";
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        private async Task CreateUserProfileAsync(string userId, RegisterViewModel model)
        {
            switch (model.UserRole)
            {
                case UserRole.Student:
                    _context.StudentProfiles.Add(new StudentProfile
                    {
                        UserId = userId,
                        FirstName = model.FirstName,
                        LastName = model.LastName
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

            await _context.SaveChangesAsync();
        }
    }
}
