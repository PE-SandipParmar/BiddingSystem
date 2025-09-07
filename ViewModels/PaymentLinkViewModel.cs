using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class PaymentLinkViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tender selection is required")]
        [Display(Name = "Tender")]
        public int TenderId { get; set; }

        [Display(Name = "Tender Bid")]
        public int? TenderBidId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment type is required")]
        [Display(Name = "Payment Type")]
        public PaymentType PaymentType { get; set; } = PaymentType.EMD;

        [Required(ErrorMessage = "Expiry date is required")]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Display properties
        public string? TenderTitle { get; set; }
        public string? TenderIdString { get; set; }
        public string? BidderName { get; set; }
        public string? CompanyName { get; set; }
        public string? LinkId { get; set; }
        public string? PaymentUrl { get; set; }
        public PaymentLinkStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UsedDate { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? UsedByUserName { get; set; }
        public string? TransactionId { get; set; }
    }

    public class PaymentLinkListViewModel
    {
        public List<PaymentLinkViewModel> PaymentLinks { get; set; } = new List<PaymentLinkViewModel>();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SearchTerm { get; set; } = string.Empty;
        public PaymentLinkStatus? Status { get; set; }
        public PaymentType? PaymentType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentTypeOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TenderOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TenderBidOptions { get; set; } = new List<SelectListItem>();
    }

    public class PaymentLinkDetailsViewModel
    {
        public int Id { get; set; }
        public string LinkId { get; set; } = string.Empty;
        public string TenderTitle { get; set; } = string.Empty;
        public string TenderId { get; set; } = string.Empty;
        public string TenderIdString { get; set; } = string.Empty;
        public string? BidderName { get; set; }
        public string? CompanyName { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public PaymentLinkStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime? UsedDate { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string? UsedByUserName { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public bool IsExpired { get; set; }
        public bool IsUsable { get; set; }
        public string StatusDisplayName { get; set; } = string.Empty;
        public string StatusBadgeClass { get; set; } = string.Empty;
        public string AmountDisplay { get; set; } = string.Empty;
    }
}
