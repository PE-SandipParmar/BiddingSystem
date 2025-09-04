using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace BiddingSystem.Data
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> emailOptions, ILogger<EmailService> logger)
        {
            _emailOptions = emailOptions.Value;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, string fullName = "")
        {
            var subject = "Password Reset Request - " + _emailOptions.CompanyName;
            var displayName = !string.IsNullOrEmpty(fullName) ? fullName : "User";

            var body = GeneratePasswordResetEmailBody(displayName, resetLink);

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string fullName, string username, string temporaryPassword = "")
        {
            var subject = $"Welcome to {_emailOptions.CompanyName}!";
            var body = GenerateWelcomeEmailBody(fullName, username, temporaryPassword);

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendAccountActivationEmailAsync(string email, string fullName)
        {
            var subject = "Account Activated - " + _emailOptions.CompanyName;
            var body = GenerateAccountActivationEmailBody(fullName);

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendRoleChangeNotificationAsync(string email, string fullName, string newRole, string oldRole = "")
        {
            var subject = "Account Role Updated - " + _emailOptions.CompanyName;
            var body = GenerateRoleChangeEmailBody(fullName, newRole, oldRole);

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendAccountDeactivationEmailAsync(string email, string fullName)
        {
            var subject = "Account Deactivated - " + _emailOptions.CompanyName;
            var body = GenerateAccountDeactivationEmailBody(fullName);

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendPasswordChangedNotificationAsync(string email, string fullName)
        {
            var subject = "Password Changed Successfully - " + _emailOptions.CompanyName;
            var body = GeneratePasswordChangedEmailBody(fullName);

            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendCustomEmailAsync(string email, string subject, string body, bool isHtml = true)
        {
            return await SendEmailAsync(email, subject, body, isHtml);
        }

        public async Task<bool> TestEmailConfigurationAsync()
        {
            try
            {
                if (!_emailOptions.EnableEmailSending)
                {
                    _logger.LogWarning("Email sending is disabled in configuration");
                    return false;
                }

                using var client = CreateSmtpClient();

                // Test connection without sending email
                await client.SendMailAsync(new MailMessage
                {
                    From = new MailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
                    Subject = "Test Email Configuration",
                    Body = "This is a test email to verify configuration.",
                    IsBodyHtml = false,
                    To = { _emailOptions.FromEmail } // Send to self for testing
                });

                _logger.LogInformation("Email configuration test successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email configuration test failed");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml)
        {
            try
            {
                if (!_emailOptions.EnableEmailSending)
                {
                    _logger.LogInformation($"Email sending disabled. Would send: {subject} to {toEmail}");
                    return true; // Return true in development mode
                }

                if (!IsValidEmail(toEmail))
                {
                    _logger.LogWarning($"Invalid email address: {toEmail}");
                    return false;
                }

                using var client = CreateSmtpClient();
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(toEmail);

                // Add headers for better deliverability
                mailMessage.Headers.Add("X-Mailer", _emailOptions.CompanyName);
                mailMessage.Headers.Add("X-Priority", "3");

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation($"Email sent successfully to {toEmail} with subject: {subject}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP error sending email to {toEmail}: {smtpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"General error sending email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient(_emailOptions.SmtpServer, _emailOptions.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailOptions.SmtpUsername, _emailOptions.SmtpPassword),
                EnableSsl = _emailOptions.EnableSsl,
                Timeout = _emailOptions.TimeoutSeconds * 1000,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            return client;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #region Email Template Methods

        private string GeneratePasswordResetEmailBody(string fullName, string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Password Reset</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ background-color: #3b82f6; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{_emailOptions.CompanyName}</h1>
            <h2>Password Reset Request</h2>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            
            <p>We received a request to reset your password for your {_emailOptions.CompanyName} account.</p>
            
            <p>Click the button below to reset your password:</p>
            
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </p>
            
            <div class='warning'>
                <strong>⚠️ Important:</strong>
                <ul>
                    <li>This link will expire in 1 hour</li>
                    <li>If you didn't request this password reset, please ignore this email</li>
                    <li>For security, never share this link with anyone</li>
                </ul>
            </div>
            
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 3px;'>{resetLink}</p>
            
            <p>If you have any questions, please contact our support team at {_emailOptions.SupportEmail}</p>
            
            <p>Best regards,<br>{_emailOptions.CompanyName} Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {_emailOptions.CompanyName}. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateWelcomeEmailBody(string fullName, string username, string temporaryPassword)
        {
            var passwordSection = !string.IsNullOrEmpty(temporaryPassword)
                ? $@"
                    <div class='warning'>
                        <strong>🔐 Temporary Login Credentials:</strong>
                        <p><strong>Username:</strong> {username}</p>
                        <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                        <p><em>⚠️ Please change your password after your first login for security.</em></p>
                    </div>"
                : $@"<p>Your username is: <strong>{username}</strong></p>";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ background-color: #10b981; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .feature-list {{ background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to {_emailOptions.CompanyName}!</h1>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            
            <p>Welcome to {_emailOptions.CompanyName}! Your account has been created successfully and you're now part of our community.</p>
            
            {passwordSection}
            
            <div class='feature-list'>
                <h3>🚀 What you can do now:</h3>
                <ul>
                    <li>Access your personalized dashboard</li>
                    <li>Manage your profile and preferences</li>
                    <li>Connect with team members</li>
                    <li>Explore all available features</li>
                </ul>
            </div>
            
            <p style='text-align: center;'>
                <a href='{_emailOptions.WebsiteUrl}/Account/Login' class='button'>Login to Your Account</a>
            </p>
            
            <p>If you have any questions or need assistance, don't hesitate to reach out to our support team at {_emailOptions.SupportEmail}</p>
            
            <p>We're excited to have you on board!</p>
            
            <p>Best regards,<br>{_emailOptions.CompanyName} Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {_emailOptions.CompanyName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateAccountActivationEmailBody(string fullName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Account Activated</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ background-color: #10b981; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Account Activated</h1>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            
            <p>Great news! Your account has been activated and you now have full access to {_emailOptions.CompanyName}.</p>
            
            <p>You can now:</p>
            <ul>
                <li>Login to your account</li>
                <li>Access all available features</li>
                <li>Update your profile information</li>
                <li>Start using the system</li>
            </ul>
            
            <p style='text-align: center;'>
                <a href='{_emailOptions.WebsiteUrl}/Account/Login' class='button'>Login Now</a>
            </p>
            
            <p>If you experience any issues accessing your account, please contact our support team at {_emailOptions.SupportEmail}</p>
            
            <p>Best regards,<br>{_emailOptions.CompanyName} Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {_emailOptions.CompanyName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateRoleChangeEmailBody(string fullName, string newRole, string oldRole)
        {
            var roleChangeText = !string.IsNullOrEmpty(oldRole)
                ? $"Your role has been updated from <strong>{oldRole}</strong> to <strong>{newRole}</strong>."
                : $"Your role has been set to <strong>{newRole}</strong>.";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Role Updated</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #f59e0b; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ background-color: #f59e0b; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
        .role-info {{ background-color: #fef3c7; border: 1px solid #f59e0b; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔄 Role Updated</h1>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            
            <p>This is to notify you that your account role has been updated in {_emailOptions.CompanyName}.</p>
            
            <div class='role-info'>
                <p>{roleChangeText}</p>
            </div>
            
            <p>With your new role, you may now have access to different features and capabilities within the system. Please log in to explore your updated permissions.</p>
            
            <p style='text-align: center;'>
                <a href='{_emailOptions.WebsiteUrl}/Account/Login' class='button'>Login to Your Account</a>
            </p>
            
            <p>If you have any questions about your new role or need assistance, please contact our support team at {_emailOptions.SupportEmail}</p>
            
            <p>Best regards,<br>{_emailOptions.CompanyName} Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {_emailOptions.CompanyName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateAccountDeactivationEmailBody(string fullName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Account Deactivated</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #ef4444; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
        .warning {{ background-color: #fef2f2; border: 1px solid #ef4444; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>❌ Account Deactivated</h1>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            
            <p>This is to inform you that your account with {_emailOptions.CompanyName} has been deactivated.</p>
            
            <div class='warning'>
                <p><strong>What this means:</strong></p>
                <ul>
                    <li>You will no longer be able to log in to your account</li>
                    <li>Access to system features has been suspended</li>
                    <li>Your data remains secure and preserved</li>
                </ul>
            </div>
            
            <p>If you believe this deactivation was made in error or if you need to reactivate your account, please contact our support team immediately at {_emailOptions.SupportEmail}</p>
            
            <p>Thank you for being part of {_emailOptions.CompanyName}.</p>
            
            <p>Best regards,<br>{_emailOptions.CompanyName} Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {_emailOptions.CompanyName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordChangedEmailBody(string fullName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Password Changed</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
        .security-info {{ background-color: #d1fae5; border: 1px solid #10b981; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .warning {{ background-color: #fef2f2; border: 1px solid #ef4444; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Changed Successfully</h1>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            
            <p>This is a confirmation that your password for your {_emailOptions.CompanyName} account has been successfully changed.</p>
            
            <div class='security-info'>
                <p><strong>✅ Security Details:</strong></p>
                <ul>
                    <li>Date & Time: {DateTime.Now:dddd, MMMM dd, yyyy 'at' HH:mm} UTC</li>
                    <li>Action: Password Updated</li>
                    <li>Status: Successful</li>
                </ul>
            </div>
            
            <div class='warning'>
                <p><strong>⚠️ If you did not make this change:</strong></p>
                <ul>
                    <li>Your account may have been compromised</li>
                    <li>Contact our support team immediately at {_emailOptions.SupportEmail}</li>
                    <li>Consider enabling additional security measures</li>
                </ul>
            </div>
            
            <p>For your security, we recommend:</p>
            <ul>
                <li>Using a unique, strong password for your account</li>
                <li>Not sharing your login credentials with anyone</li>
                <li>Logging out from shared or public computers</li>
                <li>Regularly updating your password</li>
            </ul>
            
            <p>If you have any questions or concerns about account security, please don't hesitate to contact our support team.</p>
            
            <p>Best regards,<br>{_emailOptions.CompanyName} Security Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {_emailOptions.CompanyName}. All rights reserved.</p>
            <p>This is an automated security notification.</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }
}
