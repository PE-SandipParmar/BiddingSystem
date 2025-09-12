using BiddingSystem.Models;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

namespace BiddingSystem.Data
{
    public class RazorpayPaymentService : IRazorpayPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly RazorpayClient _razorpayClient;
        private readonly ILogger<RazorpayPaymentService> _logger;

        public RazorpayPaymentService(IConfiguration configuration, ILogger<RazorpayPaymentService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _razorpayClient = new RazorpayClient(
                _configuration["RazorpaySettings:KeyId"],
                _configuration["RazorpaySettings:KeySecret"]
            );
        }

        public async Task<RazorpayOrderResponse> CreateOrderAsync(BiddingSystem.Models.PaymentLink paymentLink, TenderBid tenderBid)
        {
            try
            {
                // Create Razorpay order
                var options = new Dictionary<string, object>
                {
                    { "amount", (int)(paymentLink.Amount * 100) }, // Amount in paise
                    { "currency", "INR" },
                    { "receipt", paymentLink.LinkId },
                    { "payment_capture", 1 }, // Auto capture
                    { "notes", new Dictionary<string, object>
                        {
                            { "payment_link_id", paymentLink.LinkId },
                            { "tender_id", paymentLink.TenderId.ToString() },
                            { "tender_bid_id", paymentLink.TenderBidId?.ToString() ?? "" },
                            { "payment_type", paymentLink.PaymentType.ToString() },
                            { "bidder_name", tenderBid?.BidderName ?? "" },
                            { "company_name", tenderBid?.CompanyName ?? "" }
                        }
                    }
                };

                Razorpay.Api.Order order = _razorpayClient.Order.Create(options);

                return new RazorpayOrderResponse
                {
                    OrderId = order["id"].ToString(),
                    RazorpayKey = _configuration["RazorpaySettings:KeyId"],
                    Amount = (int)(paymentLink.Amount * 100),
                    Currency = "INR",
                    Receipt = paymentLink.LinkId,
                    Notes = (Dictionary<string, object>)options["notes"]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Razorpay order for LinkId: {LinkId}", paymentLink.LinkId);
                throw;
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(string paymentId, string orderId, string signature)
        {
            try
            {
                var message = orderId + "|" + paymentId;
                var expectedSignature = GenerateSignature(message, _configuration["RazorpaySettings:KeySecret"]);

                if (expectedSignature == signature)
                {
                    // Fetch payment details from Razorpay
                    Payment payment = _razorpayClient.Payment.Fetch(paymentId);

                    return new PaymentVerificationResult
                    {
                        IsValid = true,
                        PaymentId = paymentId,
                        OrderId = orderId,
                        Message = "Payment verified successfully"
                    };
                }

                return new PaymentVerificationResult
                {
                    IsValid = false,
                    Message = "Payment signature verification failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment: {PaymentId}", paymentId);
                return new PaymentVerificationResult
                {
                    IsValid = false,
                    Message = $"Error verifying payment: {ex.Message}"
                };
            }
        }

        public bool VerifyWebhookSignature(string payload, string signature)
        {
            var webhookSecret = _configuration["RazorpaySettings:WebhookSecret"];
            var expectedSignature = GenerateSignature(payload, webhookSecret);
            return expectedSignature == signature;
        }

        private string GenerateSignature(string message, string secret)
        {
            var encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }
    }
}
