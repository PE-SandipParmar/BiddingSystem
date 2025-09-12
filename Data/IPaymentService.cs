using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface IRazorpayPaymentService
    {
        Task<RazorpayOrderResponse> CreateOrderAsync(PaymentLink paymentLink, TenderBid tenderBid);
        Task<PaymentVerificationResult> VerifyPaymentAsync(string paymentId, string orderId, string signature);
        bool VerifyWebhookSignature(string payload, string signature);
    }

    public class RazorpayOrderResponse
    {
        public string OrderId { get; set; }
        public string RazorpayKey { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
        public Dictionary<string, object> Notes { get; set; }
    }

    public class PaymentVerificationResult
    {
        public bool IsValid { get; set; }
        public string PaymentId { get; set; }
        public string OrderId { get; set; }
        public string Message { get; set; }
    }
}
