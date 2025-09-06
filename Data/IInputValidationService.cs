using System.Text.RegularExpressions;

namespace BiddingSystem.Data
{
    public interface IInputValidationService
    {
        InputValidationResult ValidateInput(string input, InputType type);
        InputValidationResult SanitizeHtml(string html);
        InputValidationResult ValidateFileUpload(IFormFile file, FileUploadType type);
        InputValidationResult ValidateEmail(string email);
        InputValidationResult ValidatePhoneNumber(string phone);
        InputValidationResult ValidateAmount(decimal amount, decimal? minAmount = null, decimal? maxAmount = null);
        InputValidationResult ValidateTenderId(string tenderId);
        InputValidationResult ValidateCompanyName(string companyName);
        InputValidationResult ValidateBidderName(string bidderName);
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

    public class InputValidationResult
    {
        public bool IsValid { get; set; }
        public string? SanitizedValue { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public static InputValidationResult Success(string? sanitizedValue = null)
        {
            return new InputValidationResult { IsValid = true, SanitizedValue = sanitizedValue };
        }

        public static InputValidationResult Failure(params string[] errors)
        {
            return new InputValidationResult { IsValid = false, Errors = errors.ToList() };
        }
    }
}
