using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class EMDSDDeposit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string DepositId { get; set; } = string.Empty;

        [Required]
        public int TenderId { get; set; }

        [ForeignKey("TenderId")]
        public virtual Tender Tender { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100)]
        public string BidderName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [StringLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FSSAIBranchName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded

        [Required]
        [StringLength(10)]
        public string Type { get; set; } = "EMD"; // EMD or SD

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Navigation properties
        public virtual ICollection<EMDSDTransaction> Transactions { get; set; } = new List<EMDSDTransaction>();
    }
}
