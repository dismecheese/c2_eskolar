// Models/ViewModels/RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;
using c2_eskolar.Models.Enums;

namespace c2_eskolar.Models.ViewModels
{
    // ViewModel for handling user registration form data
    public class RegisterViewModel
    {
        // User's first name (required field)
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        // User's last name (required field)
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        // User's email address (required and must be in a valid email format)
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        // User's account password (required, minimum 6 characters)
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        // Role of the user (Student, Institution, Benefactor)
        public UserRole UserRole { get; set; } = UserRole.Student;

        // Checkbox to confirm agreement with the terms and conditions
        [Required(ErrorMessage = "You must agree to the terms and conditions")]
        public bool AgreeToTerms { get; set; }
    }
}