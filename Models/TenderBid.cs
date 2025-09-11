using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class TenderBid
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TenderId { get; set; }

        [ForeignKey("TenderId")]
        [Range(1, int.MaxValue, ErrorMessage = "Tender is required.")]
        public virtual Tender Tender { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [Display(Name = "Bidder Name")]
        public string BidderName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Bidder Email")]
        public string BidderEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Bidder Phone")]
        public string BidderPhone { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        
        [StringLength(500)]
        [Display(Name = "Company Address")]
        public string CompanyAddress { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Bid Amount")]
        [Range(1, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
        public decimal? BidAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "EMD Amount")]
        [Range(1, double.MaxValue, ErrorMessage = "EMD amount must be greater than 0")]
        public decimal? EmdAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Processing Fee")]
        [Range(1, double.MaxValue, ErrorMessage = "Processing Fee must be greater than 0")]
        public decimal? ProcessingFee { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Submitted"; // Submitted, Under Review, Accepted, Rejected, Withdrawn

        [Required]
        [StringLength(20)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded

        [StringLength(100)]
        [Display(Name = "Payment Reference")]
        public string? PaymentReference { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Submitted At")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Allocation properties
        [Display(Name = "Is Allocated")]
        public bool IsAllocated { get; set; } = false;

        [Display(Name = "Allocation Date")]
        public DateTime? AllocationDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Allocation Remarks")]
        public string? AllocationRemarks { get; set; }

        // Navigation properties
        public virtual ICollection<TenderBidDocument> Documents { get; set; } = new List<TenderBidDocument>();
    }
}