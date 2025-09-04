namespace BiddingSystem.Data
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, string fullName = "");
        Task<bool> SendWelcomeEmailAsync(string email, string fullName, string username, string temporaryPassword = "");
        Task<bool> SendAccountActivationEmailAsync(string email, string fullName);
        Task<bool> SendRoleChangeNotificationAsync(string email, string fullName, string newRole, string oldRole = "");
        Task<bool> SendAccountDeactivationEmailAsync(string email, string fullName);
        Task<bool> SendPasswordChangedNotificationAsync(string email, string fullName);
        Task<bool> SendCustomEmailAsync(string email, string subject, string body, bool isHtml = true);
        Task<bool> TestEmailConfigurationAsync();
    }

    public class EmailOptions
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "Rent Management System";
        public string CompanyLogo { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public bool EnableEmailSending { get; set; } = true;
    }
}
