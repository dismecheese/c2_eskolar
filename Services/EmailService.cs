using Microsoft.JSInterop;

namespace c2_eskolar.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
        Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string? plainTextContent = null);
    }

    public class EmailService : IEmailService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(IJSRuntime jsRuntime, ILogger<EmailService> logger, IConfiguration configuration)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
        {
            try
            {
                // For EmailJS, we send template variables instead of full HTML
                var serviceId = _configuration["EmailJS:ServiceId"];
                var templateId = _configuration["EmailJS:TemplateId"];
                var publicKey = _configuration["EmailJS:PublicKey"];

                if (string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(templateId) || string.IsNullOrEmpty(publicKey))
                {
                    _logger.LogError("EmailJS configuration is missing. Please check ServiceId, TemplateId, and PublicKey.");
                    return false;
                }

                // Initialize EmailJS if not already done
                await _jsRuntime.InvokeVoidAsync("emailJSHelper.init", publicKey);

                var templateParams = new
                {
                    to_email = email,
                    user_email = email,
                    reset_url = resetUrl,
                    from_name = "eSkolar Support",
                    subject = "Reset Your eSkolar Password"
                };

                _logger.LogInformation("Sending password reset email via EmailJS to {Email}", email);

                var result = await _jsRuntime.InvokeAsync<bool>("emailJSHelper.sendEmail", 
                    serviceId, templateId, templateParams);

                if (result)
                {
                    _logger.LogInformation("Password reset email sent successfully via EmailJS to {Email}", email);
                    return true;
                }
                else
                {
                    _logger.LogWarning("EmailJS returned false for password reset email to {Email}", email);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email via EmailJS to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string? plainTextContent = null)
        {
            try
            {
                var serviceId = _configuration["EmailJS:ServiceId"];
                var templateId = _configuration["EmailJS:TemplateId"];
                var publicKey = _configuration["EmailJS:PublicKey"];

                if (string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(templateId) || string.IsNullOrEmpty(publicKey))
                {
                    _logger.LogError("EmailJS configuration is missing. Please check ServiceId, TemplateId, and PublicKey.");
                    return false;
                }

                // Initialize EmailJS if not already done
                await _jsRuntime.InvokeVoidAsync("emailJSHelper.init", publicKey);

                var templateParams = new
                {
                    to_email = to,
                    subject = subject,
                    html_content = htmlContent,
                    text_content = plainTextContent ?? "",
                    from_name = "eSkolar Support"
                };

                _logger.LogInformation("Sending email via EmailJS to {Email} with subject: {Subject}", to, subject);

                var result = await _jsRuntime.InvokeAsync<bool>("emailJSHelper.sendEmail", 
                    serviceId, templateId, templateParams);

                if (result)
                {
                    _logger.LogInformation("Email sent successfully via EmailJS to {Email}", to);
                    return true;
                }
                else
                {
                    _logger.LogWarning("EmailJS returned false for email to {Email}", to);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via EmailJS to {Email}", to);
                return false;
            }
        }

        private string GeneratePasswordResetHtml(string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .button:hover {{ background-color: #0056b3; }}
        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #dee2e6; font-size: 14px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎓 eSkolar</h1>
        <p>Password Reset Request</p>
    </div>
    <div class='content'>
        <h2>Reset Your Password</h2>
        <p>Hello,</p>
        <p>We received a request to reset your password for your eSkolar account. If you made this request, click the button below to reset your password:</p>
        
        <div style='text-align: center;'>
            <a href='{resetUrl}' class='button'>Reset My Password</a>
        </div>
        
        <p>If the button doesn't work, copy and paste this link into your browser:</p>
        <p style='word-break: break-all; background-color: #e9ecef; padding: 10px; border-radius: 4px;'>{resetUrl}</p>
        
        <p><strong>Important:</strong> This link will expire in 24 hours for security reasons.</p>
        
        <p>If you didn't request a password reset, you can safely ignore this email. Your password will not be changed.</p>
        
        <div class='footer'>
            <p>Best regards,<br>The eSkolar Team</p>
            <p><small>This is an automated message, please do not reply to this email.</small></p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordResetText(string resetUrl)
        {
            return $@"
eSkolar - Password Reset Request

Hello,

We received a request to reset your password for your eSkolar account. 

To reset your password, please visit the following link:
{resetUrl}

Important: This link will expire in 24 hours for security reasons.

If you didn't request a password reset, you can safely ignore this email. Your password will not be changed.

Best regards,
The eSkolar Team

This is an automated message, please do not reply to this email.
";
        }
    }
}