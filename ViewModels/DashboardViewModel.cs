using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardStatistics Statistics { get; set; } = new();
        public List<Tender> RecentTenders { get; set; } = new();
        public List<TenderBid> RecentBids { get; set; } = new();
        public List<RefundRequest> PendingRefunds { get; set; } = new();
        public List<Tender> ActiveTenders { get; set; } = new();
        public List<Tender> ExpiringTenders { get; set; } = new();
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new();
        public RefundStatistics RefundStatistics { get; set; }
    }
}
