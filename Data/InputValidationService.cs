using System.Text.RegularExpressions;
using System.Text;
using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public class InputValidationService : IInputValidationService
    {
        private readonly ILogger<InputValidationService> _logger;
        private readonly SecurityOptions _securityOptions;

        // Regex patterns for validation
        private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        private static readonly Regex PhoneRegex = new(@"^[\+]?[1-9][\d]{0,15}$", RegexOptions.Compiled);
        private static readonly Regex TenderIdRegex = new(@"^[A-Z0-9\-_]{3,50}$", RegexOptions.Compiled);
        private static readonly Regex CompanyNameRegex = new(@"^[a-zA-Z0-9\s\-_\.&]{2,100}$", RegexOptions.Compiled);
        private static readonly Regex BidderNameRegex = new(@"^[a-zA-Z\s]{2,50}$", RegexOptions.Compiled);
        
        // Dangerous patterns for XSS prevention
        private static readonly Regex[] DangerousPatterns = {
            new(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"vbscript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"data:text/html", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };

        public InputValidationService(ILogger<InputValidationService> logger, IOptions<SecurityOptions> securityOptions)
        {
            _logger = logger;
            _securityOptions = securityOptions.Value;
        }

        public ValidationResult ValidateInput(string input, InputType type)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return ValidationResult.Failure("Input cannot be empty");
            }

            // Check for dangerous patterns first
            var sanitized = SanitizeInput(input);
            if (sanitized != input)
            {
                _logger.LogWarning("Potentially dangerous input detected and sanitized: {OriginalInput}", input);
            }

            return type switch
            {
                InputType.Email => ValidateEmail(sanitized),
                InputType.Phone => ValidatePhoneNumber(sanitized),
                InputType.TenderId => ValidateTenderId(sanitized),
                InputType.CompanyName => ValidateCompanyName(sanitized),
                InputType.BidderName => ValidateBidderName(sanitized),
                InputType.Description => ValidateDescription(sanitized),
                InputType.Remarks => ValidateRemarks(sanitized),
                _ => ValidationResult.Success(sanitized)
            };
        }

        public ValidationResult SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return ValidationResult.Success("");

            var sanitized = html;
            
            // Remove dangerous patterns
            foreach (var pattern in DangerousPatterns)
            {
                sanitized = pattern.Replace(sanitized, "");
            }

            // Remove HTML tags but preserve content
            sanitized = Regex.Replace(sanitized, @"<[^>]+>", "");

            // Decode HTML entities
            sanitized = System.Net.WebUtility.HtmlDecode(sanitized);

            return ValidationResult.Success(sanitized);
        }

        public ValidationResult ValidateFileUpload(IFormFile file, FileUploadType type)
        {
            if (file == null || file.Length == 0)
                return ValidationResult.Failure("No file provided");

            var errors = new List<string>();

            // Check file size
            if (file.Length > _securityOptions.FileUpload.MaxFileSizeBytes)
            {
                errors.Add($"File size exceeds maximum allowed size of {_securityOptions.FileUpload.MaxFileSizeBytes / (1024 * 1024)}MB");
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (string.IsNullOrEmpty(extension) || !_securityOptions.FileUpload.AllowedExtensions.Contains(extension))
            {
                errors.Add($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _securityOptions.FileUpload.AllowedExtensions)}");
            }

            // Check MIME type
            if (!_securityOptions.FileUpload.AllowedMimeTypes.Contains(file.ContentType))
            {
                errors.Add($"File MIME type '{file.ContentType}' is not allowed");
            }

            // Check for suspicious file names
            if (ContainsSuspiciousPatterns(file.FileName))
            {
                errors.Add("File name contains suspicious patterns");
            }

            if (errors.Any())
            {
                _logger.LogWarning("File upload validation failed for {FileName}: {Errors}", file.FileName, string.Join(", ", errors));
                return ValidationResult.Failure(errors.ToArray());
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Failure("Email is required");

            email = email.Trim().ToLower();

            if (!EmailRegex.IsMatch(email))
                return ValidationResult.Failure("Invalid email format");

            if (email.Length > 254)
                return ValidationResult.Failure("Email is too long");

            return ValidationResult.Success(email);
        }

        public ValidationResult ValidatePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return ValidationResult.Failure("Phone number is required");

            phone = phone.Trim();

            if (!PhoneRegex.IsMatch(phone))
                return ValidationResult.Failure("Invalid phone number format");

            if (phone.Length < 10 || phone.Length > 15)
                return ValidationResult.Failure("Phone number must be between 10 and 15 digits");

            return ValidationResult.Success(phone);
        }

        public ValidationResult ValidateAmount(decimal amount, decimal? minAmount = null, decimal? maxAmount = null)
        {
            if (amount < 0)
                return ValidationResult.Failure("Amount cannot be negative");

            if (minAmount.HasValue && amount < minAmount.Value)
                return ValidationResult.Failure($"Amount must be at least {minAmount:C}");

            if (maxAmount.HasValue && amount > maxAmount.Value)
                return ValidationResult.Failure($"Amount cannot exceed {maxAmount:C}");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateTenderId(string tenderId)
        {
            if (string.IsNullOrWhiteSpace(tenderId))
                return ValidationResult.Failure("Tender ID is required");

            tenderId = tenderId.Trim().ToUpper();

            if (!TenderIdRegex.IsMatch(tenderId))
                return ValidationResult.Failure("Invalid tender ID format. Use only letters, numbers, hyphens, and underscores");

            return ValidationResult.Success(tenderId);
        }

        public ValidationResult ValidateCompanyName(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return ValidationResult.Failure("Company name is required");

            companyName = companyName.Trim();

            if (!CompanyNameRegex.IsMatch(companyName))
                return ValidationResult.Failure("Invalid company name format");

            if (companyName.Length < 2 || companyName.Length > 100)
                return ValidationResult.Failure("Company name must be between 2 and 100 characters");

            return ValidationResult.Success(companyName);
        }

        public ValidationResult ValidateBidderName(string bidderName)
        {
            if (string.IsNullOrWhiteSpace(bidderName))
                return ValidationResult.Failure("Bidder name is required");

            bidderName = bidderName.Trim();

            if (!BidderNameRegex.IsMatch(bidderName))
                return ValidationResult.Failure("Invalid bidder name format. Use only letters and spaces");

            if (bidderName.Length < 2 || bidderName.Length > 50)
                return ValidationResult.Failure("Bidder name must be between 2 and 50 characters");

            return ValidationResult.Success(bidderName);
        }

        private ValidationResult ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return ValidationResult.Failure("Description is required");

            if (description.Length > 1000)
                return ValidationResult.Failure("Description cannot exceed 1000 characters");

            return ValidationResult.Success(description.Trim());
        }

        private ValidationResult ValidateRemarks(string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
                return ValidationResult.Success("");

            if (remarks.Length > 500)
                return ValidationResult.Failure("Remarks cannot exceed 500 characters");

            return ValidationResult.Success(remarks.Trim());
        }

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var sanitized = input;

            // Remove null bytes
            sanitized = sanitized.Replace("\0", "");

            // Remove control characters except newlines and tabs
            sanitized = Regex.Replace(sanitized, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

            // Trim whitespace
            sanitized = sanitized.Trim();

            return sanitized;
        }

        private bool ContainsSuspiciousPatterns(string fileName)
        {
            var suspiciousPatterns = new[]
            {
                "..", "\\", "/", ":", "*", "?", "\"", "<", ">", "|",
                "script", "javascript", "vbscript", "onload", "onerror"
            };

            return suspiciousPatterns.Any(pattern => 
                fileName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }
}
