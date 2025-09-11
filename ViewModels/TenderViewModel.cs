using System.ComponentModel.DataAnnotations;
using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class TenderViewModel
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
        public DateTime PublishDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "EMD amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "EMD amount must be greater than 0")]
        [Display(Name = "EMD Amount")]
        public decimal EmdAmount { get; set; }

        [Required(ErrorMessage = "SD amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "SD amount must be greater than 0")]
        [Display(Name = "SD Amount")]
        public decimal SdAmount { get; set; }

        [Required(ErrorMessage = "Processing fee is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Processing fee must be greater than 0")]
        [Display(Name = "Processing Fee")]
        public decimal ProcessingFee { get; set; }

        [Required(ErrorMessage = "Estimated value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Estimated value must be greater than 0")]
        [Display(Name = "Estimated Value")]
        public decimal EstimatedValue { get; set; }

        [Required(ErrorMessage = "Last date to pay EMD is required")]
        [Display(Name = "Last Date to Pay EMD")]
        public DateTime LastDateEmd { get; set; } = DateTime.Now.AddDays(30);

        [Display(Name = "Tender Closing Date")]
        public DateTime? TenderClosingDate { get; set; }

        [Display(Name = "Tender Opening Date")]
        public DateTime? TenderOpeningDate { get; set; }

        [Display(Name = "Status")]
        public TenderStatus Status { get; set; } = TenderStatus.Draft;

        // Additional properties expected by the controller
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedByUserName { get; set; } = string.Empty;

        // File upload properties
        [Display(Name = "Tender Documents")]
        public List<IFormFile>? TenderDocuments { get; set; }

        // Navigation properties
        public List<TenderDocument> Documents { get; set; } = new List<TenderDocument>();
        public List<TenderBid> Bids { get; set; } = new List<TenderBid>();
        public List<TenderDocument> ExistingDocuments { get; set; } = new List<TenderDocument>();
        public List<TenderBid> TenderBids { get; set; } = new List<TenderBid>();

        // Computed properties
        public int DocumentCount => Documents?.Count ?? 0;
        public int BidCount => Bids?.Count ?? 0;
        public string StatusDisplayName => Status.GetDisplayName();
        public string StatusBadgeClass => Status switch
        {
            TenderStatus.Draft => "bg-gray-100 text-gray-800",
            TenderStatus.Published => "bg-green-100 text-green-800",
            TenderStatus.Closed => "bg-red-100 text-red-800",
            TenderStatus.Cancelled => "bg-yellow-100 text-yellow-800",
            _ => "bg-gray-100 text-gray-800"
        };

        public bool IsEditable => Status == TenderStatus.Draft;
        public bool IsPublished => Status == TenderStatus.Published;
        public bool IsClosed => Status == TenderStatus.Closed;

        // Allocation properties
        public int? AllocatedBidId { get; set; }
        public DateTime? AllocatedAt { get; set; }
        public int? AllocatedBy { get; set; }
        public string? AllocationRemarks { get; set; }
        public TenderBid? AllocatedBid { get; set; }

        public bool IsAllocated => AllocatedBidId.HasValue;

        public bool CanBeAllocated => Status == TenderStatus.Published && !IsAllocated && Bids.Count >= 1;
    }

    public class TenderListViewModel
    {
        public List<TenderViewModel> Tenders { get; set; } = new List<TenderViewModel>();
        public string? SearchTerm { get; set; }
        public TenderStatus? StatusFilter { get; set; }
        public string? DepartmentFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public List<string> Departments { get; set; } = new List<string>();
        
        // Additional properties expected by the controller
        public int Page { get; set; } = 1;
        public TenderStatus? Status { get; set; }
        public string? Department { get; set; }
        public Dictionary<TenderStatus, int> StatusCounts { get; set; } = new Dictionary<TenderStatus, int>();
    }

    public class TenderDashboardViewModel
    {
        public int TotalTenders { get; set; }
        public int PublishedTenders { get; set; }
        public int ClosedTenders { get; set; }
        public int DraftTenders { get; set; }
        public int TotalBids { get; set; }
        public decimal TotalEstimatedValue { get; set; }
        public List<TenderViewModel> RecentTenders { get; set; } = new List<TenderViewModel>();
        public List<TenderBid> RecentBids { get; set; } = new List<TenderBid>();
        public List<KeyValuePair<string, int>> TendersByDepartment { get; set; } = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, int>> TendersByStatus { get; set; } = new List<KeyValuePair<string, int>>();
        
        // Additional properties expected by the controller and views
        public int PaidBids { get; set; }
        public decimal TotalEmdCollected { get; set; }
        public decimal TotalProcessingFeesCollected { get; set; }
        public List<TenderViewModel> ExpiringTenders { get; set; } = new List<TenderViewModel>();
        public List<TenderViewModel> UpcomingTenders { get; set; } = new List<TenderViewModel>();
        public Dictionary<TenderStatus, int> TenderCountByStatus { get; set; } = new Dictionary<TenderStatus, int>();
    }

    public class TenderDocumentViewModel
    {
        public int Id { get; set; }
        public int TenderId { get; set; }
        
        [Required(ErrorMessage = "Document name is required")]
        [StringLength(200, ErrorMessage = "Document name cannot exceed 200 characters")]
        [Display(Name = "Document Name")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required")]
        [Display(Name = "Document Type")]
        public TenderDocumentType DocumentType { get; set; } = TenderDocumentType.General;

        [Required(ErrorMessage = "File is required")]
        [Display(Name = "File")]
        public IFormFile? File { get; set; }

        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsRequired { get; set; } = false;
    }

    public class TenderBidViewModel
    {
        public int Id { get; set; }
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
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Bidder Phone")]
        public string BidderPhone { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [StringLength(500, ErrorMessage = "Company address cannot exceed 500 characters")]
        [Display(Name = "Company Address")]
        public string? CompanyAddress { get; set; }

        [Required(ErrorMessage = "Bid amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
        [Display(Name = "Bid Amount")]
        public decimal BidAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "EMD amount cannot be negative")]
        [Display(Name = "EMD Amount")]
        public decimal EmdAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Processing fee cannot be negative")]
        [Display(Name = "Processing Fee")]
        public decimal ProcessingFee { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Bid Status")]
        public string Status { get; set; } = "Submitted";

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending";

        [Display(Name = "Payment Reference")]
        [StringLength(100, ErrorMessage = "Payment reference cannot exceed 100 characters")]
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

        [Display(Name = "Tender Name")]
        public string TenderName { get; set; } = string.Empty;

        [Display(Name = "Tender Status")]
        public string TenderStatus { get; set; } = string.Empty;

        // File upload properties
        [Display(Name = "Bid Documents")]
        public List<IFormFile>? BidDocuments { get; set; }

        // Navigation properties
        public Tender? Tender { get; set; }
        public List<TenderBidDocument> ExistingDocuments { get; set; } = new List<TenderBidDocument>();
        public List<TenderBidDocument> Documents { get; set; } = new List<TenderBidDocument>();

        // Computed properties
        public string StatusDisplayName => Status;
        public string StatusBadgeClass => Status switch
        {
            "Submitted" => "bg-blue-100 text-blue-800",
            "Under Review" => "bg-yellow-100 text-yellow-800",
            "Accepted" => "bg-green-100 text-green-800",
            "Rejected" => "bg-red-100 text-red-800",
            "Withdrawn" => "bg-gray-100 text-gray-800",
            _ => "bg-gray-100 text-gray-800"
        };

        public string PaymentStatusDisplayName => PaymentStatus;
        public string PaymentStatusBadgeClass => PaymentStatus switch
        {
            "Pending" => "bg-yellow-100 text-yellow-800",
            "Paid" => "bg-green-100 text-green-800",
            "Failed" => "bg-red-100 text-red-800",
            "Refunded" => "bg-blue-100 text-blue-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }

    public class TenderBidListViewModel
    {
        public List<TenderBidViewModel> Bids { get; set; } = new List<TenderBidViewModel>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public List<string> StatusOptions { get; set; } = new List<string> { "Submitted", "Under Review", "Accepted", "Rejected", "Withdrawn" };
        public List<string> PaymentStatusOptions { get; set; } = new List<string> { "Pending", "Paid", "Failed", "Refunded" };
    }

    public class TenderBidCreateViewModel
    {
        [Required]
        [Display(Name = "Tender")]
        public int TenderId { get; set; }

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

        [Required]
        [StringLength(500)]
        [Display(Name = "Company Address")]
        public string CompanyAddress { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Bid Amount")]
        [DataType(DataType.Currency)]
        public decimal BidAmount { get; set; }

        [Required]
        [Display(Name = "EMD Amount")]
        [DataType(DataType.Currency)]
        public decimal EmdAmount { get; set; }

        [Required]
        [Display(Name = "Processing Fee")]
        [DataType(DataType.Currency)]
        public decimal ProcessingFee { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<Tender> AvailableTenders { get; set; } = new List<Tender>();
    }

    public class TenderBidEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tender")]
        public int TenderId { get; set; }

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

        [Required]
        [StringLength(500)]
        [Display(Name = "Company Address")]
        public string CompanyAddress { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Bid Amount")]
        [DataType(DataType.Currency)]
        public decimal BidAmount { get; set; }

        [Required]
        [Display(Name = "EMD Amount")]
        [DataType(DataType.Currency)]
        public decimal EmdAmount { get; set; }

        [Required]
        [Display(Name = "Processing Fee")]
        [DataType(DataType.Currency)]
        public decimal ProcessingFee { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = string.Empty;

        [Display(Name = "Payment Reference")]
        public string? PaymentReference { get; set; }

        [Display(Name = "Payment Date")]
        [DataType(DataType.DateTime)]
        public DateTime? PaymentDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<Tender> AvailableTenders { get; set; } = new List<Tender>();
        public List<string> StatusOptions { get; set; } = new List<string> { "Submitted", "Under Review", "Accepted", "Rejected", "Withdrawn" };
        public List<string> PaymentStatusOptions { get; set; } = new List<string> { "Pending", "Paid", "Failed", "Refunded" };
    }
}
