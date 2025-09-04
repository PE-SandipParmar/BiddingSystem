using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface IPasswordService
    {
        // Core password operations
        (string hash, string salt) HashPassword(string password);
        bool VerifyPassword(string password, string hash, string salt);

        // Password validation
        bool IsPasswordStrong(string password);
        PasswordValidationResult ValidatePassword(string password, string? username = null, string? email = null);

        // Password generation
        string GenerateRandomPassword(int length = 12);
        string GenerateSecurePassword(PasswordGenerationOptions? options = null);

        // Token generation
        string GenerateResetToken();
        string GenerateVerificationToken();

        // Password policy
        PasswordPolicyResult CheckPasswordPolicy(string password);
        bool IsPasswordExpired(DateTime passwordCreatedDate, int maxDays = 90);

        // Security utilities
        double CalculatePasswordEntropy(string password);
        bool IsCommonPassword(string password);
        bool CheckPasswordHistory(string newPasswordHash, List<string> previousPasswordHashes, int historyCount = 5);
    }
}
