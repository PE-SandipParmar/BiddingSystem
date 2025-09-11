using Dapper;
using BiddingSystem.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using BiddingSystem.ViewModels;

namespace BiddingSystem.Data
{
    public class RefundRepository : IRefundRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public RefundRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public async Task<PaginatedList<RefundListViewModel>> GetRefundListAsync(RefundSearchFilter filter, string userRole)
        {
            using var connection = new SqlConnection(_connectionString);
            var offset = (filter.PageNumber - 1) * filter.PageSize;

            string sql;
            if (userRole == "Maker" || userRole == "Admin")
            {
                // For Maker: Show eligible bidders for refund
                sql = @"
                    WITH RefundCTE AS (
                        SELECT 
                            tb.Id as TenderBidId,
                            t.Id as TenderId,
                            t.TenderId as TenderIdString,
                            t.TenderTitle,
                            tb.BidderName,
                            tb.CompanyName,
                            tb.TotalAmount as TotalAmountToRefund,
                            rp.Id as RefundPaymentId,
                            rp.RefundStatus,
                            rp.ReasonForRefund,
                            ROW_NUMBER() OVER (ORDER BY t.TenderId, tb.BidderName) as RowNum,
                            COUNT(*) OVER() as TotalCount
                        FROM TenderBids tb
                        INNER JOIN Tenders t ON tb.TenderId = t.Id
                        LEFT JOIN RefundPayments rp ON tb.Id = rp.TenderBidId 
                            AND rp.RefundStatus IN ('Pending', 'Approved')
                        WHERE tb.Status = 'Rejected'
                            AND tb.PaymentStatus = 'Paid'
                            AND t.Status = 3
                            AND tb.IsActive = 1
                            AND (@TenderId IS NULL OR t.TenderId LIKE '%' + @TenderId + '%')
                            AND (@TenderName IS NULL OR t.TenderTitle LIKE '%' + @TenderName + '%')
                    )
                    SELECT * FROM RefundCTE
                    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
                    ORDER BY TenderIdString, BidderName";
            }
            else // Checker role
            {
                // For Checker: Show pending refunds for approval
                sql = @"
                    WITH RefundCTE AS (
                        SELECT 
                            tb.Id as TenderBidId,
                            t.Id as TenderId,
                            t.TenderId as TenderIdString,
                            t.TenderTitle,
                            tb.BidderName,
                            tb.CompanyName,
                            rp.RefundAmount as TotalAmountToRefund,
                            rp.Id as RefundPaymentId,
                            rp.RefundStatus,
                            rp.ReasonForRefund,
                            ROW_NUMBER() OVER (ORDER BY rp.InitiatedAt DESC) as RowNum,
                            COUNT(*) OVER() as TotalCount
                        FROM RefundPayments rp
                        INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                        INNER JOIN Tenders t ON rp.TenderId = t.Id
                        WHERE rp.RefundStatus = 'Pending'
                            AND (@TenderId IS NULL OR t.TenderId LIKE '%' + @TenderId + '%')
                            AND (@TenderName IS NULL OR t.TenderTitle LIKE '%' + @TenderName + '%')
                    )
                    SELECT * FROM RefundCTE
                    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
                    ORDER BY TenderIdString, BidderName";
            }

            var parameters = new
            {
                filter.TenderId,
                filter.TenderName,
                Offset = offset,
                filter.PageSize
            };

            var results = await connection.QueryAsync<dynamic>(sql, parameters);

            var list = new PaginatedList<RefundListViewModel>
            {
                Items = new List<RefundListViewModel>(),
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize
            };

            foreach (var item in results)
            {
                list.TotalCount = (int)item.TotalCount;
                list.Items.Add(new RefundListViewModel
                {
                    TenderBidId = item.TenderBidId,
                    TenderId = item.TenderId,
                    TenderIdString = item.TenderIdString,
                    TenderTitle = item.TenderTitle,
                    BidderName = item.BidderName,
                    CompanyName = item.CompanyName,
                    TotalAmountToRefund = item.TotalAmountToRefund,
                    RefundPaymentId = item.RefundPaymentId,
                    RefundStatus = item.RefundStatus,
                    ReasonForRefund = item.ReasonForRefund ?? "Tender awarded to another bidder",
                    HasPendingRefund = item.RefundPaymentId != null
                });
            }

