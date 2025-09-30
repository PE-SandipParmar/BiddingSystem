using BiddingSystem.Models;
using BiddingSystem.ViewModels;

namespace BiddingSystem.Data
{
    public interface IPaymentTransactionRepository
    {
        // Core transaction operations
        Task<PaymentTransaction> GetByIdAsync(int id);
        Task<PaymentTransaction?> GetByPaymentIdAsync(string razorpayPaymentId);
        Task<List<PaymentTransaction>> GetByPaymentLinkIdAsync(string paymentLinkId);
        Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction);
        Task<bool> ExistsAsync(string razorpayPaymentId);
        Task<List<PaymentTransaction>> GetByTenderIdAsync(int tenderId);


        // New method for transaction list with filters
        Task<PaginatedList<PaymentTransactionViewModel>> GetTransactionListAsync(PaymentTransactionSearchFilter filter);
        Task<TransactionStatistics> GetTransactionStatisticsAsync();
    }

    public interface IWebhookEventRepository
    {
        Task<WebhookEvent> CreateAsync(WebhookEvent webhookEvent);
        Task<WebhookEvent> UpdateAsync(WebhookEvent webhookEvent);
        Task<WebhookEvent?> GetByPaymentIdAsync(string razorpayPaymentId, string eventType);
        Task<List<WebhookEvent>> GetUnprocessedEventsAsync();
    }
}