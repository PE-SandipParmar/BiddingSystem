using System.ComponentModel.DataAnnotations;

namespace BiddingSystem.Models
{
    public class PaymentRequest
    {
        [Required]
        [Display(Name = "Customer Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount (INR)")]
        public int Amount { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
