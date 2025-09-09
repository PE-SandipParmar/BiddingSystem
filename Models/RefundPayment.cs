using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BiddingSystem.Models
{
    public class RefundPayment
    {
        [Key]
        public int Id { get; set; }
        public int TenderBidId { get; set; }
        public int TenderId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        public string ReasonForRefund { get; set; } = "Tender awarded to another bidder";
        public string RefundStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

        public int InitiatedBy { get; set; }
        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;

        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? CheckerRemarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TenderBidId")]
        public virtual TenderBid? TenderBid { get; set; }

        [ForeignKey("TenderId")]
        public virtual Tender? Tender { get; set; }
    }
}
