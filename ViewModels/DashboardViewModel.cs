using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class DashboardViewModel
    {
        public User CurrentUser { get; set; } = new();
        public int TotalUsers { get; set; }
        public int RecentRegistrations { get; set; }
        public List<User> RecentUsers { get; set; } = new();
        public Dictionary<UserRole, int> UsersByRole { get; set; } = new();
        public List<string> RecentActivities { get; set; } = new();
    }
}