            list.TotalPages = (int)Math.Ceiling(list.TotalCount / (double)filter.PageSize);
            return list;
        }

        public async Task<bool> InitiateRefundsAsync(List<int> tenderBidIds, string reason, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var tenderBidId in tenderBidIds)
                {
                    // Check if refund already exists
                    var checkSql = @"
                        SELECT COUNT(*) FROM RefundPayments 
                        WHERE TenderBidId = @TenderBidId AND RefundStatus IN ('Pending', 'Approved')";

                    var exists = await connection.QuerySingleAsync<int>(checkSql,
                        new { TenderBidId = tenderBidId }, transaction);

                    if (exists > 0) continue;

                    // Get tender bid details
                    var bidSql = @"
                        SELECT tb.*, t.Id as TenderId 
                        FROM TenderBids tb 
                        INNER JOIN Tenders t ON tb.TenderId = t.Id
                        WHERE tb.Id = @TenderBidId";

                    var bid = await connection.QuerySingleOrDefaultAsync<dynamic>(bidSql,
                        new { TenderBidId = tenderBidId }, transaction);

                    if (bid == null) continue;

                    // Insert refund record
                    var insertSql = @"
                        INSERT INTO RefundPayments (
                            TenderBidId, TenderId, RefundAmount, ReasonForRefund, 
                            RefundStatus, InitiatedBy, InitiatedAt, CreatedAt
                        ) VALUES (
                            @TenderBidId, @TenderId, @RefundAmount, @ReasonForRefund,
                            'Pending', @InitiatedBy, GETUTCDATE(), GETUTCDATE()
                        )";

                    await connection.ExecuteAsync(insertSql, new
                    {
                        TenderBidId = tenderBidId,
                        TenderId = bid.TenderId,
                        RefundAmount = bid.TotalAmount,
                        ReasonForRefund = reason,
                        InitiatedBy = userId
                    }, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> ProcessRefundsAsync(List<int> refundPaymentIds, string action, string remarks, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var status = action == "Approve" ? "Approved" : "Rejected";

                foreach (var refundId in refundPaymentIds)
                {
                    // Update refund status
                    var updateSql = @"
                        UPDATE RefundPayments 
                        SET RefundStatus = @Status,
                            ApprovedBy = @ApprovedBy,
                            ApprovedAt = GETUTCDATE(),
                            CheckerRemarks = @Remarks
                        WHERE Id = @RefundId AND RefundStatus = 'Pending'";

                    await connection.ExecuteAsync(updateSql, new
                    {
                        RefundId = refundId,
                        Status = status,
                        ApprovedBy = userId,
                        Remarks = remarks
                    }, transaction);

                    // If approved, update TenderBid payment status
                    if (status == "Approved")
                    {
                        var updateBidSql = @"
                            UPDATE tb 
                            SET tb.PaymentStatus = 'Refunded',
                                tb.UpdatedAt = GETUTCDATE()
                            FROM TenderBids tb
                            INNER JOIN RefundPayments rp ON tb.Id = rp.TenderBidId
                            WHERE rp.Id = @RefundId";

                        await connection.ExecuteAsync(updateBidSql,
                            new { RefundId = refundId }, transaction);
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<RefundStatistics> GetRefundStatisticsAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    COUNT(CASE WHEN RefundStatus = 'Pending' THEN 1 END) as TotalPendingRefunds,
                    COUNT(CASE WHEN RefundStatus = 'Approved' THEN 1 END) as TotalApprovedRefunds,
                    COUNT(CASE WHEN RefundStatus = 'Rejected' THEN 1 END) as TotalRejectedRefunds,
                    SUM(RefundAmount) as TotalRefundAmount,
                    SUM(CASE WHEN RefundStatus = 'Pending' THEN RefundAmount ELSE 0 END) as PendingRefundAmount,
                    SUM(CASE WHEN RefundStatus = 'Approved' THEN RefundAmount ELSE 0 END) as ApprovedRefundAmount
                FROM RefundPayments";

            var stats = await connection.QuerySingleOrDefaultAsync<RefundStatistics>(sql);
            return stats ?? new RefundStatistics();
        }

        public async Task<PaginatedList<ApprovedRefundViewModel>> GetRefundsByStatusAsync(RefundSearchFilter filter)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                var offset = (filter.PageNumber - 1) * filter.PageSize;

                var sql = @"
                    -- First get the count
                    SELECT COUNT(*)
                    FROM RefundPayments rp
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN Users u ON rp.ApprovedBy = u.Id
                    WHERE rp.RefundStatus = @RefundStatus
                        AND (@TenderId IS NULL OR @TenderId = '' OR t.TenderId LIKE '%' + @TenderId + '%')
                        AND (@TenderName IS NULL OR @TenderName = '' OR t.TenderTitle LIKE '%' + @TenderName + '%')
                        AND (@FromDate IS NULL OR rp.ApprovedAt >= @FromDate);

                    -- Then get the data
                    SELECT 
                        rp.Id as RefundPaymentId,
                        tb.Id as TenderBidId,
                        t.Id as TenderId,
                        t.TenderId as TenderIdString,
                        t.TenderTitle,
                        tb.BidderName,
                        tb.BidderEmail,
                        tb.CompanyName,
                        rp.RefundAmount,
                        rp.ReasonForRefund,
                        rp.RefundStatus,
                        rp.InitiatedAt,
                        rp.ApprovedAt,
                        rp.CheckerRemarks,
                        u.Username as ApprovedByName,
                        CASE 
                            WHEN tb.PaymentStatus = 'Refunded' THEN 1 
                            ELSE 0 
                        END as PaymentProcessed
                    FROM RefundPayments rp
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN Users u ON rp.ApprovedBy = u.Id
                    WHERE rp.RefundStatus = @RefundStatus
                        AND (@TenderId IS NULL OR @TenderId = '' OR t.TenderId LIKE '%' + @TenderId + '%')
                        AND (@TenderName IS NULL OR @TenderName = '' OR t.TenderTitle LIKE '%' + @TenderName + '%')
                        AND (@FromDate IS NULL OR rp.ApprovedAt >= @FromDate)
                    ORDER BY rp.ApprovedAt DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var parameters = new
                {
                    RefundStatus = filter.RefundStatus ?? "Approved",
                    TenderId = filter.TenderId ?? "",
                    TenderName = filter.TenderName ?? "",
                    FromDate = filter.FromDate,
                    Offset = offset,
                    PageSize = filter.PageSize
                };

                using var multi = await connection.QueryMultipleAsync(sql, parameters);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<ApprovedRefundViewModel>()).ToList();

                var result = new PaginatedList<ApprovedRefundViewModel>
                {
                    Items = items,
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };

                result.TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                return result;
            }
            catch (Exception ex)
            {
                //_logger?.LogError(ex, "Error in GetRefundsByStatusAsync");
                throw;
            }
        }

        public async Task<ApprovedRefundViewModel?> GetRefundDetailsAsync(int refundPaymentId)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                var sql = @"
                    SELECT 
                        rp.Id as RefundPaymentId,
                        tb.Id as TenderBidId,
                        t.Id as TenderId,
                        t.TenderId as TenderIdString,
                        t.TenderTitle,
                        tb.BidderName,
                        tb.BidderEmail,
                        tb.CompanyName,
                        rp.RefundAmount,
                        rp.ReasonForRefund,
                        rp.RefundStatus,
                        rp.InitiatedAt,
                        rp.ApprovedAt,
                        rp.CheckerRemarks,
                        u.Username as ApprovedByName,
                        CASE 
                            WHEN tb.PaymentStatus = 'Refunded' THEN 1 
                            ELSE 0 
                        END as PaymentProcessed
                    FROM RefundPayments rp
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN Users u ON rp.ApprovedBy = u.Id
                    WHERE rp.Id = @RefundPaymentId";

                var parameters = new { RefundPaymentId = refundPaymentId };
                var result = await connection.QueryFirstOrDefaultAsync<ApprovedRefundViewModel>(sql, parameters);
                
                return result;
            }
            catch (Exception ex)
            {
                // Log error if needed
                return null;
            }
        }
    }
}
