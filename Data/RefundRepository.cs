using Dapper;
using BiddingSystem.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace BiddingSystem.Data
{
    public class RefundRepository : IRefundRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<RefundRepository> _logger;

        public RefundRepository(IConfiguration configuration, ILogger<RefundRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        #region RefundRequest CRUD Operations

        public async Task<RefundRequest?> GetRefundRequestByIdAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                WHERE rr.Id = @Id";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, new { Id = id }, splitOn: "Id,Id,Id");

            return result.FirstOrDefault();
        }

        public async Task<RefundRequest?> GetRefundRequestByRefundIdAsync(string refundId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                WHERE rr.RefundId = @RefundId";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, new { RefundId = refundId }, splitOn: "Id,Id,Id");

            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<RefundRequest>> GetAllRefundRequestsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                ORDER BY rr.RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, splitOn: "Id,Id,Id");

            return result;
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundRequestsByTenderBidIdAsync(int tenderBidId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                WHERE rr.TenderBidId = @TenderBidId
                ORDER BY rr.RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, new { TenderBidId = tenderBidId }, splitOn: "Id,Id,Id");

            return result;
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundRequestsByPaymentLinkIdAsync(int paymentLinkId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                WHERE rr.PaymentLinkId = @PaymentLinkId
                ORDER BY rr.RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, new { PaymentLinkId = paymentLinkId }, splitOn: "Id,Id,Id");

            return result;
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundRequestsByStatusAsync(RefundStatus status)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                WHERE rr.Status = @Status
                ORDER BY rr.RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, new { Status = status.ToString() }, splitOn: "Id,Id,Id");

            return result;
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundRequestsByUserAsync(string requestedBy)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                WHERE rr.RequestedBy = @RequestedBy
                ORDER BY rr.RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, new { RequestedBy = requestedBy }, splitOn: "Id,Id,Id");

            return result;
        }

        #endregion

        #region Search and Filtering

        public async Task<IEnumerable<RefundRequest>> SearchRefundRequestsAsync(
            string searchTerm = "", 
            RefundStatus? status = null, 
            RefundType? type = null, 
            RefundReason? reason = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            int page = 1, 
            int pageSize = 25)
        {
            using var connection = CreateConnection();
            
            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereConditions.Add("(rr.RefundId LIKE @SearchTerm OR rr.RequestedBy LIKE @SearchTerm OR tb.BidderName LIKE @SearchTerm OR tb.CompanyName LIKE @SearchTerm)");
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }
            
            if (status.HasValue)
            {
                whereConditions.Add("rr.Status = @Status");
                parameters.Add("Status", status.Value.ToString());
            }
            
            if (type.HasValue)
            {
                whereConditions.Add("rr.Type = @Type");
                parameters.Add("Type", type.Value.ToString());
            }
            
            if (reason.HasValue)
            {
                whereConditions.Add("rr.Reason = @Reason");
                parameters.Add("Reason", reason.Value.ToString());
            }
            
            if (fromDate.HasValue)
            {
                whereConditions.Add("rr.RequestedAt >= @FromDate");
                parameters.Add("FromDate", fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                whereConditions.Add("rr.RequestedAt <= @ToDate");
                parameters.Add("ToDate", toDate.Value);
            }
            
            var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
            var offset = (page - 1) * pageSize;
            
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);
            
            var sql = $@"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                {whereClause}
                ORDER BY rr.RequestedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, parameters, splitOn: "Id,Id,Id");

            return result;
        }

        public async Task<int> GetSearchCountAsync(
            string searchTerm = "", 
            RefundStatus? status = null, 
            RefundType? type = null, 
            RefundReason? reason = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null)
        {
            using var connection = CreateConnection();
            
            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereConditions.Add("(rr.RefundId LIKE @SearchTerm OR rr.RequestedBy LIKE @SearchTerm OR tb.BidderName LIKE @SearchTerm OR tb.CompanyName LIKE @SearchTerm)");
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }
            
            if (status.HasValue)
            {
                whereConditions.Add("rr.Status = @Status");
                parameters.Add("Status", status.Value.ToString());
            }
            
            if (type.HasValue)
            {
                whereConditions.Add("rr.Type = @Type");
                parameters.Add("Type", type.Value.ToString());
            }
            
            if (reason.HasValue)
            {
                whereConditions.Add("rr.Reason = @Reason");
                parameters.Add("Reason", reason.Value.ToString());
            }
            
            if (fromDate.HasValue)
            {
                whereConditions.Add("rr.RequestedAt >= @FromDate");
                parameters.Add("FromDate", fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                whereConditions.Add("rr.RequestedAt <= @ToDate");
                parameters.Add("ToDate", toDate.Value);
            }
            
            var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
            
            var sql = $@"
                SELECT COUNT(*)
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                {whereClause}";

            return await connection.QuerySingleAsync<int>(sql, parameters);
        }

        #endregion

        #region Statistics and Counts

        public async Task<Dictionary<RefundStatus, int>> GetStatusCountsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Status, COUNT(*) as Count
                FROM RefundRequests
                GROUP BY Status";

            var result = await connection.QueryAsync(sql);
            return result.ToDictionary(
                row => Enum.Parse<RefundStatus>((string)row.Status), 
                row => (int)row.Count);
        }

        public async Task<Dictionary<RefundType, int>> GetTypeCountsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Type, COUNT(*) as Count
                FROM RefundRequests
                GROUP BY Type";

            var result = await connection.QueryAsync(sql);
            return result.ToDictionary(
                row => Enum.Parse<RefundType>((string)row.Type), 
                row => (int)row.Count);
        }

        public async Task<Dictionary<RefundReason, int>> GetReasonCountsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Reason, COUNT(*) as Count
                FROM RefundRequests
                GROUP BY Reason";

            var result = await connection.QueryAsync(sql);
            return result.ToDictionary(
                row => Enum.Parse<RefundReason>((string)row.Reason), 
                row => (int)row.Count);
        }

        public async Task<decimal> GetTotalRefundedAmountAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = CreateConnection();
            
            var whereClause = "";
            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue || toDate.HasValue)
            {
                var conditions = new List<string>();
                if (fromDate.HasValue)
                {
                    conditions.Add("ProcessedAt >= @FromDate");
                    parameters.Add("FromDate", fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    conditions.Add("ProcessedAt <= @ToDate");
                    parameters.Add("ToDate", toDate.Value);
                }
                whereClause = "WHERE " + string.Join(" AND ", conditions);
            }
            
            var sql = $@"
                SELECT ISNULL(SUM(ApprovedAmount), 0)
                FROM RefundRequests
                {whereClause}
                AND Status = 'Completed'";

            return await connection.QuerySingleAsync<decimal>(sql, parameters);
        }

        public async Task<decimal> GetPendingRefundAmountAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT ISNULL(SUM(RequestedAmount), 0)
                FROM RefundRequests
                WHERE Status IN ('Pending', 'Approved', 'Processing')";

            return await connection.QuerySingleAsync<decimal>(sql);
        }

        #endregion

        #region CRUD Operations

        public async Task<RefundRequest> CreateRefundRequestAsync(RefundRequest refundRequest)
        {
            using var connection = CreateConnection();
            
            // Generate RefundId if not provided
            if (string.IsNullOrEmpty(refundRequest.RefundId))
            {
                refundRequest.RefundId = RefundRequest.GenerateRefundId();
            }
            
            refundRequest.CreatedAt = DateTime.UtcNow;
            refundRequest.RequestedAt = DateTime.UtcNow;
            
            const string sql = @"
                INSERT INTO RefundRequests 
                (RefundId, TenderBidId, PaymentLinkId, EMDSDDepositId, Type, RequestedAmount, ApprovedAmount, 
                 Reason, Status, RequestedBy, ProcessedBy, RequestedAt, ProcessedAt, 
                 Remarks, BankDetails, RefundReference, CreatedAt, UpdatedAt,
                 CreatedBy, ApprovedBy, ApprovedAt,
                 FirstCheckerId, FirstCheckerApprovedAt, FirstCheckerRemarks,
                 SecondCheckerId, SecondCheckerApprovedAt, SecondCheckerRemarks,
                 WorkflowStatus)
                VALUES 
                (@RefundId, @TenderBidId, @PaymentLinkId, @EMDSDDepositId, @Type, @RequestedAmount, @ApprovedAmount, 
                 @Reason, @Status, @RequestedBy, @ProcessedBy, @RequestedAt, @ProcessedAt, 
                 @Remarks, @BankDetails, @RefundReference, @CreatedAt, @UpdatedAt,
                 @CreatedBy, @ApprovedBy, @ApprovedAt,
                 @FirstCheckerId, @FirstCheckerApprovedAt, @FirstCheckerRemarks,
                 @SecondCheckerId, @SecondCheckerApprovedAt, @SecondCheckerRemarks,
                 @WorkflowStatus);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            try
            {
                var id = await connection.QuerySingleAsync<int>(sql, refundRequest);
                refundRequest.Id = id;
                return refundRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund request. RefundId: {RefundId}", refundRequest.RefundId);
                throw;
            }
        }

        public async Task<bool> UpdateRefundRequestAsync(RefundRequest refundRequest)
        {
            using var connection = CreateConnection();
            
            refundRequest.UpdatedAt = DateTime.UtcNow;
            
            const string sql = @"
                UPDATE RefundRequests 
                SET RefundId = @RefundId, TenderBidId = @TenderBidId, PaymentLinkId = @PaymentLinkId, 
                    Type = @Type, RequestedAmount = @RequestedAmount, ApprovedAmount = @ApprovedAmount, 
                    Reason = @Reason, Status = @Status, RequestedBy = @RequestedBy, ProcessedBy = @ProcessedBy, 
                    RequestedAt = @RequestedAt, ProcessedAt = @ProcessedAt, Remarks = @Remarks, 
                    BankDetails = @BankDetails, RefundReference = @RefundReference, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, refundRequest);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund request. Id: {Id}", refundRequest.Id);
                throw;
            }
        }

        public async Task<bool> DeleteRefundRequestAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = "DELETE FROM RefundRequests WHERE Id = @Id";

            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting refund request. Id: {Id}", id);
                throw;
            }
        }

        #endregion

        #region Status Updates

        public async Task<bool> UpdateRefundStatusAsync(int id, RefundStatus status, int? processedBy = null, string? remarks = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET Status = @Status, ProcessedBy = @ProcessedBy, ProcessedAt = @ProcessedAt, 
                    Remarks = @Remarks, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            try
            {
                var parameters = new
                {
                    Id = id,
                    Status = status.ToString(),
                    ProcessedBy = processedBy,
                    ProcessedAt = DateTime.UtcNow,
                    Remarks = remarks,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund status. Id: {Id}, Status: {Status}", id, status);
                throw;
            }
        }

        public async Task<bool> ApproveRefundAsync(int id, decimal approvedAmount, int processedBy, string? remarks = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET Status = 'Approved', ApprovedAmount = @ApprovedAmount, ProcessedBy = @ProcessedBy, 
                    ProcessedAt = @ProcessedAt, Remarks = @Remarks, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            try
            {
                var parameters = new
                {
                    Id = id,
                    ApprovedAmount = approvedAmount,
                    ProcessedBy = processedBy,
                    ProcessedAt = DateTime.UtcNow,
                    Remarks = remarks,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving refund. Id: {Id}, Amount: {Amount}", id, approvedAmount);
                throw;
            }
        }

        public async Task<bool> RejectRefundAsync(int id, int processedBy, string? remarks = null)
        {
            return await UpdateRefundStatusAsync(id, RefundStatus.Rejected, processedBy, remarks);
        }

        public async Task<bool> ProcessRefundAsync(int id, int processedBy, string? refundReference = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET Status = 'Processing', ProcessedBy = @ProcessedBy, ProcessedAt = @ProcessedAt, 
                    RefundReference = @RefundReference, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            try
            {
                var parameters = new
                {
                    Id = id,
                    ProcessedBy = processedBy,
                    ProcessedAt = DateTime.UtcNow,
                    RefundReference = refundReference,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund. Id: {Id}", id);
                throw;
            }
        }

        #endregion

        #region RefundTransaction Operations

        public async Task<RefundTransaction?> GetRefundTransactionByIdAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rt.*, rr.*
                FROM RefundTransactions rt
                LEFT JOIN RefundRequests rr ON rt.RefundRequestId = rr.Id
                WHERE rt.Id = @Id";

            var result = await connection.QueryAsync<RefundTransaction, RefundRequest, RefundTransaction>(
                sql, (transaction, refundRequest) =>
                {
                    transaction.RefundRequest = refundRequest;
                    return transaction;
                }, new { Id = id }, splitOn: "Id");

            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<RefundTransaction>> GetRefundTransactionsByRequestIdAsync(int refundRequestId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rt.*, rr.*
                FROM RefundTransactions rt
                LEFT JOIN RefundRequests rr ON rt.RefundRequestId = rr.Id
                WHERE rt.RefundRequestId = @RefundRequestId
                ORDER BY rt.ProcessedAt DESC";

            var result = await connection.QueryAsync<RefundTransaction, RefundRequest, RefundTransaction>(
                sql, (transaction, refundRequest) =>
                {
                    transaction.RefundRequest = refundRequest;
                    return transaction;
                }, new { RefundRequestId = refundRequestId }, splitOn: "Id");

            return result;
        }

        public async Task<RefundTransaction> CreateRefundTransactionAsync(RefundTransaction transaction)
        {
            using var connection = CreateConnection();
            
            // Generate TransactionReference if not provided
            if (string.IsNullOrEmpty(transaction.TransactionReference))
            {
                transaction.TransactionReference = RefundTransaction.GenerateTransactionReference();
            }
            
            transaction.CreatedAt = DateTime.UtcNow;
            
            const string sql = @"
                INSERT INTO RefundTransactions 
                (RefundRequestId, TransactionReference, Amount, Status, ProcessedAt, 
                 BankResponse, FailureReason, CreatedAt, UpdatedAt, CreatedBy)
                VALUES 
                (@RefundRequestId, @TransactionReference, @Amount, @Status, @ProcessedAt, 
                 @BankResponse, @FailureReason, @CreatedAt, @UpdatedAt, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            try
            {
                var id = await connection.QuerySingleAsync<int>(sql, transaction);
                transaction.Id = id;
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund transaction. RefundRequestId: {RefundRequestId}", transaction.RefundRequestId);
                throw;
            }
        }

        public async Task<bool> UpdateRefundTransactionAsync(RefundTransaction transaction)
        {
            using var connection = CreateConnection();
            
            transaction.UpdatedAt = DateTime.UtcNow;
            
            const string sql = @"
                UPDATE RefundTransactions 
                SET RefundRequestId = @RefundRequestId, TransactionReference = @TransactionReference, 
                    Amount = @Amount, Status = @Status, ProcessedAt = @ProcessedAt, 
                    BankResponse = @BankResponse, FailureReason = @FailureReason, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, transaction);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund transaction. Id: {Id}", transaction.Id);
                throw;
            }
        }

        public async Task<bool> UpdateTransactionStatusAsync(int id, RefundTransactionStatus status, string? bankResponse = null, string? failureReason = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundTransactions 
                SET Status = @Status, BankResponse = @BankResponse, FailureReason = @FailureReason, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            try
            {
                var parameters = new
                {
                    Id = id,
                    Status = status.ToString(),
                    BankResponse = bankResponse,
                    FailureReason = failureReason,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction status. Id: {Id}, Status: {Status}", id, status);
                throw;
            }
        }

        #endregion

        #region Validation Methods

        public async Task<bool> CanCreateRefundRequestAsync(int tenderBidId, int paymentLinkId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT COUNT(*)
                FROM RefundRequests
                WHERE TenderBidId = @TenderBidId AND PaymentLinkId = @PaymentLinkId
                AND Status NOT IN ('Rejected', 'Failed')";

            var count = await connection.QuerySingleAsync<int>(sql, new { TenderBidId = tenderBidId, PaymentLinkId = paymentLinkId });
            return count == 0;
        }

        public async Task<bool> RefundRequestExistsAsync(int tenderBidId, int paymentLinkId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT COUNT(*)
                FROM RefundRequests
                WHERE TenderBidId = @TenderBidId AND PaymentLinkId = @PaymentLinkId";

            var count = await connection.QuerySingleAsync<int>(sql, new { TenderBidId = tenderBidId, PaymentLinkId = paymentLinkId });
            return count > 0;
        }

        public async Task<bool> IsRefundRequestEligibleAsync(int tenderBidId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT tb.PaymentStatus, pl.Status as PaymentLinkStatus
                FROM TenderBids tb
                LEFT JOIN PaymentLinks pl ON tb.PaymentReference = pl.TransactionId
                WHERE tb.Id = @TenderBidId";

            var result = await connection.QueryFirstOrDefaultAsync(sql, new { TenderBidId = tenderBidId });
            
            if (result == null) return false;
            
            // Check if payment is completed and not already refunded
            return result.PaymentStatus == "Paid" && 
                   result.PaymentLinkStatus == "Used" &&
                   !await RefundRequestExistsAsync(tenderBidId, result.PaymentLinkId ?? 0);
        }

        #endregion

        #region Dashboard and Reporting

        public async Task<IEnumerable<RefundRequest>> GetRecentRefundRequestsAsync(int count = 10)
        {
            using var connection = CreateConnection();
            var sql = $@"
                SELECT TOP {count} rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                ORDER BY rr.RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, splitOn: "Id,Id,Id");

            return result;
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundRequestsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                WHERE rr.RequestedAt BETWEEN @FromDate AND @ToDate
                ORDER BY rr.RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, new { FromDate = fromDate, ToDate = toDate }, splitOn: "Id,Id,Id");

            return result;
        }

        public async Task<object> GetRefundStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = CreateConnection();
            
            var whereClause = "";
            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue || toDate.HasValue)
            {
                var conditions = new List<string>();
                if (fromDate.HasValue)
                {
                    conditions.Add("RequestedAt >= @FromDate");
                    parameters.Add("FromDate", fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    conditions.Add("RequestedAt <= @ToDate");
                    parameters.Add("ToDate", toDate.Value);
                }
                whereClause = "WHERE " + string.Join(" AND ", conditions);
            }
            
            var sql = $@"
                SELECT 
                    COUNT(*) as TotalRequests,
                    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as PendingCount,
                    SUM(CASE WHEN Status = 'Approved' THEN 1 ELSE 0 END) as ApprovedCount,
                    SUM(CASE WHEN Status = 'Processing' THEN 1 ELSE 0 END) as ProcessingCount,
                    SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) as CompletedCount,
                    SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END) as RejectedCount,
                    SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as FailedCount,
                    ISNULL(SUM(RequestedAmount), 0) as TotalRequestedAmount,
                    ISNULL(SUM(ApprovedAmount), 0) as TotalApprovedAmount,
                    ISNULL(SUM(CASE WHEN Status = 'Completed' THEN ApprovedAmount ELSE 0 END), 0) as TotalRefundedAmount
                FROM RefundRequests
                {whereClause}";

            return await connection.QuerySingleAsync(sql, parameters);
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundRequestsByTenderBidAsync(int tenderBidId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                WHERE rr.TenderBidId = @TenderBidId
                ORDER BY rr.RequestedAt DESC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    return refund;
                }, new { TenderBidId = tenderBidId }, splitOn: "Id,Id,Id");

            return result;
        }

        public async Task<IEnumerable<RefundTransaction>> GetRefundTransactionsAsync(int refundRequestId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM RefundTransactions 
                WHERE RefundRequestId = @RefundRequestId 
                ORDER BY CreatedAt DESC";

            return await connection.QueryAsync<RefundTransaction>(sql, new { RefundRequestId = refundRequestId });
        }

        #endregion

        #region Checker-Maker Workflow Operations

        public async Task<bool> SubmitForFirstCheckAsync(int id, int submittedBy)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET WorkflowStatus = 'SubmittedForFirstCheck', UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND WorkflowStatus = 'Draft'";

            try
            {
                var parameters = new
                {
                    Id = id,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting refund for first check. Id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> FirstCheckApproveAsync(int id, int checkerId, decimal approvedAmount, string? remarks = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET WorkflowStatus = 'FirstCheckApproved', 
                    FirstCheckerId = @CheckerId, 
                    FirstCheckerApprovedAt = @ApprovedAt,
                    FirstCheckerRemarks = @Remarks,
                    ApprovedAmount = @ApprovedAmount,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND WorkflowStatus = 'SubmittedForFirstCheck'";

            try
            {
                var parameters = new
                {
                    Id = id,
                    CheckerId = checkerId,
                    ApprovedAt = DateTime.UtcNow,
                    Remarks = remarks,
                    ApprovedAmount = approvedAmount,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in first check approval. Id: {Id}, CheckerId: {CheckerId}", id, checkerId);
                throw;
            }
        }

        public async Task<bool> FirstCheckRejectAsync(int id, int checkerId, string? remarks = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET WorkflowStatus = 'FirstCheckRejected', 
                    FirstCheckerId = @CheckerId, 
                    FirstCheckerApprovedAt = @RejectedAt,
                    FirstCheckerRemarks = @Remarks,
                    Status = 'Rejected',
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND WorkflowStatus = 'SubmittedForFirstCheck'";

            try
            {
                var parameters = new
                {
                    Id = id,
                    CheckerId = checkerId,
                    RejectedAt = DateTime.UtcNow,
                    Remarks = remarks,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in first check rejection. Id: {Id}, CheckerId: {CheckerId}", id, checkerId);
                throw;
            }
        }

        public async Task<bool> SubmitForSecondCheckAsync(int id, int submittedBy)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET WorkflowStatus = 'SubmittedForSecondCheck', UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND WorkflowStatus = 'FirstCheckApproved'";

            try
            {
                var parameters = new
                {
                    Id = id,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting refund for second check. Id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> SecondCheckApproveAsync(int id, int checkerId, string? remarks = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET WorkflowStatus = 'ReadyForProcessing', 
                    SecondCheckerId = @CheckerId, 
                    SecondCheckerApprovedAt = @ApprovedAt,
                    SecondCheckerRemarks = @Remarks,
                    Status = 'Approved',
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND WorkflowStatus = 'SubmittedForSecondCheck'";

            try
            {
                var parameters = new
                {
                    Id = id,
                    CheckerId = checkerId,
                    ApprovedAt = DateTime.UtcNow,
                    Remarks = remarks,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in second check approval. Id: {Id}, CheckerId: {CheckerId}", id, checkerId);
                throw;
            }
        }

        public async Task<bool> SecondCheckRejectAsync(int id, int checkerId, string? remarks = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET WorkflowStatus = 'SecondCheckRejected', 
                    SecondCheckerId = @CheckerId, 
                    SecondCheckerApprovedAt = @RejectedAt,
                    SecondCheckerRemarks = @Remarks,
                    Status = 'Rejected',
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND WorkflowStatus = 'SubmittedForSecondCheck'";

            try
            {
                var parameters = new
                {
                    Id = id,
                    CheckerId = checkerId,
                    RejectedAt = DateTime.UtcNow,
                    Remarks = remarks,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in second check rejection. Id: {Id}, CheckerId: {CheckerId}", id, checkerId);
                throw;
            }
        }

        public async Task<bool> MarkReadyForProcessingAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE RefundRequests 
                SET WorkflowStatus = 'ReadyForProcessing', UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND WorkflowStatus = 'SecondCheckApproved'";

            try
            {
                var parameters = new
                {
                    Id = id,
                    UpdatedAt = DateTime.UtcNow
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking refund ready for processing. Id: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundsPendingFirstCheckAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*, cb.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                LEFT JOIN Users cb ON rr.CreatedBy = cb.Id
                WHERE rr.WorkflowStatus = 'SubmittedForFirstCheck'
                ORDER BY rr.RequestedAt ASC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user, createdBy) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    refund.CreatedByUser = createdBy;
                    return refund;
                }, splitOn: "Id,Id,Id,Id");

            return result;
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundsPendingSecondCheckAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*, cb.*, fc.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                LEFT JOIN Users cb ON rr.CreatedBy = cb.Id
                LEFT JOIN Users fc ON rr.FirstCheckerId = fc.Id
                WHERE rr.WorkflowStatus = 'SubmittedForSecondCheck'
                ORDER BY rr.RequestedAt ASC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, User, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user, createdBy, firstChecker) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    refund.CreatedByUser = createdBy;
                    refund.FirstChecker = firstChecker;
                    return refund;
                }, splitOn: "Id,Id,Id,Id,Id");

            return result;
        }

        public async Task<IEnumerable<RefundRequest>> GetRefundsReadyForProcessingAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT rr.*, tb.*, pl.*, u.*, cb.*, fc.*, sc.*
                FROM RefundRequests rr
                LEFT JOIN TenderBids tb ON rr.TenderBidId = tb.Id
                LEFT JOIN PaymentLinks pl ON rr.PaymentLinkId = pl.Id
                LEFT JOIN Users u ON rr.ProcessedBy = u.Id
                LEFT JOIN Users cb ON rr.CreatedBy = cb.Id
                LEFT JOIN Users fc ON rr.FirstCheckerId = fc.Id
                LEFT JOIN Users sc ON rr.SecondCheckerId = sc.Id
                WHERE rr.WorkflowStatus = 'ReadyForProcessing'
                ORDER BY rr.RequestedAt ASC";

            var result = await connection.QueryAsync<RefundRequest, TenderBid, PaymentLink, User, User, User, User, RefundRequest>(
                sql, (refund, tenderBid, paymentLink, user, createdBy, firstChecker, secondChecker) =>
                {
                    refund.TenderBid = tenderBid;
                    refund.PaymentLink = paymentLink;
                    refund.ProcessedByUser = user;
                    refund.CreatedByUser = createdBy;
                    refund.FirstChecker = firstChecker;
                    refund.SecondChecker = secondChecker;
                    return refund;
                }, splitOn: "Id,Id,Id,Id,Id,Id");

            return result;
        }

        public async Task<object> GetCheckerMakerStatisticsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalRefunds,
                    SUM(CASE WHEN WorkflowStatus = 'Draft' THEN 1 ELSE 0 END) as DraftCount,
                    SUM(CASE WHEN WorkflowStatus = 'SubmittedForFirstCheck' THEN 1 ELSE 0 END) as PendingFirstCheckCount,
                    SUM(CASE WHEN WorkflowStatus = 'FirstCheckApproved' THEN 1 ELSE 0 END) as FirstCheckApprovedCount,
                    SUM(CASE WHEN WorkflowStatus = 'FirstCheckRejected' THEN 1 ELSE 0 END) as FirstCheckRejectedCount,
                    SUM(CASE WHEN WorkflowStatus = 'SubmittedForSecondCheck' THEN 1 ELSE 0 END) as PendingSecondCheckCount,
                    SUM(CASE WHEN WorkflowStatus = 'SecondCheckApproved' THEN 1 ELSE 0 END) as SecondCheckApprovedCount,
                    SUM(CASE WHEN WorkflowStatus = 'SecondCheckRejected' THEN 1 ELSE 0 END) as SecondCheckRejectedCount,
                    SUM(CASE WHEN WorkflowStatus = 'ReadyForProcessing' THEN 1 ELSE 0 END) as ReadyForProcessingCount,
                    SUM(CASE WHEN WorkflowStatus = 'Processing' THEN 1 ELSE 0 END) as ProcessingCount,
                    SUM(CASE WHEN WorkflowStatus = 'Completed' THEN 1 ELSE 0 END) as CompletedCount,
                    SUM(CASE WHEN WorkflowStatus = 'Failed' THEN 1 ELSE 0 END) as FailedCount
                FROM RefundRequests";

            return await connection.QuerySingleAsync(sql);
        }

        #endregion
    }
}
