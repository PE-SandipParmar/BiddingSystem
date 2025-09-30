using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class BidderPaymentDashboardViewModel
    {
        public string BidderName { get; set; }
        public string BidderEmail { get; set; }
        public string BidderPhone { get; set; }
        public string CompanyName { get; set; }

        public List<BidderPaymentTenderViewModel> TenderBids { get; set; } = new List<BidderPaymentTenderViewModel>();

        // Summary
        public int TotalBids { get; set; }
        public decimal TotalAmountDue { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal TotalAmountPending { get; set; }
    }

    public class BidderPaymentTenderViewModel
    {
        public int BidId { get; set; }
        public int TenderId { get; set; }
        public string TenderTitle { get; set; }
        public string TenderRefNo { get; set; }

        // Amounts
        public decimal BidAmount { get; set; }
        public decimal EmdAmount { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }

        // Status
        public string BidStatus { get; set; }
        public string OverallPaymentStatus { get; set; }
        public bool IsFullyPaid { get; set; }
        public DateTime SubmittedDate { get; set; }

        // Payment Components
        public List<PaymentComponentViewModel> PaymentComponents { get; set; } = new List<PaymentComponentViewModel>();
    }

    public class PaymentComponentViewModel
    {
        public PaymentType PaymentType { get; set; }
        public string PaymentTypeLabel { get; set; }
        public decimal Amount { get; set; }

        // Payment Link Details
        public int? PaymentLinkId { get; set; }
        public string PaymentLinkUrl { get; set; }
        public string SecurityToken { get; set; }
        public PaymentLinkStatus LinkStatus { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Payment Status
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }

        // Payment Attempts
        public int FailedAttempts { get; set; }
        public DateTime? LastAttemptDate { get; set; }

        // Payment History
        public List<PaymentHistoryViewModel> PaymentHistory { get; set; } = new List<PaymentHistoryViewModel>();

        // Helper Properties
        public string StatusBadgeClass => IsPaid ? "bg-green-100 text-green-800" :
                                         IsExpired ? "bg-red-100 text-red-800" :
                                         "bg-yellow-100 text-yellow-800";

        public string StatusText => IsPaid ? "Paid" :
                                   IsExpired ? "Expired" :
                                   "Pending";

        public bool CanPay => !IsPaid && !IsExpired && LinkStatus == PaymentLinkStatus.Active;
    }

    public class PaymentHistoryViewModel
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ErrorDescription { get; set; }

        public string StatusBadgeClass => Status == "Success" ? "bg-green-100 text-green-800" :
                                         Status == "Failed" ? "bg-red-100 text-red-800" :
                                         "bg-gray-100 text-gray-800";
    }

    public class PaymentReceiptViewModel
    {
        // Transaction Details
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public PaymentType PaymentType { get; set; }

        // Bidder Information
        public string BidderName { get; set; }
        public string BidderEmail { get; set; }
        public string BidderPhone { get; set; }
        public string CompanyName { get; set; }

        // Tender Information
        public string TenderTitle { get; set; }
        public string TenderRefNo { get; set; }

        // Additional Payment Details
        public string BankName { get; set; }
        public string CardLast4 { get; set; }
        public string UPIId { get; set; }
        public string WalletName { get; set; }

        // Display Helpers
        public string AmountDisplay => Amount.ToString("C", new System.Globalization.CultureInfo("en-IN"));
        public string PaymentTypeDisplay => PaymentType.GetDisplayName();

        public string PaymentMethodDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(CardLast4))
                    return $"Card ending in {CardLast4}";
                if (!string.IsNullOrEmpty(UPIId))
                    return $"UPI - {UPIId}";
                if (!string.IsNullOrEmpty(WalletName))
                    return $"Wallet - {WalletName}";
                if (!string.IsNullOrEmpty(BankName))
                    return $"Net Banking - {BankName}";
                return PaymentMethod ?? "Online Payment";
            }
        }
    }

    // Enum Extension
    public static class PaymentLinkStatusExtensions
    {
        public static string GetDisplayName(this PaymentLinkStatus status)
        {
            return status switch
            {
                PaymentLinkStatus.Active => "Active",
                PaymentLinkStatus.Used => "Used",
                PaymentLinkStatus.Expired => "Expired",
                PaymentLinkStatus.Cancelled => "Cancelled",
                PaymentLinkStatus.NotGenerated => "Not Generated",
                _ => status.ToString()
            };
        }
    }
}