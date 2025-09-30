using BiddingSystem.Models;
using System;
using System.Collections.Generic;

namespace BiddingSystem.ViewModels
{
    // Keep RefundRequestItem in ViewModels namespace
    public class RefundRequestItem
    {
        public int TenderBidId { get; set; }
        public int PaymentLinkId { get; set; }
        public PaymentType PaymentType { get; set; }
    }

    // Updated InitiateRefundRequest to use RefundRequestItem
    public class InitiateRefundRequest
    {
        public List<RefundRequestItem> RefundItems { get; set; } = new List<RefundRequestItem>();
        public string ReasonForRefund { get; set; } = "Tender awarded to another bidder";
    }

    // Updated RefundListViewModel
    public class RefundListViewModel
    {
        public int TenderBidId { get; set; }
        public int TenderId { get; set; }
        public string TenderIdString { get; set; } = string.Empty;
        public string TenderTitle { get; set; } = string.Empty;
        public string BidderName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        // Payment specific fields
        public PaymentType PaymentType { get; set; }
        public string PaymentTypeDisplay { get; set; } = string.Empty;
        public int? PaymentLinkId { get; set; }
        public string? PaymentLinkStatus { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public decimal PaymentAmount { get; set; }

        // Refund fields
        public decimal TotalAmountToRefund { get; set; }
        public string ReasonForRefund { get; set; } = string.Empty;
        public bool HasPendingRefund { get; set; }
        public int? RefundPaymentId { get; set; }
        public string? RefundStatus { get; set; }
        public string? RazorpayRefundId { get; set; }
        public string? RefundErrorMessage { get; set; }

        // Computed properties
        public bool IsPaymentComplete => PaymentLinkStatus == "Used";
        public bool CanInitiateRefund => IsPaymentComplete && !HasPendingRefund && RefundStatus != "Approved";
        public string? TransactionDetails { get; set; } // Added for showing transaction info
    }

    // Updated ApproveRefundRequest
    public class ApproveRefundRequest
    {
        public List<int> RefundPaymentIds { get; set; } = new List<int>();
        public string Action { get; set; } = ""; // Approve or Reject
        public string? CheckerRemarks { get; set; }
    }

    // Updated ApprovedRefundViewModel
    public class ApprovedRefundViewModel
    {
        public int RefundPaymentId { get; set; }
        public int TenderBidId { get; set; }
        public int TenderId { get; set; }
        public string TenderIdString { get; set; } = string.Empty;
        public string TenderTitle { get; set; } = string.Empty;
        public string BidderName { get; set; } = string.Empty;
        public string BidderEmail { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        // Payment type details
        public PaymentType? PaymentType { get; set; }
        public string PaymentTypeDisplay { get; set; } = string.Empty;
        public string? OriginalPaymentId { get; set; }

        public decimal RefundAmount { get; set; }
        public string ReasonForRefund { get; set; } = string.Empty;
        public string RefundStatus { get; set; } = string.Empty;
        public DateTime InitiatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? CheckerRemarks { get; set; }
        public string? ApprovedByName { get; set; }
        public bool PaymentProcessed { get; set; }
        public string? RazorpayRefundId { get; set; }
        public string? RefundErrorMessage { get; set; }
    }

    // Keep existing classes
    public class RefundStatistics
    {
        public int TotalPendingRefunds { get; set; }
        public int TotalApprovedRefunds { get; set; }
        public int TotalRejectedRefunds { get; set; }
        public int TotalFailedRefunds { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal PendingRefundAmount { get; set; }
        public decimal ApprovedRefundAmount { get; set; }
    }

    public class RefundSearchFilter
    {
        public string? TenderId { get; set; }
        public string? TenderName { get; set; }
        public string? RefundStatus { get; set; }
        public PaymentType? PaymentType { get; set; } // Filter by payment type
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool ShowPendingOnly { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    //public class RefundPaymentInfo
    //{
    //    public int RefundPaymentId { get; set; }
    //    public decimal RefundAmount { get; set; }
    //    public string RefundStatus { get; set; } = string.Empty;
    //    public string? RazorpayRefundId { get; set; }
    //    public string ReasonForRefund { get; set; } = string.Empty;
    //    public DateTime? InitiatedAt { get; set; }
    //    public DateTime? ApprovedAt { get; set; }
    //    public string? CheckerRemarks { get; set; }
    //    public string? RefundErrorMessage { get; set; }
    //    public string? InitiatedByName { get; set; }
    //    public string? ApprovedByName { get; set; }
    //    public string? OriginalPaymentId { get; set; }
    //    public decimal? OriginalPaymentAmount { get; set; }
    //}

    // Supporting classes for refund operations
    public class PaymentRefundSummary
    {
        public int PaymentLinkId { get; set; }
        public int TenderBidId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string PaymentTypeDisplay { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? LinkId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public PaymentLinkStatus PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Refund details if exists
        public int? RefundPaymentId { get; set; }
        public string? RefundStatus { get; set; }
        public string? RazorpayRefundId { get; set; }
        public DateTime? RefundInitiatedDate { get; set; }
        public DateTime? RefundApprovedDate { get; set; }
        public string? RefundErrorMessage { get; set; }
        public decimal? RefundAmount { get; set; }

        // Computed properties
        public bool CanInitiateRefund => PaymentStatus == PaymentLinkStatus.Used &&
                                         string.IsNullOrEmpty(RefundStatus);
        public bool IsRefundComplete => RefundStatus == "Approved" &&
                                        !string.IsNullOrEmpty(RazorpayRefundId);
    }

    public class RetryRefundResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RazorpayRefundId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Dictionary<string, string>? AdditionalInfo { get; set; }
    }

    public class RetryRefundRequest
    {
        public int RefundPaymentId { get; set; }
    }

    public class RefundHistoryItem
    {
        public int RefundPaymentId { get; set; }
        public string RefundStatus { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public DateTime InitiatedAt { get; set; }
        public string InitiatedBy { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RazorpayRefundId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Remarks { get; set; }
    }

    public class RefundValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<RefundValidationItem> ValidationItems { get; set; } = new List<RefundValidationItem>();
    }

    public class RefundValidationItem
    {
        public int PaymentLinkId { get; set; }
        public PaymentType PaymentType { get; set; }
        public bool IsValid { get; set; }
        public string? ErrorReason { get; set; }
    }

    public class BulkRetryResult
    {
        public int TotalAttempted { get; set; }
        public int SuccessfulCount { get; set; }
        public int FailedCount { get; set; }
        public List<IndividualRetryResult> Results { get; set; } = new List<IndividualRetryResult>();
    }

    public class IndividualRetryResult
    {
        public int RefundPaymentId { get; set; }
        public bool Success { get; set; }
        public string? RazorpayRefundId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RefundExportData
    {
        public List<RefundExportRow> Rows { get; set; } = new List<RefundExportRow>();
        public RefundExportSummary Summary { get; set; } = new RefundExportSummary();
        public DateTime GeneratedAt { get; set; }
    }

    public class RefundExportRow
    {
        public string TenderId { get; set; } = string.Empty;
        public string TenderTitle { get; set; } = string.Empty;
        public string BidderName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public decimal PaymentAmount { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public string RefundStatus { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string? RefundId { get; set; }
        public DateTime? RefundInitiatedDate { get; set; }
        public DateTime? RefundApprovedDate { get; set; }
        public string? InitiatedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RefundReason { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RefundExportSummary
    {
        public int TotalRefunds { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public Dictionary<string, int> RefundsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<PaymentType, decimal> AmountByPaymentType { get; set; } = new Dictionary<PaymentType, decimal>();
    }
}