using System.ComponentModel.DataAnnotations;

namespace c2_eskolar.Models.ViewModels
{
    // ViewModel used for handling login form data
    public class LoginViewModel
    {
        // User's email address (required and must be a valid email format)
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        // User's password (required field)
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        // Flag to remember user login across sessions
        public bool RememberMe { get; set; }
    }
}