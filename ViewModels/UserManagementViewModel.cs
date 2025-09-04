using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class UserManagementViewModel
    {
        public List<User> Users { get; set; } = new();
        public UserRole CurrentUserRole { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public Dictionary<UserRole, int> UsersByRole { get; set; } = new();
    }
}
