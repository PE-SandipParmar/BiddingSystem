// BidderPaymentDashboardViewModel.cs
using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class BidderPaymentDashboardViewModel
    {
        public string BidderName { get; set; }
        public string BidderEmail { get; set; }
        public string BidderPhone { get; set; }
        public string CompanyName { get; set; }

        public List<BidderPaymentTenderViewModel> TenderBids { get; set; }

        // Summary Statistics
        public int TotalBids { get; set; }
        public decimal TotalAmountDue { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal TotalAmountPending { get; set; }
        public decimal TotalRefunded { get; set; } // New property for total refunded amount

        public BidderPaymentDashboardViewModel()
        {
            TenderBids = new List<BidderPaymentTenderViewModel>();
        }
    }

    public class BidderPaymentTenderViewModel
    {
        public int BidId { get; set; }
        public int TenderId { get; set; }
        public string TenderTitle { get; set; }
        public string TenderRefNo { get; set; }

        // Bid Information
        public decimal BidAmount { get; set; }
        public decimal EmdAmount { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string BidStatus { get; set; }
        public string OverallPaymentStatus { get; set; }
        public DateTime SubmittedDate { get; set; }

        // Payment Summary
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
        public decimal TotalRefunded { get; set; } // New property for total refunded at tender level
        public bool IsFullyPaid { get; set; }
        public bool HasRefunds { get; set; } // New property to indicate if any refunds exist

        public List<PaymentComponentViewModel> PaymentComponents { get; set; }

        public BidderPaymentTenderViewModel()
        {
            PaymentComponents = new List<PaymentComponentViewModel>();
        }
    }

    public class PaymentComponentViewModel
    {
        // Payment Information
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

        // Failed Attempts
        public int FailedAttempts { get; set; }
        public DateTime? LastAttemptDate { get; set; }

        // Refund Information - NEW PROPERTIES
        public bool HasRefund { get; set; }
        public string RefundStatus { get; set; } // Initiated, Processing, Completed, Failed
        public decimal RefundAmount { get; set; }
        public string RefundTransactionId { get; set; }
        public DateTime? RefundDate { get; set; }
        public DateTime? RefundInitiatedDate { get; set; }
        public string RefundReason { get; set; }
        public string RefundRemarks { get; set; }
        public string RefundProcessedBy { get; set; }

        // Payment History
        public List<PaymentHistoryViewModel> PaymentHistory { get; set; }

        // Computed Properties
        public bool CanPay => !IsPaid && !IsExpired && LinkStatus == PaymentLinkStatus.Active;

        public string StatusText
        {
            get
            {
                if (HasRefund)
                {
                    if (RefundStatus == "Completed") return "Refunded";
                    if (RefundStatus == "Processing") return "Refund Processing";
                    if (RefundStatus == "Initiated") return "Refund Initiated";
                }
                if (IsPaid) return "Paid";
                if (IsExpired) return "Expired";
                if (LinkStatus == PaymentLinkStatus.Active) return "Pending";
                return "Not Generated";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (HasRefund)
                {
                    if (RefundStatus == "Completed") return "bg-indigo-100 text-indigo-800";
                    if (RefundStatus == "Processing") return "bg-blue-100 text-blue-800";
                    if (RefundStatus == "Initiated") return "bg-yellow-100 text-yellow-800";
                }
                if (IsPaid) return "bg-green-100 text-green-800";
                if (IsExpired) return "bg-red-100 text-red-800";
                if (LinkStatus == PaymentLinkStatus.Active) return "bg-yellow-100 text-yellow-800";
                return "bg-gray-100 text-gray-800";
            }
        }

        public PaymentComponentViewModel()
        {
            PaymentHistory = new List<PaymentHistoryViewModel>();
        }
    }

    public class PaymentHistoryViewModel
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ErrorDescription { get; set; }
    }

    public class RefundDetailsViewModel
    {
        public int BidId { get; set; }
        public string TenderTitle { get; set; }
        public string BidderName { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRefunded { get; set; }
        public decimal PendingRefund { get; set; }
        public List<RefundComponentViewModel> RefundComponents { get; set; }

        public RefundDetailsViewModel()
        {
            RefundComponents = new List<RefundComponentViewModel>();
        }
    }

    public class RefundComponentViewModel
    {
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string RefundId { get; set; }
        public DateTime? RefundDate { get; set; }
        public string RefundMethod { get; set; }
        public string Reason { get; set; }
        public string ProcessedBy { get; set; }
    }
}