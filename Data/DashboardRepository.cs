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
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<DashboardStatistics> GetStatisticsAsync(string userId, string userRole, int year)
        {
            using var connection = CreateConnection();
            
            // First, let's check if RefundRequests table exists and has data
            var debugSql = @"
                SELECT 
                    (SELECT COUNT(*) FROM RefundRequests) as TotalRefundRequests,
                    (SELECT COUNT(*) FROM RefundRequests WHERE Status = 1) as AllPendingRefunds,
                    (SELECT COUNT(*) FROM RefundRequests WHERE Status = 4) as AllProcessedRefunds,
                    (SELECT COUNT(*) FROM RefundRequests WHERE YEAR(RequestedAt) = @Year) as YearRefundRequests,
                    (SELECT COUNT(*) FROM RefundRequests WHERE YEAR(CreatedAt) = @Year) as YearCreatedRefunds";
            
            var debugResult = await connection.QueryFirstOrDefaultAsync<dynamic>(debugSql, new { Year = year });
            
            var sql = @"
                SELECT 
                    (SELECT COUNT(*) FROM Tenders WHERE YEAR(CreatedAt) = @Year) as TotalTenders,
                    (SELECT COUNT(*) FROM TenderBids WHERE YEAR(CreatedAt) = @Year) as TotalBids,
                    (SELECT COUNT(*) FROM Tenders WHERE Status = 2 AND YEAR(CreatedAt) = @Year) as ActiveTenders,
                    (SELECT COUNT(*) FROM TenderBids WHERE Status = 'Submitted' AND YEAR(CreatedAt) = @Year) as SubmittedBids,
                    (SELECT COUNT(*) FROM EMDSDDeposits WHERE Status = 'Pending') as PendingRefunds,
                    (SELECT ISNULL(SUM(BidAmount), 0) FROM TenderBids WHERE YEAR(CreatedAt) = @Year) as TotalBidAmount,
                    (SELECT ISNULL(SUM(EmdAmount), 0) FROM TenderBids WHERE YEAR(CreatedAt) = @Year AND PaymentStatus = 'Paid') as TotalEMDSDAmount,
                    (SELECT ISNULL(SUM(SdAmount), 0) FROM Tenders WHERE YEAR(CreatedAt) = @Year) as TotalSDAmount,
                    (SELECT COUNT(*) FROM EMDSDDeposits WHERE Status = 'Refunded') as ProcessedRefunds,
                    (SELECT ISNULL(SUM(RequestedAmount), 0) FROM RefundRequests WHERE Status = 2 AND YEAR(RequestedAt) = @Year) as TotalRefundAmount";

            var result = await connection.QueryFirstOrDefaultAsync<DashboardStatistics>(sql, new { Year = year });
            
            // Add debug information to the result
            if (result != null)
            {
                // We'll add debug info to the view instead
            }
            
            return result ?? new DashboardStatistics();
        }

        public async Task<List<Tender>> GetRecentTendersAsync(string userId, string userRole, int count)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT TOP (@Count) 
                    Id, TenderId, TenderTitle, Description, Department, PublishDate, EmdAmount, SdAmount,
                    ProcessingFee, EstimatedValue, LastDateEmd, TenderClosingDate, TenderOpeningDate,
                    Status, CreatedBy, CreatedAt, UpdatedAt, PublishedAt, IsActive
                FROM Tenders 
                ORDER BY CreatedAt DESC";

            var result = await connection.QueryAsync<Tender>(sql, new { Count = count });
            return result.ToList();
        }

        public async Task<List<TenderBid>> GetRecentBidsAsync(string userId, string userRole, int count)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT TOP (@Count) 
                    Id, TenderId, BidderName, BidderEmail, BidderPhone, CompanyName, CompanyAddress,
                    BidAmount, EmdAmount, ProcessingFee, TotalAmount, Status, PaymentStatus, 
                    PaymentReference, PaymentDate, SubmittedAt, IsActive, CreatedAt, UpdatedAt, Remarks
                FROM TenderBids 
                ORDER BY CreatedAt DESC";

            var result = await connection.QueryAsync<TenderBid>(sql, new { Count = count });
            return result.ToList();
        }

        public async Task<List<RefundRequest>> GetPendingRefundsAsync(string userId, string userRole, int count)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT TOP (@Count) 
                    Id, RefundId, TenderBidId, PaymentLinkId, Type, RequestedAmount, ApprovedAmount,
                    Reason, Status, RequestedBy, ProcessedBy, RequestedAt, ProcessedAt, 
                    Remarks, BankDetails, RefundReference, CreatedAt, UpdatedAt
                FROM RefundRequests 
                WHERE Status = 1
                ORDER BY RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest>(sql, new { Count = count });
            return result.ToList();
        }

        public async Task<List<Tender>> GetActiveTendersAsync(string userId, string userRole, int count)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT TOP (@Count) 
                    Id, TenderId, TenderTitle, Description, Department, PublishDate, EmdAmount, SdAmount,
                    ProcessingFee, EstimatedValue, LastDateEmd, TenderClosingDate, TenderOpeningDate,
                    Status, CreatedBy, CreatedAt, UpdatedAt, PublishedAt, IsActive
                FROM Tenders 
                WHERE Status = 2 AND TenderClosingDate > GETDATE()
                ORDER BY TenderClosingDate ASC";

            var result = await connection.QueryAsync<Tender>(sql, new { Count = count });
            return result.ToList();
        }

        public async Task<List<Tender>> GetExpiringTendersAsync(string userId, string userRole, int count)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT TOP (@Count) 
                    Id, TenderId, TenderTitle, Description, Department, PublishDate, EmdAmount, SdAmount,
                    ProcessingFee, EstimatedValue, LastDateEmd, TenderClosingDate, TenderOpeningDate,
                    Status, CreatedBy, CreatedAt, UpdatedAt, PublishedAt, IsActive
                FROM Tenders 
                WHERE Status = 2 AND TenderClosingDate BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE())
                ORDER BY TenderClosingDate ASC";

            var result = await connection.QueryAsync<Tender>(sql, new { Count = count });
            return result.ToList();
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT DISTINCT YEAR(CreatedAt) as Year
                FROM Tenders
                UNION
                SELECT DISTINCT YEAR(CreatedAt) as Year
                FROM TenderBids
                ORDER BY Year DESC";

            var result = await connection.QueryAsync<int>(sql);
            return result.ToList();
        }

        public async Task<List<RecentActivity>> GetRecentActivityAsync(string userId, string userRole)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT TOP 10
                    'Tender' as ActivityType,
                    'Tender created: ' + TenderTitle as Description,
                    'System' as UserName,
                    CreatedAt as Timestamp,
                    'Tender' as EntityType,
                    Id as EntityId,
                    Status
                FROM Tenders
                WHERE CreatedAt >= DATEADD(DAY, -30, GETDATE())
                
                UNION ALL
                
                SELECT TOP 10
                    'Bid' as ActivityType,
                    'Bid submitted for tender: ' + t.TenderTitle as Description,
                    u.Username as UserName,
                    tb.CreatedAt as Timestamp,
                    'TenderBid' as EntityType,
                    tb.Id as EntityId,
                    tb.Status
                FROM TenderBids tb
                INNER JOIN Tenders t ON tb.TenderId = t.Id
                INNER JOIN Users u ON tb.UserId = u.Id
                WHERE tb.CreatedAt >= DATEADD(DAY, -30, GETDATE())
                
                ORDER BY Timestamp DESC";

            var result = await connection.QueryAsync<RecentActivity>(sql);
            return result.ToList();
        }

        public async Task<Dictionary<string, int>> GetTenderStatusDistributionAsync(string userId, string userRole)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT 
                    CASE Status
                        WHEN 1 THEN 'Draft'
                        WHEN 2 THEN 'Published'
                        WHEN 3 THEN 'Closed'
                        WHEN 4 THEN 'Cancelled'
                        ELSE 'Unknown'
                    END as StatusName,
                    COUNT(*) as Count
                FROM Tenders
                GROUP BY Status";

            var result = await connection.QueryAsync<dynamic>(sql);
            return result.ToDictionary(x => (string)x.StatusName, x => (int)x.Count);
        }

        public async Task<Dictionary<string, int>> GetBidStatusDistributionAsync(string userId, string userRole)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT Status, COUNT(*) as Count
                FROM TenderBids
                GROUP BY Status";

            var result = await connection.QueryAsync<dynamic>(sql);
            return result.ToDictionary(x => (string)x.Status, x => (int)x.Count);
        }

        public async Task<MonthlyTrends> GetMonthlyTrendsAsync(string userId, string userRole, int year)
        {
            using var connection = CreateConnection();
            
            var sql = @"
                WITH Months AS (
                    SELECT 1 as Month, 'Jan' as MonthName UNION ALL
                    SELECT 2, 'Feb' UNION ALL SELECT 3, 'Mar' UNION ALL SELECT 4, 'Apr' UNION ALL
                    SELECT 5, 'May' UNION ALL SELECT 6, 'Jun' UNION ALL SELECT 7, 'Jul' UNION ALL
                    SELECT 8, 'Aug' UNION ALL SELECT 9, 'Sep' UNION ALL SELECT 10, 'Oct' UNION ALL
                    SELECT 11, 'Nov' UNION ALL SELECT 12, 'Dec'
                )
                SELECT 
                    m.MonthName as Month,
                    ISNULL(t.TenderCount, 0) as TenderCount,
                    ISNULL(b.BidCount, 0) as BidCount,
                    ISNULL(b.TotalBidAmount, 0) as TotalBidAmount,
                    ISNULL(b.TotalEMDSDAmount, 0) as TotalEMDSDAmount,
                    ISNULL(e.TotalCollectedEMD, 0) as TotalCollectedEMD
                FROM Months m
                LEFT JOIN (
                    SELECT MONTH(CreatedAt) as Month, COUNT(*) as TenderCount
                    FROM Tenders WHERE YEAR(CreatedAt) = @Year
                    GROUP BY MONTH(CreatedAt)
                ) t ON m.Month = t.Month
                LEFT JOIN (
                    SELECT MONTH(CreatedAt) as Month, 
                           COUNT(*) as BidCount,
                           ISNULL(SUM(BidAmount), 0) as TotalBidAmount,
                           ISNULL(SUM(EmdAmount), 0) as TotalEMDSDAmount
                    FROM TenderBids WHERE YEAR(CreatedAt) = @Year
                    GROUP BY MONTH(CreatedAt)
                ) b ON m.Month = b.Month
                LEFT JOIN (
                    SELECT MONTH(TransactionDate) as Month,
                           ISNULL(SUM(Amount), 0) as TotalCollectedEMD
                    FROM EMDSDDeposits 
                    WHERE YEAR(TransactionDate) = @Year AND Status = 'Paid'
                    GROUP BY MONTH(TransactionDate)
                ) e ON m.Month = e.Month
                ORDER BY m.Month";

            var result = await connection.QueryAsync<dynamic>(sql, new { Year = year });
            
            return new MonthlyTrends
            {
                Months = result.Select(x => (string)x.Month).ToList(),
                TenderCounts = result.Select(x => (int)x.TenderCount).ToList(),
                BidCounts = result.Select(x => (int)x.BidCount).ToList(),
                TotalBidAmounts = result.Select(x => (decimal)x.TotalBidAmount).ToList(),
                TotalEMDSDAmounts = result.Select(x => (decimal)x.TotalEMDSDAmount).ToList(),
                TotalCollectedEMDs = result.Select(x => (decimal)x.TotalCollectedEMD).ToList()
            };
        }

        public async Task<object> GetRefundDebugInfoAsync()
        {
            using var connection = CreateConnection();
            
            var sql = @"
                SELECT 
                    -- RefundRequests table
                    (SELECT COUNT(*) FROM RefundRequests) as TotalRefundRequests,
                    (SELECT COUNT(*) FROM RefundRequests WHERE Status = 1) as RefundRequestsPending,
                    (SELECT COUNT(*) FROM RefundRequests WHERE Status = 4) as RefundRequestsCompleted,
                    
                    -- EMDSDDeposits table
                    (SELECT COUNT(*) FROM EMDSDDeposits) as TotalEMDSDDeposits,
                    (SELECT COUNT(*) FROM EMDSDDeposits WHERE Status = 'Pending') as EMDSDPending,
                    (SELECT COUNT(*) FROM EMDSDDeposits WHERE Status = 'Paid') as EMDSDPaid,
                    (SELECT COUNT(*) FROM EMDSDDeposits WHERE Status = 'Refunded') as EMDSDRefunded,
                    (SELECT COUNT(*) FROM EMDSDDeposits WHERE Status = 'Failed') as EMDSDFailed,
                    
                    -- Sample data
                    (SELECT TOP 1 Status FROM EMDSDDeposits) as SampleEMDSDStatus,
                    (SELECT TOP 1 Type FROM EMDSDDeposits) as SampleEMDSDType";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql);
            return result;
        }
    }
}
