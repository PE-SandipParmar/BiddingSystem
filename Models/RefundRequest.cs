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

        [Display(Name = "EMD/SD Deposit ID")]
        public int? EMDSDDepositId { get; set; }

        [ForeignKey("EMDSDDepositId")]
        public virtual EMDSDDeposit? EMDSDDeposit { get; set; }

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

        [Display(Name = "Approved By")]
        public int? ApprovedBy { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByUser { get; set; }

        [Display(Name = "Approved At")]
        public DateTime? ApprovedAt { get; set; }

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

        // Checker-Maker Workflow Fields
        [Display(Name = "Created By (Maker)")]
        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [Display(Name = "First Checker")]
        public int? FirstCheckerId { get; set; }

        [ForeignKey("FirstCheckerId")]
        public virtual User? FirstChecker { get; set; }

        [Display(Name = "First Checker Approval Date")]
        public DateTime? FirstCheckerApprovedAt { get; set; }

        [Display(Name = "First Checker Remarks")]
        [StringLength(500)]
        public string? FirstCheckerRemarks { get; set; }

        [Display(Name = "Second Checker")]
        public int? SecondCheckerId { get; set; }

        [ForeignKey("SecondCheckerId")]
        public virtual User? SecondChecker { get; set; }

        [Display(Name = "Second Checker Approval Date")]
        public DateTime? SecondCheckerApprovedAt { get; set; }

        [Display(Name = "Second Checker Remarks")]
        [StringLength(500)]
        public string? SecondCheckerRemarks { get; set; }

        [Display(Name = "Workflow Status")]
        public RefundWorkflowStatus WorkflowStatus { get; set; } = RefundWorkflowStatus.Draft;

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


        public bool IsCompleted()
        {
            return Status == RefundStatus.Completed;
        }

        public bool IsRejected()
        {
            return Status == RefundStatus.Rejected;
        }

        // Checker-Maker Workflow Helper Methods
        public bool CanBeSubmittedForFirstCheck()
        {
            return WorkflowStatus == RefundWorkflowStatus.Draft && Status == RefundStatus.Pending;
        }

        public bool CanBeFirstChecked()
        {
            return WorkflowStatus == RefundWorkflowStatus.SubmittedForFirstCheck;
        }

        public bool CanBeSubmittedForSecondCheck()
        {
            return WorkflowStatus == RefundWorkflowStatus.FirstCheckApproved;
        }

        public bool CanBeSecondChecked()
        {
            return WorkflowStatus == RefundWorkflowStatus.SubmittedForSecondCheck;
        }

        public bool CanBeProcessed()
        {
            return WorkflowStatus == RefundWorkflowStatus.ReadyForProcessing && Status == RefundStatus.Approved;
        }

        public bool IsWorkflowCompleted()
        {
            return WorkflowStatus == RefundWorkflowStatus.Completed;
        }

        public bool IsWorkflowRejected()
        {
            return WorkflowStatus == RefundWorkflowStatus.FirstCheckRejected || 
                   WorkflowStatus == RefundWorkflowStatus.SecondCheckRejected;
        }

        public string GetWorkflowStatusBadgeClass()
        {
            return WorkflowStatus switch
            {
                RefundWorkflowStatus.Draft => "bg-gray-100 text-gray-800",
                RefundWorkflowStatus.SubmittedForFirstCheck => "bg-yellow-100 text-yellow-800",
                RefundWorkflowStatus.FirstCheckApproved => "bg-blue-100 text-blue-800",
                RefundWorkflowStatus.FirstCheckRejected => "bg-red-100 text-red-800",
                RefundWorkflowStatus.SubmittedForSecondCheck => "bg-yellow-100 text-yellow-800",
                RefundWorkflowStatus.SecondCheckApproved => "bg-green-100 text-green-800",
                RefundWorkflowStatus.SecondCheckRejected => "bg-red-100 text-red-800",
                RefundWorkflowStatus.ReadyForProcessing => "bg-green-100 text-green-800",
                RefundWorkflowStatus.Processing => "bg-blue-100 text-blue-800",
                RefundWorkflowStatus.Completed => "bg-green-100 text-green-800",
                RefundWorkflowStatus.Failed => "bg-red-100 text-red-800",
                _ => "bg-gray-100 text-gray-800"
            };
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

    public enum RefundWorkflowStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Submitted for First Check")]
        SubmittedForFirstCheck = 2,

        [Display(Name = "First Check Approved")]
        FirstCheckApproved = 3,

        [Display(Name = "First Check Rejected")]
        FirstCheckRejected = 4,

        [Display(Name = "Submitted for Second Check")]
        SubmittedForSecondCheck = 5,

        [Display(Name = "Second Check Approved")]
        SecondCheckApproved = 6,

        [Display(Name = "Second Check Rejected")]
        SecondCheckRejected = 7,

        [Display(Name = "Ready for Processing")]
        ReadyForProcessing = 8,

        [Display(Name = "Processing")]
        Processing = 9,

        [Display(Name = "Completed")]
        Completed = 10,

        [Display(Name = "Failed")]
        Failed = 11
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

    public static class RefundWorkflowStatusExtensions
    {
        public static string GetDisplayName(this RefundWorkflowStatus status)
        {
            return status switch
            {
                RefundWorkflowStatus.Draft => "Draft",
                RefundWorkflowStatus.SubmittedForFirstCheck => "Submitted for First Check",
                RefundWorkflowStatus.FirstCheckApproved => "First Check Approved",
                RefundWorkflowStatus.FirstCheckRejected => "First Check Rejected",
                RefundWorkflowStatus.SubmittedForSecondCheck => "Submitted for Second Check",
                RefundWorkflowStatus.SecondCheckApproved => "Second Check Approved",
                RefundWorkflowStatus.SecondCheckRejected => "Second Check Rejected",
                RefundWorkflowStatus.ReadyForProcessing => "Ready for Processing",
                RefundWorkflowStatus.Processing => "Processing",
                RefundWorkflowStatus.Completed => "Completed",
                RefundWorkflowStatus.Failed => "Failed",
                _ => "Unknown"
            };
        }
    }
}
