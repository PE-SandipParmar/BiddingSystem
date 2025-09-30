using System;
using System.ComponentModel.DataAnnotations;
using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class PaymentTransactionViewModel
    {
        public int Id { get; set; }
        public string PaymentLinkId { get; set; }
        public int TenderId { get; set; }
        public string TenderIdString { get; set; }
        public string TenderTitle { get; set; }
        public int? TenderBidId { get; set; }
        public string BidderName { get; set; }
        public string CompanyName { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string BankName { get; set; }
        public string CardLast4 { get; set; }
        public string UPIId { get; set; }
        public string WalletName { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public PaymentType? PaymentType { get; set; }
        public string PaymentTypeDisplay => PaymentType?.ToString() ?? "N/A";

        // Display helpers
        public string StatusClass
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "success" => "bg-green-100 text-green-800",
                    "failed" => "bg-red-100 text-red-800",
                    "pending" => "bg-yellow-100 text-yellow-800",
                    "refunded" => "bg-blue-100 text-blue-800",
                    _ => "bg-gray-100 text-gray-800"
                };
            }
        }

        public string PaymentMethodDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(PaymentMethod))
                {
                    if (PaymentMethod == "REFUND") return "Refund";
                    if (!string.IsNullOrEmpty(BankName)) return $"{PaymentMethod} - {BankName}";
                    if (!string.IsNullOrEmpty(CardLast4)) return $"Card ****{CardLast4}";
                    if (!string.IsNullOrEmpty(UPIId)) return $"UPI - {UPIId}";
                    if (!string.IsNullOrEmpty(WalletName)) return $"Wallet - {WalletName}";
                    return PaymentMethod;
                }
                return "N/A";
            }
        }
    }

    public class PaymentTransactionSearchFilter
    {
        public string TenderId { get; set; }
        public string TenderName { get; set; }
        public string BidderName { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public PaymentType? PaymentType { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class TransactionStatistics
    {
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int RefundedTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal TodayAmount { get; set; }
        public int TodayTransactions { get; set; }
    }
}