using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using System.Data.SqlClient;
using System.Data;

namespace BiddingSystem.Data
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IDashboardRepository repository, ILogger<DashboardService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<DashboardData> GetDashboardDataAsync(string userId, string userRole, int year)
        {
            try
            {
                var statistics = await GetStatisticsAsync(userId, userRole, year);
                var recentTenders = await GetRecentTendersAsync(userId, userRole, 5);
                var recentBids = await GetRecentBidsAsync(userId, userRole, 5);
                var pendingRefunds = await GetPendingRefundsAsync(userId, userRole, 5);
                var activeTenders = await GetActiveTendersAsync(userId, userRole, 5);
                var expiringTenders = await GetExpiringTendersAsync(userId, userRole, 5);

                return new DashboardData
                {
                    Statistics = statistics,
                    RecentTenders = recentTenders,
                    RecentBids = recentBids,
                    PendingRefunds = pendingRefunds,
                    ActiveTenders = activeTenders,
                    ExpiringTenders = expiringTenders
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data for user {UserId} and year {Year}", userId, year);
                throw;
            }
        }

        public async Task<DashboardStatistics> GetStatisticsAsync(string userId, string userRole, int year)
        {
            try
            {
                return await _repository.GetStatisticsAsync(userId, userRole, year);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for user {UserId} and year {Year}", userId, year);
                throw;
            }
        }

        public async Task<List<Tender>> GetRecentTendersAsync(string userId, string userRole, int count = 5)
        {
            try
            {
                return await _repository.GetRecentTendersAsync(userId, userRole, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent tenders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<TenderBid>> GetRecentBidsAsync(string userId, string userRole, int count = 5)
        {
            try
            {
                return await _repository.GetRecentBidsAsync(userId, userRole, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent bids for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<RefundRequest>> GetPendingRefundsAsync(string userId, string userRole, int count = 5)
        {
            try
            {
                return await _repository.GetPendingRefundsAsync(userId, userRole, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending refunds for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Tender>> GetActiveTendersAsync(string userId, string userRole, int count = 5)
        {
            try
            {
                return await _repository.GetActiveTendersAsync(userId, userRole, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active tenders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Tender>> GetExpiringTendersAsync(string userId, string userRole, int count = 5)
        {
            try
            {
                return await _repository.GetExpiringTendersAsync(userId, userRole, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring tenders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            try
            {
                return await _repository.GetAvailableYearsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available years");
                throw;
            }
        }

        public async Task<List<RecentActivity>> GetRecentActivityAsync(string userId, string userRole)
        {
            try
            {
                return await _repository.GetRecentActivityAsync(userId, userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetTenderStatusDistributionAsync(string userId, string userRole)
        {
            try
            {
                return await _repository.GetTenderStatusDistributionAsync(userId, userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender status distribution for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetBidStatusDistributionAsync(string userId, string userRole)
        {
            try
            {
                return await _repository.GetBidStatusDistributionAsync(userId, userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bid status distribution for user {UserId}", userId);
                throw;
            }
        }

        public async Task<MonthlyTrends> GetMonthlyTrendsAsync(string userId, string userRole, int year)
        {
            try
            {
                return await _repository.GetMonthlyTrendsAsync(userId, userRole, year);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly trends for user {UserId} and year {Year}", userId, year);
                throw;
            }
        }

        public async Task<object> GetRefundDebugInfoAsync()
        {
            try
            {
                return await _repository.GetRefundDebugInfoAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund debug info");
                throw;
            }
        }
    }
}
