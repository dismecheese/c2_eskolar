using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace c2_eskolar.Models.Validation
{
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        public PasswordComplexityAttribute()
        {
            ErrorMessage = "Password must be at least 6 characters, contain at least one digit, and at least one uppercase letter.";
        }

        public override bool IsValid(object? value)
        {
            var password = value as string ?? string.Empty;
            if (password.Length < 6)
                return false;
            if (!Regex.IsMatch(password, @"\d")) // at least one digit
                return false;
            if (!Regex.IsMatch(password, @"[A-Z]")) // at least one uppercase
                return false;
            return true;
        }
    }
}