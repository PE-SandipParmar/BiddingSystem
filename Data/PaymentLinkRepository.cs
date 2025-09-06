using Dapper;
using BiddingSystem.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BiddingSystem.Data
{
    public class PaymentLinkRepository : IPaymentLinkRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<PaymentLinkRepository> _logger;

        public PaymentLinkRepository(IConfiguration configuration, ILogger<PaymentLinkRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<PaymentLink?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT pl.*, t.TenderTitle, t.TenderId as TenderIdString,
                           cb.Username as CreatedByUserName, ub.Username as UsedByUserName
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    INNER JOIN Users cb ON pl.CreatedBy = cb.Id
                    LEFT JOIN Users ub ON pl.UsedBy = ub.Id
                    WHERE pl.Id = @Id AND pl.IsActive = 1";

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
                return result != null ? MapToPaymentLink(result) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment link by ID: {Id}", id);
                throw;
            }
        }

        public async Task<PaymentLink?> GetByLinkIdAsync(string linkId)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT pl.*, t.TenderTitle, t.TenderId as TenderIdString,
                           cb.Username as CreatedByUserName, ub.Username as UsedByUserName
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    INNER JOIN Users cb ON pl.CreatedBy = cb.Id
                    LEFT JOIN Users ub ON pl.UsedBy = ub.Id
                    WHERE pl.LinkId = @LinkId AND pl.IsActive = 1";

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { LinkId = linkId });
                return result != null ? MapToPaymentLink(result) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment link by LinkId: {LinkId}", linkId);
                throw;
            }
        }

        public async Task<PaymentLink?> GetBySecurityTokenAsync(string token)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT pl.*, t.TenderTitle, t.TenderId as TenderIdString,
                           cb.Username as CreatedByUserName, ub.Username as UsedByUserName
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    INNER JOIN Users cb ON pl.CreatedBy = cb.Id
                    LEFT JOIN Users ub ON pl.UsedBy = ub.Id
                    WHERE pl.SecurityToken = @Token AND pl.IsActive = 1";

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Token = token });
                return result != null ? MapToPaymentLink(result) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment link by security token");
                throw;
            }
        }

        public async Task<List<PaymentLink>> GetAllAsync()
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT pl.*, t.TenderTitle, t.TenderId as TenderIdString,
                           cb.Username as CreatedByUserName, ub.Username as UsedByUserName
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    INNER JOIN Users cb ON pl.CreatedBy = cb.Id
                    LEFT JOIN Users ub ON pl.UsedBy = ub.Id
                    WHERE pl.IsActive = 1
                    ORDER BY pl.CreatedDate DESC";

                var results = await connection.QueryAsync<dynamic>(sql);
                return results.Select(MapToPaymentLink).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payment links");
                throw;
            }
        }

        public async Task<List<PaymentLink>> GetByTenderIdAsync(int tenderId)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT pl.*, t.TenderTitle, t.TenderId as TenderIdString,
                           cb.Username as CreatedByUserName, ub.Username as UsedByUserName
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    INNER JOIN Users cb ON pl.CreatedBy = cb.Id
                    LEFT JOIN Users ub ON pl.UsedBy = ub.Id
                    WHERE pl.TenderId = @TenderId AND pl.IsActive = 1
                    ORDER BY pl.CreatedDate DESC";

                var results = await connection.QueryAsync<dynamic>(sql, new { TenderId = tenderId });
                return results.Select(MapToPaymentLink).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment links by tender ID: {TenderId}", tenderId);
                throw;
            }
        }

        public async Task<List<PaymentLink>> GetByUserIdAsync(int userId)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT pl.*, t.TenderTitle, t.TenderId as TenderIdString,
                           cb.Username as CreatedByUserName, ub.Username as UsedByUserName
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    INNER JOIN Users cb ON pl.CreatedBy = cb.Id
                    LEFT JOIN Users ub ON pl.UsedBy = ub.Id
                    WHERE pl.CreatedBy = @UserId AND pl.IsActive = 1
                    ORDER BY pl.CreatedDate DESC";

                var results = await connection.QueryAsync<dynamic>(sql, new { UserId = userId });
                return results.Select(MapToPaymentLink).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment links by user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<PaymentLink> CreateAsync(PaymentLink paymentLink)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    INSERT INTO PaymentLinks (LinkId, TenderId, Amount, PaymentType, PaymentUrl, SecurityToken, 
                                            Status, CreatedDate, ExpiryDate, CreatedBy, IsActive, Notes)
                    VALUES (@LinkId, @TenderId, @Amount, @PaymentType, @PaymentUrl, @SecurityToken, 
                            @Status, @CreatedDate, @ExpiryDate, @CreatedBy, @IsActive, @Notes);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var id = await connection.QuerySingleAsync<int>(sql, new
                {
                    paymentLink.LinkId,
                    paymentLink.TenderId,
                    paymentLink.Amount,
                    PaymentType = (int)paymentLink.PaymentType,
                    paymentLink.PaymentUrl,
                    paymentLink.SecurityToken,
                    Status = (int)paymentLink.Status,
                    paymentLink.CreatedDate,
                    paymentLink.ExpiryDate,
                    paymentLink.CreatedBy,
                    paymentLink.IsActive,
                    paymentLink.Notes
                });

                paymentLink.Id = id;
                return paymentLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment link");
                throw;
            }
        }

        public async Task<PaymentLink> UpdateAsync(PaymentLink paymentLink)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    UPDATE PaymentLinks 
                    SET Amount = @Amount, PaymentType = @PaymentType, PaymentUrl = @PaymentUrl,
                        Status = @Status, ExpiryDate = @ExpiryDate, UsedDate = @UsedDate,
                        UsedBy = @UsedBy, TransactionId = @TransactionId, Notes = @Notes
                    WHERE Id = @Id AND IsActive = 1";

                await connection.ExecuteAsync(sql, new
                {
                    paymentLink.Id,
                    paymentLink.Amount,
                    PaymentType = (int)paymentLink.PaymentType,
                    paymentLink.PaymentUrl,
                    Status = (int)paymentLink.Status,
                    paymentLink.ExpiryDate,
                    paymentLink.UsedDate,
                    paymentLink.UsedBy,
                    paymentLink.TransactionId,
                    paymentLink.Notes
                });

                return paymentLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment link: {Id}", paymentLink.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "UPDATE PaymentLinks SET IsActive = 0 WHERE Id = @Id";
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment link: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string linkId, int? excludeId = null)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "SELECT COUNT(1) FROM PaymentLinks WHERE LinkId = @LinkId AND IsActive = 1";
                object parameters;

                if (excludeId.HasValue)
                {
                    sql += " AND Id != @ExcludeId";
                    parameters = new { LinkId = linkId, ExcludeId = excludeId.Value };
                }
                else
                {
                    parameters = new { LinkId = linkId };
                }

                var count = await connection.QuerySingleAsync<int>(sql, parameters);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if payment link exists: {LinkId}", linkId);
                throw;
            }
        }

        public async Task<bool> TransactionIdExistsAsync(string transactionId)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = "SELECT COUNT(1) FROM PaymentLinks WHERE TransactionId = @TransactionId AND IsActive = 1";
                var count = await connection.QuerySingleAsync<int>(sql, new { TransactionId = transactionId });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if transaction ID exists: {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task<List<PaymentLink>> SearchAsync(string searchTerm, PaymentLinkStatus? status, 
            PaymentType? paymentType, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 25)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT pl.*, t.TenderTitle, t.TenderId as TenderIdString,
                           cb.Username as CreatedByUserName, ub.Username as UsedByUserName
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    INNER JOIN Users cb ON pl.CreatedBy = cb.Id
                    LEFT JOIN Users ub ON pl.UsedBy = ub.Id
                    WHERE pl.IsActive = 1";

                var parameters = new DynamicParameters();
                var conditions = new List<string>();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    conditions.Add("(pl.LinkId LIKE @SearchTerm OR t.TenderTitle LIKE @SearchTerm OR t.TenderId LIKE @SearchTerm)");
                    parameters.Add("SearchTerm", $"%{searchTerm}%");
                }

                if (status.HasValue)
                {
                    conditions.Add("pl.Status = @Status");
                    parameters.Add("Status", (int)status.Value);
                }

                if (paymentType.HasValue)
                {
                    conditions.Add("pl.PaymentType = @PaymentType");
                    parameters.Add("PaymentType", (int)paymentType.Value);
                }

                if (fromDate.HasValue)
                {
                    conditions.Add("pl.CreatedDate >= @FromDate");
                    parameters.Add("FromDate", fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    conditions.Add("pl.CreatedDate <= @ToDate");
                    parameters.Add("ToDate", toDate.Value);
                }

                if (conditions.Any())
                {
                    sql += " AND " + string.Join(" AND ", conditions);
                }

                sql += " ORDER BY pl.CreatedDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", (page - 1) * pageSize);
                parameters.Add("PageSize", pageSize);

                var results = await connection.QueryAsync<dynamic>(sql, parameters);
                return results.Select(MapToPaymentLink).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching payment links");
                throw;
            }
        }

        public async Task<int> GetSearchCountAsync(string searchTerm, PaymentLinkStatus? status, 
            PaymentType? paymentType, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT COUNT(1)
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    WHERE pl.IsActive = 1";

                var parameters = new DynamicParameters();
                var conditions = new List<string>();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    conditions.Add("(pl.LinkId LIKE @SearchTerm OR t.TenderTitle LIKE @SearchTerm OR t.TenderId LIKE @SearchTerm)");
                    parameters.Add("SearchTerm", $"%{searchTerm}%");
                }

                if (status.HasValue)
                {
                    conditions.Add("pl.Status = @Status");
                    parameters.Add("Status", (int)status.Value);
                }

                if (paymentType.HasValue)
                {
                    conditions.Add("pl.PaymentType = @PaymentType");
                    parameters.Add("PaymentType", (int)paymentType.Value);
                }

                if (fromDate.HasValue)
                {
                    conditions.Add("pl.CreatedDate >= @FromDate");
                    parameters.Add("FromDate", fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    conditions.Add("pl.CreatedDate <= @ToDate");
                    parameters.Add("ToDate", toDate.Value);
                }

                if (conditions.Any())
                {
                    sql += " AND " + string.Join(" AND ", conditions);
                }

                return await connection.QuerySingleAsync<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search count for payment links");
                throw;
            }
        }

        public async Task<bool> MarkAsUsedAsync(int id, int? userId, string transactionId)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    UPDATE PaymentLinks 
                    SET Status = @Status, UsedDate = @UsedDate, UsedBy = @UsedBy, TransactionId = @TransactionId
                    WHERE Id = @Id AND IsActive = 1 AND Status = @ActiveStatus";

                var rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    Status = (int)PaymentLinkStatus.Used,
                    UsedDate = DateTime.UtcNow,
                    UsedBy = userId,
                    TransactionId = transactionId,
                    ActiveStatus = (int)PaymentLinkStatus.Active
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment link as used: {Id}", id);
                throw;
            }
        }

        public async Task<bool> MarkAsExpiredAsync(int id)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    UPDATE PaymentLinks 
                    SET Status = @Status
                    WHERE Id = @Id AND IsActive = 1 AND Status = @ActiveStatus";

                var rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    Status = (int)PaymentLinkStatus.Expired,
                    ActiveStatus = (int)PaymentLinkStatus.Active
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment link as expired: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CancelAsync(int id, string reason)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    UPDATE PaymentLinks 
                    SET Status = @Status, Notes = @Notes
                    WHERE Id = @Id AND IsActive = 1";

                var rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    Status = (int)PaymentLinkStatus.Cancelled,
                    Notes = reason
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment link: {Id}", id);
                throw;
            }
        }

        public async Task<List<PaymentLink>> GetExpiredLinksAsync()
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT pl.*, t.TenderTitle, t.TenderId as TenderIdString,
                           cb.Username as CreatedByUserName, ub.Username as UsedByUserName
                    FROM PaymentLinks pl
                    INNER JOIN Tenders t ON pl.TenderId = t.Id
                    INNER JOIN Users cb ON pl.CreatedBy = cb.Id
                    LEFT JOIN Users ub ON pl.UsedBy = ub.Id
                    WHERE pl.IsActive = 1 AND pl.Status = @ActiveStatus AND pl.ExpiryDate < @Now";

                var results = await connection.QueryAsync<dynamic>(sql, new
                {
                    ActiveStatus = (int)PaymentLinkStatus.Active,
                    Now = DateTime.UtcNow
                });

                return results.Select(MapToPaymentLink).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired payment links");
                throw;
            }
        }

        public async Task<bool> UpdateExpiredStatusAsync()
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    UPDATE PaymentLinks 
                    SET Status = @ExpiredStatus
                    WHERE IsActive = 1 AND Status = @ActiveStatus AND ExpiryDate < @Now";

                var rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    ExpiredStatus = (int)PaymentLinkStatus.Expired,
                    ActiveStatus = (int)PaymentLinkStatus.Active,
                    Now = DateTime.UtcNow
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expired payment link statuses");
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT COUNT(1) 
                    FROM PaymentLinks 
                    WHERE SecurityToken = @Token AND IsActive = 1 AND Status = @ActiveStatus AND ExpiryDate > @Now";

                var count = await connection.QuerySingleAsync<int>(sql, new
                {
                    Token = token,
                    ActiveStatus = (int)PaymentLinkStatus.Active,
                    Now = DateTime.UtcNow
                });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment link token");
                throw;
            }
        }

        public async Task<string> GenerateUniqueLinkIdAsync()
        {
            string linkId;
            do
            {
                linkId = PaymentLink.GenerateLinkId();
            } while (await ExistsAsync(linkId));

            return linkId;
        }

        public async Task<bool> IsLinkUsableAsync(int id)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT COUNT(1) 
                    FROM PaymentLinks 
                    WHERE Id = @Id AND IsActive = 1 AND Status = @ActiveStatus AND ExpiryDate > @Now";

                var count = await connection.QuerySingleAsync<int>(sql, new
                {
                    Id = id,
                    ActiveStatus = (int)PaymentLinkStatus.Active,
                    Now = DateTime.UtcNow
                });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if payment link is usable: {Id}", id);
                throw;
            }
        }

        public async Task<Dictionary<PaymentLinkStatus, int>> GetStatusCountsAsync()
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT Status, COUNT(1) as Count
                    FROM PaymentLinks 
                    WHERE IsActive = 1
                    GROUP BY Status";

                var results = await connection.QueryAsync<dynamic>(sql);
                var counts = new Dictionary<PaymentLinkStatus, int>();

                foreach (var result in results)
                {
                    var status = (PaymentLinkStatus)result.Status;
                    counts[status] = result.Count;
                }

                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment link status counts");
                throw;
            }
        }

        public async Task<decimal> GetTotalAmountByStatusAsync(PaymentLinkStatus status)
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT ISNULL(SUM(Amount), 0)
                    FROM PaymentLinks 
                    WHERE IsActive = 1 AND Status = @Status";

                return await connection.QuerySingleAsync<decimal>(sql, new { Status = (int)status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total amount by status: {Status}", status);
                throw;
            }
        }

        public async Task<int> GetActiveLinksCountAsync()
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT COUNT(1) 
                    FROM PaymentLinks 
                    WHERE IsActive = 1 AND Status = @ActiveStatus AND ExpiryDate > @Now";

                return await connection.QuerySingleAsync<int>(sql, new
                {
                    ActiveStatus = (int)PaymentLinkStatus.Active,
                    Now = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active payment links count");
                throw;
            }
        }

        public async Task<int> GetExpiredLinksCountAsync()
        {
            try
            {
                using var connection = CreateConnection();
                var sql = @"
                    SELECT COUNT(1) 
                    FROM PaymentLinks 
                    WHERE IsActive = 1 AND Status = @ExpiredStatus";

                return await connection.QuerySingleAsync<int>(sql, new
                {
                    ExpiredStatus = (int)PaymentLinkStatus.Expired
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired payment links count");
                throw;
            }
        }

        private static PaymentLink MapToPaymentLink(dynamic result)
        {
            return new PaymentLink
            {
                Id = result.Id,
                LinkId = result.LinkId,
                TenderId = result.TenderId,
                Amount = result.Amount,
                PaymentType = (PaymentType)result.PaymentType,
                PaymentUrl = result.PaymentUrl,
                SecurityToken = result.SecurityToken,
                Status = (PaymentLinkStatus)result.Status,
                CreatedDate = result.CreatedDate,
                ExpiryDate = result.ExpiryDate,
                UsedDate = result.UsedDate,
                UsedBy = result.UsedBy,
                TransactionId = result.TransactionId,
                Notes = result.Notes,
                CreatedBy = result.CreatedBy,
                IsActive = result.IsActive,
                Tender = new Tender { Id = result.TenderId, TenderTitle = result.TenderTitle, TenderId = result.TenderIdString },
                CreatedByUser = new User { Username = result.CreatedByUserName },
                UsedByUser = result.UsedByUserName != null ? new User { Username = result.UsedByUserName } : null
            };
        }
    }
}
