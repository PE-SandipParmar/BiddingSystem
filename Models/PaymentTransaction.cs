using System.ComponentModel.DataAnnotations;

namespace BiddingSystem.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentLinkId { get; set; }

        [Required]
        public int TenderId { get; set; }

        public int? TenderBidId { get; set; }

        [StringLength(100)]
        public string? RazorpayPaymentId { get; set; }

        [StringLength(100)]
        public string? RazorpayOrderId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Success, Failed, Pending

        [StringLength(50)]
        public string? PaymentMethod { get; set; } // card, upi, netbanking, wallet

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(4)]
        public string? CardLast4 { get; set; }

        [StringLength(100)]
        public string? UPIId { get; set; }

        [StringLength(50)]
        public string? WalletName { get; set; }

        [StringLength(100)]
        public string? ErrorCode { get; set; }

        [StringLength(500)]
        public string? ErrorDescription { get; set; }

        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WebhookEvent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } // payment.captured, payment.failed, etc.

        [StringLength(100)]
        public string? RazorpayPaymentId { get; set; }

        [StringLength(100)]
        public string? RazorpayOrderId { get; set; }

        [StringLength(50)]
        public string? PaymentLinkId { get; set; }

        public decimal? Amount { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public bool ProcessedSuccessfully { get; set; } = false;

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime ReceivedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }
    }
}
