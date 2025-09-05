using BiddingSystem.Models;
using System.Data.SqlClient;
using System.Data;
using Dapper;

namespace BiddingSystem.Data
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DashboardRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<DashboardStatistics> GetDashboardStatisticsAsync(int financialYear)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<DashboardStatistics>(
                "sp_GetDashboardStatistics",
                new { FinancialYear = financialYear },
                commandType: CommandType.StoredProcedure
            );
            return result ?? new DashboardStatistics();
        }

        public async Task<IEnumerable<MonthlyTrendData>> GetMonthlyExpenditureTrendAsync(int financialYear, int months)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<MonthlyTrendData>(
                "sp_GetMonthlyExpenditureTrend",
                new { FinancialYear = financialYear, Months = months },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<IEnumerable<LeasePaymentStatusData>> GetLeasePaymentStatusAsync(int financialYear)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<LeasePaymentStatusData>(
                "sp_GetLeasePaymentStatus",
                new { FinancialYear = financialYear },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<IEnumerable<DepartmentLeaseDistribution>> GetDepartmentWiseLeaseDistributionAsync(int financialYear)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<DepartmentLeaseDistribution>(
                "sp_GetDepartmentWiseLeaseDistribution",
                new { FinancialYear = financialYear },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<IEnumerable<TopVendorData>> GetTopVendorsByPaymentAsync(int topN, int financialYear)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<TopVendorData>(
                "sp_GetTopVendorsByPayment",
                new { TopN = topN, FinancialYear = financialYear },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<IEnumerable<LeaseExpiryAlert>> GetLeaseExpiryAlertsAsync(int daysAhead)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<LeaseExpiryAlert>(
                "sp_GetLeaseExpiryAlert",
                new { DaysAhead = daysAhead },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<IEnumerable<PaymentSummary>> GetPaymentSummaryByTypeAsync(int financialYear)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<PaymentSummary>(
                "sp_GetPaymentSummaryByType",
                new { FinancialYear = financialYear },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(int topN)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<RecentActivity>(
                "sp_GetRecentActivities",
                new { TopN = topN },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }
    }
}
