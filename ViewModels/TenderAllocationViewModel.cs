using System.ComponentModel.DataAnnotations;
using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class TenderAllocationViewModel
    {
        public int TenderId { get; set; }
        public string TenderTitle { get; set; } = string.Empty;
        public string TenderIdDisplay { get; set; } = string.Empty;
        public bool IsAllocated { get; set; }
        public bool CanBeAllocated { get; set; }
        public int TotalBids { get; set; }
        public int EligibleBids { get; set; }

        // Current allocation details
        public int? AllocatedBidId { get; set; }
        public string? AllocatedBidderName { get; set; }
        public string? AllocatedBidderCompany { get; set; }
        public decimal? AllocatedBidAmount { get; set; }
        public DateTime? AllocatedAt { get; set; }
        public string? AllocatedByUserName { get; set; }
        public string? AllocationRemarks { get; set; }

        // Available bids for allocation
        public List<TenderBidAllocationOption> AvailableBids { get; set; } = new List<TenderBidAllocationOption>();
    }

    public class TenderBidAllocationOption
    {
        public int Id { get; set; }
        public string BidderName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal BidAmount { get; set; }
        public decimal EmdAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string? Remarks { get; set; }
        public bool IsEligible { get; set; }
        public string EligibilityReason { get; set; } = string.Empty;
    }

    public class TenderAllocationRequestViewModel
    {
        [Required]
        public int TenderId { get; set; }

        [Required]
        [Display(Name = "Select Bid")]
        public int BidId { get; set; }

        [StringLength(500)]
        [Display(Name = "Allocation Remarks")]
        public string? Remarks { get; set; }

        public string TenderTitle { get; set; } = string.Empty;
        public List<TenderBidAllocationOption> AvailableBids { get; set; } = new List<TenderBidAllocationOption>();
    }

    public class TenderDeallocationRequestViewModel
    {
        [Required]
        public int TenderId { get; set; }

        [StringLength(500)]
        [Display(Name = "Deallocation Remarks")]
        public string? Remarks { get; set; }

        public string TenderTitle { get; set; } = string.Empty;
        public string AllocatedBidderName { get; set; } = string.Empty;
        public string AllocatedBidderCompany { get; set; } = string.Empty;
        public decimal AllocatedBidAmount { get; set; }
        public DateTime AllocatedAt { get; set; }
    }
}
