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

    public class RefundResponse
    {
        public bool Success { get; set; }
        public string RefundId { get; set; }
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string SpeedProcessed { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Receipt { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public string RawResponse { get; set; }
    }

    public class RazorRefundStatus
    {
        public string RefundId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
