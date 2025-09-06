using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class EMDSDTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EMDSDDepositId { get; set; }

        [ForeignKey("EMDSDDepositId")]
        public virtual EMDSDDeposit EMDSDDeposit { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TransactionReference { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = string.Empty; // Payment, Refund, Adjustment

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        [Required]
        public DateTime TransactionDate { get; set; }

        [StringLength(100)]
        public string? BankReference { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }
    }
}
