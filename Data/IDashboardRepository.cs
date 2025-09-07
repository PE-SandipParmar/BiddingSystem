using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface IDashboardRepository
    {
        Task<DashboardStatistics> GetStatisticsAsync(string userId, string userRole, int year);
        Task<List<Tender>> GetRecentTendersAsync(string userId, string userRole, int count);
        Task<List<TenderBid>> GetRecentBidsAsync(string userId, string userRole, int count);
        Task<List<RefundRequest>> GetPendingRefundsAsync(string userId, string userRole, int count);
        Task<List<Tender>> GetActiveTendersAsync(string userId, string userRole, int count);
        Task<List<Tender>> GetExpiringTendersAsync(string userId, string userRole, int count);
        Task<List<int>> GetAvailableYearsAsync();
        Task<List<RecentActivity>> GetRecentActivityAsync(string userId, string userRole);
        Task<Dictionary<string, int>> GetTenderStatusDistributionAsync(string userId, string userRole);
        Task<Dictionary<string, int>> GetBidStatusDistributionAsync(string userId, string userRole);
        Task<MonthlyTrends> GetMonthlyTrendsAsync(string userId, string userRole, int year);
        Task<object> GetRefundDebugInfoAsync();
    }
}
