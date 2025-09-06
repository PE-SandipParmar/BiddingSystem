using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class RefundRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Refund ID")]
        public string RefundId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tender Bid ID")]
        public int TenderBidId { get; set; }

        [ForeignKey("TenderBidId")]
        public virtual TenderBid TenderBid { get; set; } = null!;

        [Required]
        [Display(Name = "Payment Link ID")]
        public int PaymentLinkId { get; set; }

        [ForeignKey("PaymentLinkId")]
        public virtual PaymentLink PaymentLink { get; set; } = null!;

        [Required]
        [Display(Name = "Refund Type")]
        public RefundType Type { get; set; } = RefundType.Full;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Requested Amount")]
        public decimal RequestedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Approved Amount")]
        public decimal? ApprovedAmount { get; set; }

        [Required]
        [Display(Name = "Refund Reason")]
        public RefundReason Reason { get; set; } = RefundReason.BidRejected;

        [Required]
        [Display(Name = "Status")]
        public RefundStatus Status { get; set; } = RefundStatus.Pending;

        [Required]
        [StringLength(200)]
        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; } = string.Empty;

        [Display(Name = "Processed By")]
        public int? ProcessedBy { get; set; }

        [ForeignKey("ProcessedBy")]
        public virtual User? ProcessedByUser { get; set; }

        [Required]
        [Display(Name = "Requested At")]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Processed At")]
        public DateTime? ProcessedAt { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(1000)]
        [Display(Name = "Bank Details")]
        public string? BankDetails { get; set; }

        [StringLength(100)]
        [Display(Name = "Refund Reference")]
        public string? RefundReference { get; set; }

        [Required]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<RefundTransaction> Transactions { get; set; } = new List<RefundTransaction>();

        // Computed properties
        [NotMapped]
        public string StatusDisplayName => Status.GetDisplayName();

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            RefundStatus.Pending => "bg-yellow-100 text-yellow-800",
            RefundStatus.Approved => "bg-green-100 text-green-800",
            RefundStatus.Processing => "bg-blue-100 text-blue-800",
            RefundStatus.Completed => "bg-green-100 text-green-800",
            RefundStatus.Rejected => "bg-red-100 text-red-800",
            RefundStatus.Failed => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };

        [NotMapped]
        public string TypeDisplayName => Type.GetDisplayName();

        [NotMapped]
        public string ReasonDisplayName => Reason.GetDisplayName();

        [NotMapped]
        public string RequestedAmountDisplay => RequestedAmount.ToString("C", new System.Globalization.CultureInfo("en-IN"));

        [NotMapped]
        public string ApprovedAmountDisplay => ApprovedAmount?.ToString("C", new System.Globalization.CultureInfo("en-IN")) ?? "N/A";

        // Helper methods
        public static string GenerateRefundId()
        {
            return $"REF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        public bool CanBeProcessed()
        {
            return Status == RefundStatus.Approved && ApprovedAmount.HasValue && ApprovedAmount > 0;
        }

        public bool IsCompleted()
        {
            return Status == RefundStatus.Completed;
        }

        public bool IsRejected()
        {
            return Status == RefundStatus.Rejected;
        }
    }

    public enum RefundType
    {
        [Display(Name = "Full Refund")]
        Full = 1,

        [Display(Name = "Partial Refund")]
        Partial = 2,

        [Display(Name = "Processing Fee Refund")]
        ProcessingFee = 3,

        [Display(Name = "EMD Refund")]
        EMD = 4,

        [Display(Name = "SD Refund")]
        SD = 5
    }

    public enum RefundReason
    {
        [Display(Name = "Bid Rejected")]
        BidRejected = 1,

        [Display(Name = "Bid Withdrawn")]
        BidWithdrawn = 2,

        [Display(Name = "Tender Cancelled")]
        TenderCancelled = 3,

        [Display(Name = "Payment Error")]
        PaymentError = 4,

        [Display(Name = "Duplicate Payment")]
        DuplicatePayment = 5,

        [Display(Name = "Technical Issue")]
        TechnicalIssue = 6,

        [Display(Name = "Other")]
        Other = 7
    }

    public enum RefundStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Approved")]
        Approved = 2,

        [Display(Name = "Processing")]
        Processing = 3,

        [Display(Name = "Completed")]
        Completed = 4,

        [Display(Name = "Rejected")]
        Rejected = 5,

        [Display(Name = "Failed")]
        Failed = 6
    }

    public static class RefundTypeExtensions
    {
        public static string GetDisplayName(this RefundType type)
        {
            return type switch
            {
                RefundType.Full => "Full Refund",
                RefundType.Partial => "Partial Refund",
                RefundType.ProcessingFee => "Processing Fee Refund",
                RefundType.EMD => "EMD Refund",
                RefundType.SD => "SD Refund",
                _ => "Unknown"
            };
        }
    }

    public static class RefundReasonExtensions
    {
        public static string GetDisplayName(this RefundReason reason)
        {
            return reason switch
            {
                RefundReason.BidRejected => "Bid Rejected",
                RefundReason.BidWithdrawn => "Bid Withdrawn",
                RefundReason.TenderCancelled => "Tender Cancelled",
                RefundReason.PaymentError => "Payment Error",
                RefundReason.DuplicatePayment => "Duplicate Payment",
                RefundReason.TechnicalIssue => "Technical Issue",
                RefundReason.Other => "Other",
                _ => "Unknown"
            };
        }
    }

    public static class RefundStatusExtensions
    {
        public static string GetDisplayName(this RefundStatus status)
        {
            return status switch
            {
                RefundStatus.Pending => "Pending",
                RefundStatus.Approved => "Approved",
                RefundStatus.Processing => "Processing",
                RefundStatus.Completed => "Completed",
                RefundStatus.Rejected => "Rejected",
                RefundStatus.Failed => "Failed",
                _ => "Unknown"
            };
        }
    }
}
