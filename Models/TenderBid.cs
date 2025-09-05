using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class TenderBid
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tender ID")]
        public int TenderId { get; set; }

        [Required(ErrorMessage = "Bidder name is required")]
        [StringLength(200, ErrorMessage = "Bidder name cannot exceed 200 characters")]
        [Display(Name = "Bidder Name")]
        public string BidderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bidder email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Bidder Email")]
        public string BidderEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bidder phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Bidder Phone")]
        public string BidderPhone { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [StringLength(500, ErrorMessage = "Company address cannot exceed 500 characters")]
        [Display(Name = "Company Address")]
        public string? CompanyAddress { get; set; }

        [Required(ErrorMessage = "Bid amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Bid amount must be positive")]
        [Display(Name = "Bid Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BidAmount { get; set; }

        [Display(Name = "EMD Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EmdAmount { get; set; }

        [Display(Name = "Processing Fee")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProcessingFee { get; set; }

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Bid Status")]
        public BidStatus Status { get; set; } = BidStatus.Submitted;

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Display(Name = "Payment Reference")]
        [StringLength(100, ErrorMessage = "Payment reference cannot exceed 100 characters")]
        public string? PaymentReference { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Submitted At")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Tender Tender { get; set; } = null!;
        public virtual ICollection<TenderBidDocument> TenderBidDocuments { get; set; } = new List<TenderBidDocument>();

        // Computed properties
        [NotMapped]
        public string StatusDisplayName => Status.GetDisplayName();

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            BidStatus.Submitted => "bg-blue-100 text-blue-800",
            BidStatus.UnderReview => "bg-yellow-100 text-yellow-800",
            BidStatus.Accepted => "bg-green-100 text-green-800",
            BidStatus.Rejected => "bg-red-100 text-red-800",
            BidStatus.Withdrawn => "bg-gray-100 text-gray-800",
            _ => "bg-gray-100 text-gray-800"
        };

        [NotMapped]
        public string PaymentStatusDisplayName => PaymentStatus.GetDisplayName();

        [NotMapped]
        public string PaymentStatusBadgeClass => PaymentStatus switch
        {
            PaymentStatus.Pending => "bg-yellow-100 text-yellow-800",
            PaymentStatus.Paid => "bg-green-100 text-green-800",
            PaymentStatus.Failed => "bg-red-100 text-red-800",
            PaymentStatus.Refunded => "bg-blue-100 text-blue-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }

    public enum BidStatus
    {
        [Display(Name = "Submitted")]
        Submitted = 1,

        [Display(Name = "Under Review")]
        UnderReview = 2,

        [Display(Name = "Accepted")]
        Accepted = 3,

        [Display(Name = "Rejected")]
        Rejected = 4,

        [Display(Name = "Withdrawn")]
        Withdrawn = 5
    }

    public enum PaymentStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Paid")]
        Paid = 2,

        [Display(Name = "Failed")]
        Failed = 3,

        [Display(Name = "Refunded")]
        Refunded = 4
    }

    public static class BidStatusExtensions
    {
        public static string GetDisplayName(this BidStatus status)
        {
            return status switch
            {
                BidStatus.Submitted => "Submitted",
                BidStatus.UnderReview => "Under Review",
                BidStatus.Accepted => "Accepted",
                BidStatus.Rejected => "Rejected",
                BidStatus.Withdrawn => "Withdrawn",
                _ => "Unknown"
            };
        }
    }

    public static class PaymentStatusExtensions
    {
        public static string GetDisplayName(this PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Pending",
                PaymentStatus.Paid => "Paid",
                PaymentStatus.Failed => "Failed",
                PaymentStatus.Refunded => "Refunded",
                _ => "Unknown"
            };
        }
    }
}
