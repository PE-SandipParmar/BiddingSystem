using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class Tender
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tender ID is required")]
        [Display(Name = "Tender ID")]
        public string TenderId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tender title is required")]
        [StringLength(200, ErrorMessage = "Tender title cannot exceed 200 characters")]
        [Display(Name = "Tender Title")]
        public string TenderTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Publish date is required")]
        [Display(Name = "Publish Date")]
        public DateTime PublishDate { get; set; }

        [Required(ErrorMessage = "EMD amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "EMD amount must be greater than 0")]
        [Display(Name = "EMD Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EmdAmount { get; set; }

        [Required(ErrorMessage = "SD amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "SD amount must be greater than 0")]
        [Display(Name = "SD Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SdAmount { get; set; }

        [Required(ErrorMessage = "Processing fee is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Processing fee must be greater than 0")]
        [Display(Name = "Processing Fee")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProcessingFee { get; set; }

        [Required(ErrorMessage = "Estimated value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Estimated value must be greater than 0")]
        [Display(Name = "Estimated Value")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedValue { get; set; }

        [Required(ErrorMessage = "Last date to pay EMD is required")]
        [Display(Name = "Last Date to Pay EMD")]
        public DateTime LastDateEmd { get; set; }

        [Display(Name = "Tender Closing Date")]
        public DateTime? TenderClosingDate { get; set; }

        [Display(Name = "Tender Allocation Date")]
        public DateTime? TenderOpeningDate { get; set; }

        [Display(Name = "Status")]
        public TenderStatus Status { get; set; } = TenderStatus.Draft;

        [Display(Name = "Created By")]
        public int CreatedBy { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Published At")]
        public DateTime? PublishedAt { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Allocation properties
        [Display(Name = "Allocated Bid ID")]
        public int? AllocatedBidId { get; set; }

        [Display(Name = "Allocated At")]
        public DateTime? AllocatedAt { get; set; }

        [Display(Name = "Allocated By")]
        public int? AllocatedBy { get; set; }

        [StringLength(500)]
        [Display(Name = "Allocation Remarks")]
        public string? AllocationRemarks { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual User? AllocatedByUser { get; set; }
        public virtual TenderBid? AllocatedBid { get; set; }
        public virtual ICollection<TenderDocument> TenderDocuments { get; set; } = new List<TenderDocument>();
        public virtual ICollection<TenderBid> TenderBids { get; set; } = new List<TenderBid>();

        // Computed properties
        [NotMapped]
        public string StatusDisplayName => Status.GetDisplayName();

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            TenderStatus.Draft => "bg-gray-100 text-gray-800",
            TenderStatus.Published => "bg-green-100 text-green-800",
            TenderStatus.Closed => "bg-red-100 text-red-800",
            TenderStatus.Cancelled => "bg-yellow-100 text-yellow-800",
            _ => "bg-gray-100 text-gray-800"
        };

        [NotMapped]
        public bool IsEditable => Status == TenderStatus.Draft;

        [NotMapped]
        public bool IsPublished => Status == TenderStatus.Published;

        [NotMapped]
        public bool IsClosed => Status == TenderStatus.Closed;

        [NotMapped]
        public bool IsAllocated => AllocatedBidId.HasValue;

        [NotMapped]
        public bool CanBeAllocated => Status == TenderStatus.Published && !IsAllocated && TenderBids.Count >= 5;
    }

    public enum TenderStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Published")]
        Published = 2,

        [Display(Name = "Closed")]
        Closed = 3,

        [Display(Name = "Cancelled")]
        Cancelled = 4
    }

    public static class TenderStatusExtensions
    {
        public static string GetDisplayName(this TenderStatus status)
        {
            return status switch
            {
                TenderStatus.Draft => "Draft",
                TenderStatus.Published => "Published",
                TenderStatus.Closed => "Closed",
                TenderStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}
