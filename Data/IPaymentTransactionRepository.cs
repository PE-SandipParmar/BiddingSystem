using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction?> GetByPaymentIdAsync(string razorpayPaymentId);
        Task<PaymentTransaction?> GetByPaymentLinkIdAsync(string paymentLinkId);
        Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction);
        Task<bool> ExistsAsync(string razorpayPaymentId);
        Task<List<PaymentTransaction>> GetByTenderIdAsync(int tenderId);
    }

    public interface IWebhookEventRepository
    {
        Task<WebhookEvent> CreateAsync(WebhookEvent webhookEvent);
        Task<WebhookEvent> UpdateAsync(WebhookEvent webhookEvent);
        Task<WebhookEvent?> GetByPaymentIdAsync(string razorpayPaymentId, string eventType);
        Task<List<WebhookEvent>> GetUnprocessedEventsAsync();
    }
}
