using BiddingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace BiddingSystem.ViewModels
{
    public class RefundTransactionViewModel
    {
        public int Id { get; set; }
        public int RefundRequestId { get; set; }
        
        [Display(Name = "Transaction Reference")]
        public string TransactionReference { get; set; } = string.Empty;
        
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
        
        [Display(Name = "Status")]
        public RefundTransactionStatus Status { get; set; }
        
        [Display(Name = "Processed At")]
        public DateTime ProcessedAt { get; set; }
        
        [Display(Name = "Bank Response")]
        public string? BankResponse { get; set; }
        
        [Display(Name = "Failure Reason")]
        public string? FailureReason { get; set; }
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }
        
        // Display properties
        public string AmountDisplay => Amount.ToString("C");
        
        public string StatusDisplayName => Status switch
        {
            RefundTransactionStatus.Pending => "Pending",
            RefundTransactionStatus.Processing => "Processing",
            RefundTransactionStatus.Success => "Success",
            RefundTransactionStatus.Failed => "Failed",
            RefundTransactionStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
        
        public string StatusBadgeClass => Status switch
        {
            RefundTransactionStatus.Pending => "bg-yellow-100 text-yellow-800",
            RefundTransactionStatus.Processing => "bg-blue-100 text-blue-800",
            RefundTransactionStatus.Success => "bg-green-100 text-green-800",
            RefundTransactionStatus.Failed => "bg-red-100 text-red-800",
            RefundTransactionStatus.Cancelled => "bg-gray-100 text-gray-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }
}
