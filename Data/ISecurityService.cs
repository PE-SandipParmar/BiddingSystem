namespace BiddingSystem.Data
{
    public interface ISecurityService
    {
        string SanitizeInput(string input);
        bool IsValidTransactionId(string transactionId);
        bool IsValidAmount(decimal amount);
        bool IsValidExpiryDate(DateTime expiryDate);
        string GenerateSecureToken();
        bool ValidateSecurityToken(string token);
    }
}
