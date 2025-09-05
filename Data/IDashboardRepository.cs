using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface IDashboardRepository
    {
        Task<DashboardStatistics> GetDashboardStatisticsAsync(int financialYear);
        Task<IEnumerable<MonthlyTrendData>> GetMonthlyExpenditureTrendAsync(int financialYear, int months);
        Task<IEnumerable<LeasePaymentStatusData>> GetLeasePaymentStatusAsync(int financialYear);
        Task<IEnumerable<DepartmentLeaseDistribution>> GetDepartmentWiseLeaseDistributionAsync(int financialYear);
        Task<IEnumerable<TopVendorData>> GetTopVendorsByPaymentAsync(int topN, int financialYear);
        Task<IEnumerable<LeaseExpiryAlert>> GetLeaseExpiryAlertsAsync(int daysAhead);
        Task<IEnumerable<PaymentSummary>> GetPaymentSummaryByTypeAsync(int financialYear);
        Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(int topN);
    }
}
