using System.ComponentModel.DataAnnotations;
using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class RefundRequestViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Refund ID")]
        public string RefundId { get; set; } = string.Empty;
        
        [Display(Name = "Tender Bid ID")]
        public int TenderBidId { get; set; }
        
        [Display(Name = "Payment Link ID")]
        public int PaymentLinkId { get; set; }
        
        [Display(Name = "Refund Type")]
        public RefundType Type { get; set; }
        
        [Display(Name = "Requested Amount")]
        public decimal RequestedAmount { get; set; }
        
        [Display(Name = "Approved Amount")]
        public decimal? ApprovedAmount { get; set; }
        
        [Display(Name = "Refund Reason")]
        public RefundReason Reason { get; set; }
        
        [Display(Name = "Status")]
        public RefundStatus Status { get; set; }
        
        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; } = string.Empty;
        
        [Display(Name = "Processed By")]
        public int? ProcessedBy { get; set; }
        
        [Display(Name = "Requested At")]
        public DateTime RequestedAt { get; set; }
        
        [Display(Name = "Processed At")]
        public DateTime? ProcessedAt { get; set; }
        
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        
        [Display(Name = "Bank Details")]
        public string? BankDetails { get; set; }
        
        [Display(Name = "Refund Reference")]
        public string? RefundReference { get; set; }
        
        // Related data
        public TenderBid? TenderBid { get; set; }
        public PaymentLink? PaymentLink { get; set; }
        public User? ProcessedByUser { get; set; }
        public List<RefundTransaction> Transactions { get; set; } = new();
        
        // Checker-Maker Workflow Fields
        [Display(Name = "Created By (Maker)")]
        public int? CreatedBy { get; set; }
        
        [Display(Name = "First Checker")]
        public int? FirstCheckerId { get; set; }
        
        [Display(Name = "First Checker Approval Date")]
        public DateTime? FirstCheckerApprovedAt { get; set; }
        
        [Display(Name = "First Checker Remarks")]
        public string? FirstCheckerRemarks { get; set; }
        
        [Display(Name = "Second Checker")]
        public int? SecondCheckerId { get; set; }
        
        [Display(Name = "Second Checker Approval Date")]
        public DateTime? SecondCheckerApprovedAt { get; set; }
        
        [Display(Name = "Second Checker Remarks")]
        public string? SecondCheckerRemarks { get; set; }
        
        [Display(Name = "Workflow Status")]
        public RefundWorkflowStatus WorkflowStatus { get; set; }
        
        // Related workflow users
        public User? CreatedByUser { get; set; }
        public User? FirstChecker { get; set; }
        public User? SecondChecker { get; set; }
        
        // Display properties
        public string StatusDisplayName => Status.GetDisplayName();
        public string TypeDisplayName => Type.GetDisplayName();
        public string ReasonDisplayName => Reason.GetDisplayName();
        public string WorkflowStatusDisplayName => WorkflowStatus.GetDisplayName();
        public string RequestedAmountDisplay => RequestedAmount.ToString("C", new System.Globalization.CultureInfo("en-IN"));
        public string ApprovedAmountDisplay => ApprovedAmount?.ToString("C", new System.Globalization.CultureInfo("en-IN")) ?? "N/A";
        
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
        
        public string WorkflowStatusBadgeClass => WorkflowStatus switch
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

    public class RefundRequestCreateViewModel
    {
        [Required]
        [Display(Name = "Tender Bid")]
        public int TenderBidId { get; set; }
        
        [Required]
        [Display(Name = "Payment Link")]
        public int PaymentLinkId { get; set; }
        
        [Display(Name = "Created By")]
        public int CreatedBy { get; set; }
        
        [Display(Name = "Approved By")]
        public int? ApprovedBy { get; set; }
        
        [Required]
        [Display(Name = "Refund Type")]
        public RefundType Type { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Requested Amount")]
        public decimal RequestedAmount { get; set; }
        
        [Required]
        [Display(Name = "Refund Reason")]
        public RefundReason Reason { get; set; }
        
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        
        [StringLength(1000, ErrorMessage = "Bank Details cannot exceed 1000 characters")]
        [Display(Name = "Bank Details")]
        public string? BankDetails { get; set; }
        
        // Related data for display
        public TenderBid? TenderBid { get; set; }
        public PaymentLink? PaymentLink { get; set; }
        
        // Available options
        public List<RefundType> AvailableTypes { get; set; } = new();
        public List<RefundReason> AvailableReasons { get; set; } = new();
    }

}
