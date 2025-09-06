using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface IPaymentLinkRepository
    {
        // CRUD Operations
        Task<PaymentLink?> GetByIdAsync(int id);
        Task<PaymentLink?> GetByLinkIdAsync(string linkId);
        Task<PaymentLink?> GetBySecurityTokenAsync(string token);
        Task<List<PaymentLink>> GetAllAsync();
        Task<List<PaymentLink>> GetByTenderIdAsync(int tenderId);
        Task<List<PaymentLink>> GetByUserIdAsync(int userId);
        Task<PaymentLink> CreateAsync(PaymentLink paymentLink);
        Task<PaymentLink> UpdateAsync(PaymentLink paymentLink);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string linkId, int? excludeId = null);
        Task<bool> TransactionIdExistsAsync(string transactionId);

        // Search and Filter
        Task<List<PaymentLink>> SearchAsync(string searchTerm, PaymentLinkStatus? status, 
            PaymentType? paymentType, DateTime? fromDate, DateTime? toDate, 
            int page = 1, int pageSize = 25);
        Task<int> GetSearchCountAsync(string searchTerm, PaymentLinkStatus? status, 
            PaymentType? paymentType, DateTime? fromDate, DateTime? toDate);

        // Status Management
        Task<bool> MarkAsUsedAsync(int id, int? userId, string transactionId);
        Task<bool> MarkAsExpiredAsync(int id);
        Task<bool> CancelAsync(int id, string reason);
        Task<List<PaymentLink>> GetExpiredLinksAsync();
        Task<bool> UpdateExpiredStatusAsync();

        // Security
        Task<bool> ValidateTokenAsync(string token);
        Task<string> GenerateUniqueLinkIdAsync();
        Task<bool> IsLinkUsableAsync(int id);

        // Statistics
        Task<Dictionary<PaymentLinkStatus, int>> GetStatusCountsAsync();
        Task<decimal> GetTotalAmountByStatusAsync(PaymentLinkStatus status);
        Task<int> GetActiveLinksCountAsync();
        Task<int> GetExpiredLinksCountAsync();
    }
}
