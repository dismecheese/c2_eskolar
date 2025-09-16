using Microsoft.AspNetCore.Mvc;
using c2_eskolar.Services;
using c2_eskolar.Models.ViewModels;

namespace c2_eskolar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
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
                        "Student" => "/dashboard/student",
                        "Benefactor" => "/dashboard/benefactor",
                        "Institution" => "/dashboard/institution",
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
    }
}
