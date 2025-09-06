using BiddingSystem.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiddingSystem.ViewModels
{
    public class RefundRequestEditViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Refund ID")]
        public string RefundId { get; set; } = string.Empty;
        
        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; } = string.Empty;
        
        [Display(Name = "Requested At")]
        public DateTime RequestedAt { get; set; }
        
        [Required(ErrorMessage = "Refund type is required")]
        [Display(Name = "Refund Type")]
        public RefundType Type { get; set; }
        
        [Required(ErrorMessage = "Refund reason is required")]
        [Display(Name = "Refund Reason")]
        public RefundReason Reason { get; set; }
        
        [Required(ErrorMessage = "Requested amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Requested Amount")]
        public decimal RequestedAmount { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Approved amount cannot be negative")]
        [Display(Name = "Approved Amount")]
        public decimal? ApprovedAmount { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public RefundStatus Status { get; set; }
        
        [Display(Name = "Refund Reference")]
        [StringLength(100, ErrorMessage = "Refund reference cannot exceed 100 characters")]
        public string? RefundReference { get; set; }
        
        [Display(Name = "Remarks")]
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }
        
        [Display(Name = "Bank Details")]
        [StringLength(1000, ErrorMessage = "Bank details cannot exceed 1000 characters")]
        public string? BankDetails { get; set; }
        
        // Related data
        public TenderBidViewModel? TenderBid { get; set; }
        
        // Dropdown options
        public List<SelectListItem> AvailableTypes { get; set; } = new();
        public List<SelectListItem> AvailableReasons { get; set; } = new();
        public List<SelectListItem> AvailableStatuses { get; set; } = new();
    }
}
