using System.Text.RegularExpressions;

namespace BiddingSystem.Data
{
    public interface IInputValidationService
    {
        ValidationResult ValidateInput(string input, InputType type);
        ValidationResult SanitizeHtml(string html);
        ValidationResult ValidateFileUpload(IFormFile file, FileUploadType type);
        ValidationResult ValidateEmail(string email);
        ValidationResult ValidatePhoneNumber(string phone);
        ValidationResult ValidateAmount(decimal amount, decimal? minAmount = null, decimal? maxAmount = null);
        ValidationResult ValidateTenderId(string tenderId);
        ValidationResult ValidateCompanyName(string companyName);
        ValidationResult ValidateBidderName(string bidderName);
    }

    public enum InputType
    {
        General,
        Email,
        Phone,
        Amount,
        TenderId,
        CompanyName,
        BidderName,
        Description,
        Remarks
    }

    public enum FileUploadType
    {
        Document,
        Image,
        Any
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? SanitizedValue { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public static ValidationResult Success(string? sanitizedValue = null)
        {
            return new ValidationResult { IsValid = true, SanitizedValue = sanitizedValue };
        }

        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult { IsValid = false, Errors = errors.ToList() };
        }
    }
}
