using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class RefundTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Refund Request ID")]
        public int RefundRequestId { get; set; }

        [ForeignKey("RefundRequestId")]
        public virtual RefundRequest RefundRequest { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [Display(Name = "Transaction Reference")]
        public string TransactionReference { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Status")]
        public RefundTransactionStatus Status { get; set; } = RefundTransactionStatus.Pending;

        [Required]
        [Display(Name = "Processed At")]
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        [Display(Name = "Bank Response")]
        public string? BankResponse { get; set; }

        [StringLength(500)]
        [Display(Name = "Failure Reason")]
        public string? FailureReason { get; set; }

        [Required]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        // Computed properties
        [NotMapped]
        public string StatusDisplayName => Status.GetDisplayName();

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            RefundTransactionStatus.Pending => "bg-yellow-100 text-yellow-800",
            RefundTransactionStatus.Processing => "bg-blue-100 text-blue-800",
            RefundTransactionStatus.Success => "bg-green-100 text-green-800",
            RefundTransactionStatus.Failed => "bg-red-100 text-red-800",
            RefundTransactionStatus.Cancelled => "bg-gray-100 text-gray-800",
            _ => "bg-gray-100 text-gray-800"
        };

        [NotMapped]
        public string AmountDisplay => Amount.ToString("C", new System.Globalization.CultureInfo("en-IN"));

        // Helper methods
        public static string GenerateTransactionReference()
        {
            return $"RT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        public bool IsSuccessful()
        {
            return Status == RefundTransactionStatus.Success;
        }

        public bool IsFailed()
        {
            return Status == RefundTransactionStatus.Failed;
        }

        public bool IsPending()
        {
            return Status == RefundTransactionStatus.Pending || Status == RefundTransactionStatus.Processing;
        }
    }

    public enum RefundTransactionStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Processing")]
        Processing = 2,

        [Display(Name = "Success")]
        Success = 3,

        [Display(Name = "Failed")]
        Failed = 4,

        [Display(Name = "Cancelled")]
        Cancelled = 5
    }

    public static class RefundTransactionStatusExtensions
    {
        public static string GetDisplayName(this RefundTransactionStatus status)
        {
            return status switch
            {
                RefundTransactionStatus.Pending => "Pending",
                RefundTransactionStatus.Processing => "Processing",
                RefundTransactionStatus.Success => "Success",
                RefundTransactionStatus.Failed => "Failed",
                RefundTransactionStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}
