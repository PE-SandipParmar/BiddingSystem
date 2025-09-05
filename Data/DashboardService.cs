using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(int financialYear)
        {
            var dashboardData = new DashboardViewModel();

            // Fetch all data in parallel for better performance
            var statisticsTask = _dashboardRepository.GetDashboardStatisticsAsync(financialYear);
            var monthlyTrendTask = _dashboardRepository.GetMonthlyExpenditureTrendAsync(financialYear, 6);
            var paymentStatusTask = _dashboardRepository.GetLeasePaymentStatusAsync(financialYear);
            var departmentDistTask = _dashboardRepository.GetDepartmentWiseLeaseDistributionAsync(financialYear);
            var topVendorsTask = _dashboardRepository.GetTopVendorsByPaymentAsync(5, financialYear);
            var expiryAlertsTask = _dashboardRepository.GetLeaseExpiryAlertsAsync(30);
            var paymentSummaryTask = _dashboardRepository.GetPaymentSummaryByTypeAsync(financialYear);
            var recentActivitiesTask = _dashboardRepository.GetRecentActivitiesAsync(10);

            await Task.WhenAll(
                statisticsTask,
                monthlyTrendTask,
                paymentStatusTask,
                departmentDistTask,
                topVendorsTask,
                expiryAlertsTask,
                paymentSummaryTask,
                recentActivitiesTask
            );

          
            return dashboardData;
        }

        public async Task<DashboardStatistics> GetStatisticsAsync(int financialYear)
        {
            return await _dashboardRepository.GetDashboardStatisticsAsync(financialYear);
        }

        public async Task<object> GetChartDataAsync(string chartType, int financialYear)
        {
            return chartType.ToLower() switch
            {
                "monthlytrend" => await _dashboardRepository.GetMonthlyExpenditureTrendAsync(financialYear, 6),
                "paymentstatus" => await _dashboardRepository.GetLeasePaymentStatusAsync(financialYear),
                "department" => await _dashboardRepository.GetDepartmentWiseLeaseDistributionAsync(financialYear),
                "paymentsummary" => await _dashboardRepository.GetPaymentSummaryByTypeAsync(financialYear),
                _ => throw new ArgumentException($"Invalid chart type: {chartType}")
            };
        }
    }
}
