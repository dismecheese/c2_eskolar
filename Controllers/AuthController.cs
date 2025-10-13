using Microsoft.AspNetCore.Mvc;
using c2_eskolar.Services;
using c2_eskolar.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace c2_eskolar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly StudentProfileService _studentProfileService;
        private readonly BenefactorProfileService _benefactorProfileService;
        private readonly InstitutionProfileService _institutionProfileService;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(
            AuthService authService, 
            StudentProfileService studentProfileService,
            BenefactorProfileService benefactorProfileService,
            InstitutionProfileService institutionProfileService,
            UserManager<IdentityUser> userManager)
        {
            _authService = authService;
            _studentProfileService = studentProfileService;
            _benefactorProfileService = benefactorProfileService;
            _institutionProfileService = institutionProfileService;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password, [FromForm] bool remember = false)
        {
            try
            {
                Console.WriteLine($"[AuthController] Processing login for: {email}");

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return Redirect("/login?error=invalid");
                }

                var loginModel = new LoginViewModel
                {
                    Email = email,
                    Password = password,
                    RememberMe = remember
                };

                var result = await _authService.LoginAsync(loginModel);
                
                Console.WriteLine($"[AuthController] Login result - Succeeded: {result.Succeeded}");

                if (result.Succeeded)
                {
                    var userRole = await _authService.GetUserRoleAsync(email);
                    Console.WriteLine($"[AuthController] User role: {userRole}");

                    string redirectUrl = userRole switch
                    {
                        "Student" => await GetStudentRedirectUrl(email),
                        "Benefactor" => await GetBenefactorRedirectUrl(email),
                        "Institution" => await GetInstitutionRedirectUrl(email),
                        "SuperAdmin" => "/dashboard/superadmin",
                        _ => "/dashboard/student"
                    };

                    Console.WriteLine($"[AuthController] Redirecting to: {redirectUrl}");
                    return Redirect(redirectUrl);
                }
                else
                {
                    Console.WriteLine("[AuthController] Login failed");
                    
                    // Provide specific error messages
                    string errorMessage;
                    if (result.IsLockedOut)
                    {
                        errorMessage = "Account is locked out.";
                    }
                    else if (result.IsNotAllowed)
                    {
                        errorMessage = "Sign in not allowed.";
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        errorMessage = "Two-factor authentication required.";
                    }
                    else
                    {
                        // Check if user exists to give more specific feedback
                        var userExists = await _authService.UserExistsAsync(email);
                        if (!userExists)
                        {
                            errorMessage = "No account found with this email address.";
                        }
                        else
                        {
                            errorMessage = "Invalid password.";
                        }
                    }
                    
                    return Redirect($"/login?error={Uri.EscapeDataString(errorMessage)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthController] Exception: {ex.Message}");
                return Redirect("/login?error=exception");
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                return Redirect("/login?success=logged-out");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthController] Logout error: {ex.Message}");
                return Redirect("/login");
            }
        }

        private async Task<string> GetStudentRedirectUrl(string email)
        {
            try
            {
                // Get the user by email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return "/dashboard/unverified";
                }

                // Get the student profile to check verification status
                var studentProfile = await _studentProfileService.GetProfileByUserIdAsync(user.Id);
                if (studentProfile == null)
                {
                    return "/dashboard/unverified";
                }

                // Check if student is verified
                bool isVerified = studentProfile.IsVerified == true && 
                                 !string.IsNullOrEmpty(studentProfile.VerificationStatus) && 
                                 studentProfile.VerificationStatus == "Verified";

                return isVerified ? "/dashboard/student" : "/dashboard/unverified";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthController] Error checking student verification: {ex.Message}");
                // Default to unverified on error for safety
                return "/dashboard/unverified";
            }
        }

        private async Task<string> GetBenefactorRedirectUrl(string email)
        {
            try
            {
                // Get the user by email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return "/dashboard/benefactor/unverified";
                }

                // Get the benefactor profile to check verification status
                var benefactorProfile = await _benefactorProfileService.GetProfileByUserIdAsync(user.Id);
                if (benefactorProfile == null)
                {
                    return "/dashboard/benefactor/unverified";
                }

                // Check if benefactor is verified
                bool isVerified = benefactorProfile.IsVerified == true && 
                                 !string.IsNullOrEmpty(benefactorProfile.VerificationStatus) && 
                                 benefactorProfile.VerificationStatus == "Verified";

                return isVerified ? "/dashboard/benefactor" : "/dashboard/benefactor/unverified";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthController] Error checking benefactor verification: {ex.Message}");
                // Default to unverified on error for safety
                return "/dashboard/benefactor/unverified";
            }
        }

        private async Task<string> GetInstitutionRedirectUrl(string email)
        {
            try
            {
                // Get the user by email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return "/dashboard/institution/unverified";
                }

                // Get the institution profile to check verification status
                var institutionProfile = await _institutionProfileService.GetProfileByUserIdAsync(user.Id);
                if (institutionProfile == null)
                {
                    return "/dashboard/institution/unverified";
                }

                // Check if institution is verified
                bool isVerified = institutionProfile.IsVerified == true && 
                                 !string.IsNullOrEmpty(institutionProfile.VerificationStatus) && 
                                 institutionProfile.VerificationStatus == "Verified";

                return isVerified ? "/dashboard/institution" : "/dashboard/institution/unverified";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthController] Error checking institution verification: {ex.Message}");
                // Default to unverified on error for safety
                return "/dashboard/institution/unverified";
            }
        }
    }
}
