// Location: /Services/RazorpayService.cs
// This is a NEW FILE - create it in your Services folder

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Razorpay.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BiddingSystem.Services
{
    public interface IRazorpayService
    {
        Task<RefundResponse> ProcessRefund(string paymentId, decimal amount, string reason, Dictionary<string, object> notes = null);
        Task<RazorRefundStatus> GetRefundStatus(string refundId);
    }

    public class RazorpayService : IRazorpayService
    {
        private readonly RazorpayClient _client;
        private readonly ILogger<RazorpayService> _logger;
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayService(IConfiguration configuration, ILogger<RazorpayService> logger)
        {
            _logger = logger;
            _keyId = configuration["RazorpaySettings:KeyId"];
            _keySecret = configuration["RazorpaySettings:KeySecret"];
            _client = new RazorpayClient(_keyId, _keySecret);
        }

        public async Task<RefundResponse> ProcessRefund(string paymentId, decimal amount, string reason, Dictionary<string, object> notes = null)
        {
            try
            {
                _logger.LogInformation($"Processing refund for payment: {paymentId}, Amount: {amount}");

                // Convert amount to paise (Razorpay uses smallest currency unit)
                var amountInPaise = (int)(amount * 100);

                var refundRequest = new Dictionary<string, object>
                {
                    { "amount", amountInPaise },
                    { "speed", "optimum" }, // "normal" or "optimum"
                    { "receipt", $"REFUND_{DateTime.Now:yyyyMMddHHmmss}" }
                };

                if (notes != null)
                {
                    refundRequest.Add("notes", notes);
                }

                // Create refund using Razorpay API - this is synchronous
                var refund = _client.Payment.Fetch(paymentId).Refund(refundRequest);

                var response = new RefundResponse
                {
                    Success = true,
                    RefundId = refund.Attributes["id"].ToString(),
                    PaymentId = refund.Attributes["payment_id"].ToString(),
                    Amount = Convert.ToDecimal(refund.Attributes["amount"]) / 100,
                    Status = refund.Attributes["status"].ToString(),
                    SpeedProcessed = refund.Attributes.ContainsKey("speed_processed") ? refund.Attributes["speed_processed"].ToString() : "normal",
                    CreatedAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(refund.Attributes["created_at"])).DateTime,
                    Receipt = refund.Attributes.ContainsKey("receipt") ? refund.Attributes["receipt"].ToString() : null,
                    RawResponse = JsonConvert.SerializeObject(refund.Attributes)
                };

                _logger.LogInformation($"Refund created successfully: {response.RefundId}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for payment: {paymentId}");
                return new RefundResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = ex.GetType().Name
                };
            }
        }

        public async Task<RazorRefundStatus> GetRefundStatus(string refundId)
        {
            try
            {
                var refund = _client.Refund.Fetch(refundId);

                return new RazorRefundStatus
                {
                    RefundId = refund.Attributes["id"].ToString(),
                    Status = refund.Attributes["status"].ToString(),
                    Amount = Convert.ToDecimal(refund.Attributes["amount"]) / 100,
                    ProcessedAt = refund.Attributes.ContainsKey("processed_at") && refund.Attributes["processed_at"] != null
                        ? DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(refund.Attributes["processed_at"])).DateTime
                        : (DateTime?)null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching refund status: {refundId}");
                throw;
            }
        }
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