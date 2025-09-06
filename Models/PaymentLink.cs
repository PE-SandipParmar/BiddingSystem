using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace BiddingSystem.Models
{
    public class PaymentLink
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Link ID cannot exceed 50 characters")]
        [Display(Name = "Link ID")]
        public string LinkId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tender ID")]
        public int TenderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Type")]
        public PaymentType PaymentType { get; set; } = PaymentType.EMD;

        [Required]
        [StringLength(500, ErrorMessage = "Payment URL cannot exceed 500 characters")]
        [Display(Name = "Payment URL")]
        public string PaymentUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(64, ErrorMessage = "Security token cannot exceed 64 characters")]
        [Display(Name = "Security Token")]
        public string SecurityToken { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Status")]
        public PaymentLinkStatus Status { get; set; } = PaymentLinkStatus.Active;

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Used Date")]
        public DateTime? UsedDate { get; set; }

        [Display(Name = "Used By")]
        public int? UsedBy { get; set; }

        [StringLength(100, ErrorMessage = "Transaction ID cannot exceed 100 characters")]
        [Display(Name = "Transaction ID")]
        public string? TransactionId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Created By")]
        public int CreatedBy { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Tender Tender { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual User? UsedByUser { get; set; }

        // Computed properties
        [NotMapped]
        public string StatusDisplayName => Status.GetDisplayName();

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            PaymentLinkStatus.Active => "bg-green-100 text-green-800",
            PaymentLinkStatus.Used => "bg-blue-100 text-blue-800",
            PaymentLinkStatus.Expired => "bg-red-100 text-red-800",
            PaymentLinkStatus.Cancelled => "bg-gray-100 text-gray-800",
            _ => "bg-gray-100 text-gray-800"
        };

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;

        [NotMapped]
        public bool IsUsable => Status == PaymentLinkStatus.Active && !IsExpired;

        [NotMapped]
        public string AmountDisplay => Amount.ToString("C", new System.Globalization.CultureInfo("en-IN"));

        // Security methods
        public static string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public static string GenerateLinkId()
        {
            return $"PL-{DateTime.UtcNow:yyyy-MMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        public bool ValidateSecurityToken(string token)
        {
            return SecurityToken == token && IsUsable;
        }
    }

    public enum PaymentType
    {
        [Display(Name = "EMD (Earnest Money Deposit)")]
        EMD = 1,

        [Display(Name = "SD (Security Deposit)")]
        SD = 2,

        [Display(Name = "Processing Fee")]
        ProcessingFee = 3,

        [Display(Name = "Other")]
        Other = 4
    }

    public enum PaymentLinkStatus
    {
        [Display(Name = "Active")]
        Active = 1,

        [Display(Name = "Used")]
        Used = 2,

        [Display(Name = "Expired")]
        Expired = 3,

        [Display(Name = "Cancelled")]
        Cancelled = 4
    }

    public static class PaymentLinkStatusExtensions
    {
        public static string GetDisplayName(this PaymentLinkStatus status)
        {
            return status switch
            {
                PaymentLinkStatus.Active => "Active",
                PaymentLinkStatus.Used => "Used",
                PaymentLinkStatus.Expired => "Expired",
                PaymentLinkStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }

    public static class PaymentTypeExtensions
    {
        public static string GetDisplayName(this PaymentType type)
        {
            return type switch
            {
                PaymentType.EMD => "EMD (Earnest Money Deposit)",
                PaymentType.SD => "SD (Security Deposit)",
                PaymentType.ProcessingFee => "Processing Fee",
                PaymentType.Other => "Other",
                _ => "Unknown"
            };
        }
    }
}
