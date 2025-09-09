namespace BiddingSystem.ViewModels
{
    // Keep other ViewModels as they were
    public class RefundListViewModel
    {
        public int TenderBidId { get; set; }
        public int TenderId { get; set; }
        public string TenderIdString { get; set; } = string.Empty;
        public string TenderTitle { get; set; } = string.Empty;
        public string BidderName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal TotalAmountToRefund { get; set; }
        public string ReasonForRefund { get; set; } = string.Empty;
        public bool HasPendingRefund { get; set; }
        public int? RefundPaymentId { get; set; }
        public string? RefundStatus { get; set; }
    }

    public class RefundStatistics
    {
        public int TotalPendingRefunds { get; set; }
        public int TotalApprovedRefunds { get; set; }
        public int TotalRejectedRefunds { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal PendingRefundAmount { get; set; }
        public decimal ApprovedRefundAmount { get; set; }
    }

    // Update RefundSearchFilter to include date filtering
    public class RefundSearchFilter
    {
        public string? TenderId { get; set; }
        public string? TenderName { get; set; }
        public string? RefundStatus { get; set; } // For filtering by status
        public DateTime? FromDate { get; set; } // For date range filtering
        public DateTime? ToDate { get; set; }
        public bool ShowPendingOnly { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
    // Add this new ViewModel for approved refunds
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
        public decimal RefundAmount { get; set; }
        public string ReasonForRefund { get; set; } = string.Empty;
        public string RefundStatus { get; set; } = string.Empty;
        public DateTime InitiatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? CheckerRemarks { get; set; }
        public string? ApprovedByName { get; set; }
        public bool PaymentProcessed { get; set; }
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

    public class InitiateRefundRequest
    {
        public List<int> TenderBidIds { get; set; } = new List<int>();
        public string ReasonForRefund { get; set; } = "Tender awarded to another bidder";
    }

    public class ApproveRefundRequest
    {
        public List<int> RefundPaymentIds { get; set; } = new List<int>();
        public string Action { get; set; } = ""; // Approve or Reject
        public string? CheckerRemarks { get; set; }
    }
}
