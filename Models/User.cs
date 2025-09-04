using System.ComponentModel.DataAnnotations;

namespace BiddingSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Checker;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpires { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Computed  properties
        public string FullName => $"{FirstName} {LastName}";

        public string RoleDisplayName => Role.GetDisplayName();

        public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    }


}
