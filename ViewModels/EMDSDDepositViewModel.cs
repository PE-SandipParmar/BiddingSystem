using System.ComponentModel.DataAnnotations;
using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class EMDSDDepositViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Deposit ID")]
        public string DepositId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tender")]
        public int TenderId { get; set; }

        [Display(Name = "Tender Name")]
        public string TenderName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tender Bid")]
        public int TenderBidId { get; set; }

        [Required]
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Bidder Name")]
        public string BidderName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "FSSAI Branch Name")]
        public string FSSAIBranchName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; } = "EMD";

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Tender? Tender { get; set; }
        public List<EMDSDTransaction> Transactions { get; set; } = new List<EMDSDTransaction>();
    }

    public class EMDSDDepositListViewModel
    {
        public List<EMDSDDepositViewModel> Deposits { get; set; } = new List<EMDSDDepositViewModel>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public List<string> StatusOptions { get; set; } = new List<string> { "Pending", "Paid", "Failed", "Refunded" };
        public List<string> TypeOptions { get; set; } = new List<string> { "EMD", "SD" };
    }

    public class EMDSDDepositCreateViewModel
    {
        [Required]
        [Display(Name = "Tender")]
        public int TenderId { get; set; }

        [Required]
        [Display(Name = "Tender Bid")]
        public int TenderBidId { get; set; }

        [Required]
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Bidder Name")]
        public string BidderName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "FSSAI Branch Name")]
        public string FSSAIBranchName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; } = "EMD";

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<Tender> AvailableTenders { get; set; } = new List<Tender>();
        public List<string> TypeOptions { get; set; } = new List<string> { "EMD", "SD" };
    }

    public class EMDSDDepositEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Deposit ID")]
        public string DepositId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tender")]
        public int TenderId { get; set; }

        [Required]
        [Display(Name = "Tender Bid")]
        public int TenderBidId { get; set; }

        [Required]
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Bidder Name")]
        public string BidderName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "FSSAI Branch Name")]
        public string FSSAIBranchName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<Tender> AvailableTenders { get; set; } = new List<Tender>();
        public List<string> StatusOptions { get; set; } = new List<string> { "Pending", "Paid", "Failed", "Refunded" };
        public List<string> TypeOptions { get; set; } = new List<string> { "EMD", "SD" };
    }
}
