using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace BiddingSystem.Data
{
    public class SecurityService : ISecurityService
    {
        private readonly ILogger<SecurityService> _logger;
        private static readonly Regex TransactionIdRegex = new(@"^[A-Za-z0-9\-_]{1,100}$", RegexOptions.Compiled);
        private static readonly Regex InputSanitizationRegex = new(@"[<>""'&]", RegexOptions.Compiled);

        public SecurityService(ILogger<SecurityService> logger)
        {
            _logger = logger;
        }

        public string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove potentially dangerous characters
            var sanitized = InputSanitizationRegex.Replace(input, string.Empty);
            
            // Trim whitespace
            sanitized = sanitized.Trim();
            
            // Limit length to prevent buffer overflow attacks
            if (sanitized.Length > 1000)
            {
                sanitized = sanitized.Substring(0, 1000);
                _logger.LogWarning("Input truncated due to excessive length");
            }

            return sanitized;
        }

        public bool IsValidTransactionId(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                return false;

            // Check length
            if (transactionId.Length < 1 || transactionId.Length > 100)
                return false;

            // Check format (alphanumeric, hyphens, underscores only)
            return TransactionIdRegex.IsMatch(transactionId);
        }

        public bool IsValidAmount(decimal amount)
        {
            // Amount must be positive and within reasonable limits
            return amount > 0 && amount <= 999999999.99m; // Max 999 million
        }

        public bool IsValidExpiryDate(DateTime expiryDate)
        {
            var now = DateTime.UtcNow;
            var maxExpiry = now.AddYears(1); // Maximum 1 year from now

            return expiryDate > now && expiryDate <= maxExpiry;
        }

        public string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32]; // 256 bits
            rng.GetBytes(bytes);
            
            // Convert to base64 and make URL-safe
            var token = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
                
            return token;
        }

        public bool ValidateSecurityToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Check length (base64 without padding for 32 bytes = 43 characters)
            if (token.Length != 43)
                return false;

            // Check format (URL-safe base64)
            var base64Regex = new Regex(@"^[A-Za-z0-9\-_]+$");
            return base64Regex.IsMatch(token);
        }
    }
}
