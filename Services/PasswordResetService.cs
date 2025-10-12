using System.Security.Cryptography;
using System.Text;
using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public interface IPasswordResetService
    {
        Task<bool> SendPasswordResetAsync(string email, string baseUrl, string? ipAddress = null, string? userAgent = null);
        Task<bool> ValidateResetTokenAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task CleanupExpiredTokensAsync();
    }

    public class PasswordResetService : IPasswordResetService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            UserManager<IdentityUser> userManager,
            IEmailService emailService,
            ILogger<PasswordResetService> logger)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetAsync(string email, string baseUrl, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                // Check if user exists
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // For security, we don't reveal if the email exists or not
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return true; // Return true to prevent email enumeration
                }

                // Generate secure token
                var token = GenerateSecureToken();
                var expiresAt = DateTime.UtcNow.AddHours(24); // 24-hour expiration

                // Create password reset record
                var passwordReset = new PasswordReset
                {
                    Email = email,
                    Token = token,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                using var context = _contextFactory.CreateDbContext();
                context.PasswordResets.Add(passwordReset);
                await context.SaveChangesAsync();

                // Build reset URL
                var resetUrl = $"{baseUrl}/reset-password?token={token}";

                // Send email
                var emailSent = await _emailService.SendPasswordResetEmailAsync(email, token, resetUrl);

                if (emailSent)
                {
                    _logger.LogInformation("Password reset email sent successfully to {Email}", email);
                    
                    // For demo/testing purposes, log the reset URL when email restrictions apply
                    _logger.LogInformation("Demo: Reset URL for {Email}: {ResetUrl}", email, resetUrl);
                    
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send password reset email to {Email}", email);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset for {Email}", email);
                return false;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var resetRecord = await context.PasswordResets
                    .FirstOrDefaultAsync(pr => pr.Token == token && !pr.IsUsed && pr.ExpiresAt > DateTime.UtcNow);

                return resetRecord != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating reset token");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var resetRecord = await context.PasswordResets
                    .FirstOrDefaultAsync(pr => pr.Token == token && !pr.IsUsed && pr.ExpiresAt > DateTime.UtcNow);

                if (resetRecord == null)
                {
                    _logger.LogWarning("Invalid or expired reset token used: {Token}", token);
                    return false;
                }

                var user = await _userManager.FindByEmailAsync(resetRecord.Email);
                if (user == null)
                {
                    _logger.LogError("User not found for reset token: {Email}", resetRecord.Email);
                    return false;
                }

                // Generate password reset token using ASP.NET Core Identity
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

                if (result.Succeeded)
                {
                    // Mark token as used
                    resetRecord.IsUsed = true;
                    resetRecord.UsedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();

                    _logger.LogInformation("Password reset successful for user {Email}", resetRecord.Email);
                    return true;
                }
                else
                {
                    _logger.LogError("Password reset failed for user {Email}: {Errors}", 
                        resetRecord.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for token {Token}", token);
                return false;
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var expiredTokens = await context.PasswordResets
                    .Where(pr => pr.ExpiresAt <= DateTime.UtcNow || pr.IsUsed)
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    context.PasswordResets.RemoveRange(expiredTokens);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Cleaned up {Count} expired password reset tokens", expiredTokens.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired password reset tokens");
            }
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32]; // 256 bits
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}