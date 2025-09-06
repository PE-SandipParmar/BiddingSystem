using Dapper;
using BiddingSystem.Models;
using System.Data;
using System.Data.SqlClient;

namespace BiddingSystem.Data
{
    public class TenderRepository : ITenderRepository
    {
        private readonly string _connectionString;

        public TenderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        #region Basic CRUD Operations

        public async Task<Tender?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                WHERE t.Id = @Id AND t.IsActive = 1";

            var tender = await connection.QueryFirstOrDefaultAsync<Tender>(sql, new { Id = id });
            return tender;
        }

        public async Task<Tender?> GetByTenderIdAsync(string tenderId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                WHERE t.TenderId = @TenderId AND t.IsActive = 1";

            var tender = await connection.QueryFirstOrDefaultAsync<Tender>(sql, new { TenderId = tenderId });
            return tender;
        }

        public async Task<List<Tender>> GetAllAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                WHERE t.IsActive = 1
                ORDER BY t.CreatedAt DESC";

            var tenders = await connection.QueryAsync<Tender>(sql);
            return tenders.ToList();
        }

        public async Task<int> CreateAsync(Tender tender)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO Tenders (TenderId, TenderTitle, Description, Department, PublishDate, 
                                   EmdAmount, SdAmount, ProcessingFee, EstimatedValue, LastDateEmd,
                                   TenderClosingDate, TenderOpeningDate, Status, CreatedBy, CreatedAt, IsActive)
                VALUES (@TenderId, @TenderTitle, @Description, @Department, @PublishDate,
                        @EmdAmount, @SdAmount, @ProcessingFee, @EstimatedValue, @LastDateEmd,
                        @TenderClosingDate, @TenderOpeningDate, @Status, @CreatedBy, @CreatedAt, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.QuerySingleAsync<int>(sql, tender);
        }

        public async Task<bool> UpdateAsync(Tender tender)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Tenders 
                SET TenderId = @TenderId, TenderTitle = @TenderTitle, Description = @Description,
                    Department = @Department, PublishDate = @PublishDate, EmdAmount = @EmdAmount,
                    SdAmount = @SdAmount, ProcessingFee = @ProcessingFee, EstimatedValue = @EstimatedValue,
                    LastDateEmd = @LastDateEmd, TenderClosingDate = @TenderClosingDate,
                    TenderOpeningDate = @TenderOpeningDate, Status = @Status, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            tender.UpdatedAt = DateTime.UtcNow;
            var rowsAffected = await connection.ExecuteAsync(sql, tender);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Tenders 
                SET IsActive = 0, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(string tenderId, int? excludeId = null)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT COUNT(1) 
                FROM Tenders 
                WHERE TenderId = @TenderId AND IsActive = 1";

            var parameters = new DynamicParameters();
            parameters.Add("TenderId", tenderId);
            
            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
                parameters.Add("ExcludeId", excludeId.Value);
            }

            var count = await connection.QuerySingleAsync<int>(sql, parameters);
            return count > 0;
        }

        #endregion

        #region Tender Status Operations

        public async Task<bool> PublishTenderAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Tenders 
                SET Status = @Status, PublishedAt = @PublishedAt, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Status = TenderStatus.Published,
                PublishedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<bool> CloseTenderAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Tenders 
                SET Status = @Status, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Status = TenderStatus.Closed,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<bool> CancelTenderAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Tenders 
                SET Status = @Status, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Status = TenderStatus.Cancelled,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, TenderStatus status)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Tenders 
                SET Status = @Status, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        #endregion

        #region Search and Filtering

        public async Task<List<Tender>> SearchTendersAsync(string searchTerm, TenderStatus? status = null, string? department = null)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                WHERE t.IsActive = 1";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                sql += " AND (t.TenderId LIKE @SearchTerm OR t.TenderTitle LIKE @SearchTerm OR t.Description LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (status.HasValue)
            {
                sql += " AND t.Status = @Status";
                parameters.Add("Status", status.Value);
            }

            if (!string.IsNullOrEmpty(department))
            {
                sql += " AND t.Department = @Department";
                parameters.Add("Department", department);
            }

            sql += " ORDER BY t.CreatedAt DESC";

            var tenders = await connection.QueryAsync<Tender>(sql, parameters);
            return tenders.ToList();
        }

        public async Task<(List<Tender> Tenders, int TotalCount)> GetPagedTendersAsync(int page, int pageSize, string? searchTerm = null, TenderStatus? status = null, string? department = null)
        {
            using var connection = CreateConnection();

            var whereClause = "WHERE t.IsActive = 1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += " AND (t.TenderId LIKE @SearchTerm OR t.TenderTitle LIKE @SearchTerm OR t.Description LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (status.HasValue)
            {
                whereClause += " AND t.Status = @Status";
                parameters.Add("Status", status.Value);
            }

            if (!string.IsNullOrEmpty(department))
            {
                whereClause += " AND t.Department = @Department";
                parameters.Add("Department", department);
            }

            // Get total count
            var countSql = $"SELECT COUNT(1) FROM Tenders t {whereClause}";
            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

            // Get paged tenders
            var offset = (page - 1) * pageSize;
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var tendersSql = $@"
                SELECT t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                {whereClause}
                ORDER BY t.CreatedAt DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var tenders = await connection.QueryAsync<Tender>(tendersSql, parameters);
            return (tenders.ToList(), totalCount);
        }

        #endregion

        #region Statistics and Analytics

        public async Task<int> GetTotalTendersCountAsync()
        {
            using var connection = CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Tenders WHERE IsActive = 1";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<int> GetActiveTendersCountAsync()
        {
            using var connection = CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Tenders WHERE Status = @Status AND IsActive = 1";
            return await connection.QuerySingleAsync<int>(sql, new { Status = TenderStatus.Published });
        }

        public async Task<Dictionary<TenderStatus, int>> GetTenderCountByStatusAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Status, COUNT(1) as Count
                FROM Tenders 
                WHERE IsActive = 1
                GROUP BY Status";

            var result = await connection.QueryAsync<(TenderStatus Status, int Count)>(sql);
            return result.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<List<Tender>> GetRecentTendersAsync(int days = 7, int limit = 10)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT TOP(@Limit) t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                WHERE t.CreatedAt >= @FromDate AND t.IsActive = 1
                ORDER BY t.CreatedAt DESC";

            var fromDate = DateTime.UtcNow.AddDays(-days);
            var tenders = await connection.QueryAsync<Tender>(sql, new { Limit = limit, FromDate = fromDate });
            return tenders.ToList();
        }

        public async Task<List<Tender>> GetTendersByDepartmentAsync(string department)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                WHERE t.Department = @Department AND t.IsActive = 1
                ORDER BY t.CreatedAt DESC";

            var tenders = await connection.QueryAsync<Tender>(sql, new { Department = department });
            return tenders.ToList();
        }

        #endregion

        #region Tender Documents

        public async Task<List<TenderDocument>> GetTenderDocumentsAsync(int tenderId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT td.*, u.FirstName, u.LastName, u.Email as UploadedByEmail
                FROM TenderDocuments td
                LEFT JOIN Users u ON td.UploadedBy = u.Id
                WHERE td.TenderId = @TenderId AND td.IsActive = 1
                ORDER BY td.UploadedAt DESC";

            var documents = await connection.QueryAsync<TenderDocument>(sql, new { TenderId = tenderId });
            return documents.ToList();
        }

        public async Task<TenderDocument?> GetTenderDocumentByIdAsync(int documentId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT td.*, u.FirstName, u.LastName, u.Email as UploadedByEmail
                FROM TenderDocuments td
                LEFT JOIN Users u ON td.UploadedBy = u.Id
                WHERE td.Id = @Id AND td.IsActive = 1";

            var document = await connection.QueryFirstOrDefaultAsync<TenderDocument>(sql, new { Id = documentId });
            return document;
        }

        public async Task<int> AddTenderDocumentAsync(TenderDocument document)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO TenderDocuments (TenderId, DocumentName, FileName, FilePath, FileSize, 
                                           ContentType, DocumentType, IsRequired, UploadedBy, UploadedAt, IsActive)
                VALUES (@TenderId, @DocumentName, @FileName, @FilePath, @FileSize,
                        @ContentType, @DocumentType, @IsRequired, @UploadedBy, @UploadedAt, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.QuerySingleAsync<int>(sql, document);
        }

        public async Task<bool> DeleteTenderDocumentAsync(int documentId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE TenderDocuments 
                SET IsActive = 0
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = documentId });
            return rowsAffected > 0;
        }

        #endregion

        #region Tender Bids

        public async Task<List<TenderBid>> GetTenderBidsAsync(int tenderId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM TenderBids 
                WHERE TenderId = @TenderId AND IsActive = 1
                ORDER BY SubmittedAt DESC";

            var bids = await connection.QueryAsync<TenderBid>(sql, new { TenderId = tenderId });
            return bids.ToList();
        }

        public async Task<TenderBid?> GetTenderBidByIdAsync(int bidId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM TenderBids 
                WHERE Id = @Id AND IsActive = 1";

            var bid = await connection.QueryFirstOrDefaultAsync<TenderBid>(sql, new { Id = bidId });
            return bid;
        }

        public async Task<int> AddTenderBidAsync(TenderBid bid)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO TenderBids (TenderId, BidderName, BidderEmail, BidderPhone, CompanyName, 
                                      CompanyAddress, BidAmount, EmdAmount, ProcessingFee, TotalAmount,
                                      Status, PaymentStatus, PaymentReference, PaymentDate, SubmittedAt, IsActive)
                VALUES (@TenderId, @BidderName, @BidderEmail, @BidderPhone, @CompanyName,
                        @CompanyAddress, @BidAmount, @EmdAmount, @ProcessingFee, @TotalAmount,
                        @Status, @PaymentStatus, @PaymentReference, @PaymentDate, @SubmittedAt, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.QuerySingleAsync<int>(sql, bid);
        }

        public async Task<bool> UpdateTenderBidAsync(TenderBid bid)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE TenderBids 
                SET BidderName = @BidderName, BidderEmail = @BidderEmail, BidderPhone = @BidderPhone,
                    CompanyName = @CompanyName, CompanyAddress = @CompanyAddress, BidAmount = @BidAmount,
                    EmdAmount = @EmdAmount, ProcessingFee = @ProcessingFee, TotalAmount = @TotalAmount,
                    Status = @Status, PaymentStatus = @PaymentStatus, PaymentReference = @PaymentReference,
                    PaymentDate = @PaymentDate
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, bid);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteTenderBidAsync(int bidId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE TenderBids 
                SET IsActive = 0
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = bidId });
            return rowsAffected > 0;
        }

        public async Task<List<TenderBid>> GetBidsByStatusAsync(string status)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM TenderBids 
                WHERE Status = @Status AND IsActive = 1
                ORDER BY SubmittedAt DESC";

            var bids = await connection.QueryAsync<TenderBid>(sql, new { Status = status });
            return bids.ToList();
        }

        public async Task<List<TenderBid>> GetBidsByPaymentStatusAsync(string paymentStatus)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM TenderBids 
                WHERE PaymentStatus = @PaymentStatus AND IsActive = 1
                ORDER BY SubmittedAt DESC";

            var bids = await connection.QueryAsync<TenderBid>(sql, new { PaymentStatus = paymentStatus });
            return bids.ToList();
        }

        #endregion

        #region Payment Operations

        public async Task<bool> UpdateBidPaymentStatusAsync(int bidId, string status, string? paymentReference = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE TenderBids 
                SET PaymentStatus = @PaymentStatus, PaymentReference = @PaymentReference, 
                    PaymentDate = @PaymentDate
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = bidId,
                PaymentStatus = status,
                PaymentReference = paymentReference,
                PaymentDate = status == "Paid" ? DateTime.UtcNow : (DateTime?)null
            });
            return rowsAffected > 0;
        }

        public async Task<List<TenderBid>> GetPendingPaymentsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM TenderBids 
                WHERE PaymentStatus = @PaymentStatus AND IsActive = 1
                ORDER BY SubmittedAt ASC";

            var bids = await connection.QueryAsync<TenderBid>(sql, new { PaymentStatus = "Pending" });
            return bids.ToList();
        }

        public async Task<decimal> GetTotalEmdCollectedAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT ISNULL(SUM(EmdAmount), 0) 
                FROM TenderBids 
                WHERE PaymentStatus = @PaymentStatus AND IsActive = 1";

            return await connection.QuerySingleAsync<decimal>(sql, new { PaymentStatus = "Paid" });
        }

        public async Task<decimal> GetTotalProcessingFeesCollectedAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT ISNULL(SUM(ProcessingFee), 0) 
                FROM TenderBids 
                WHERE PaymentStatus = @PaymentStatus AND IsActive = 1";

            return await connection.QuerySingleAsync<decimal>(sql, new { PaymentStatus = "Paid" });
        }

        #endregion

        #region Dashboard Statistics

        public async Task<Dictionary<string, object>> GetDashboardStatisticsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT 
                    (SELECT COUNT(1) FROM Tenders WHERE IsActive = 1) as TotalTenders,
                    (SELECT COUNT(1) FROM Tenders WHERE Status = 2 AND IsActive = 1) as PublishedTenders,
                    (SELECT COUNT(1) FROM TenderBids WHERE IsActive = 1) as TotalBids,
                    (SELECT COUNT(1) FROM TenderBids WHERE PaymentStatus = 'Paid' AND IsActive = 1) as PaidBids,
                    (SELECT ISNULL(SUM(EmdAmount), 0) FROM TenderBids WHERE PaymentStatus = 'Paid' AND IsActive = 1) as TotalEmdCollected,
                    (SELECT ISNULL(SUM(ProcessingFee), 0) FROM TenderBids WHERE PaymentStatus = 'Paid' AND IsActive = 1) as TotalProcessingFeesCollected";

            var result = await connection.QueryFirstOrDefaultAsync(sql);
            return result?.ToDictionary() ?? new Dictionary<string, object>();
        }

        public async Task<List<Tender>> GetExpiringTendersAsync(int days = 7)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                WHERE t.LastDateEmd <= @ExpiryDate AND t.Status = @Status AND t.IsActive = 1
                ORDER BY t.LastDateEmd ASC";

            var expiryDate = DateTime.UtcNow.AddDays(days);
            var tenders = await connection.QueryAsync<Tender>(sql, new { ExpiryDate = expiryDate, Status = TenderStatus.Published });
            return tenders.ToList();
        }

        public async Task<List<Tender>> GetUpcomingTendersAsync(int days = 7)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT t.*, u.FirstName, u.LastName, u.Email as CreatedByEmail
                FROM Tenders t
                LEFT JOIN Users u ON t.CreatedBy = u.Id
                WHERE t.PublishDate <= @PublishDate AND t.Status = @Status AND t.IsActive = 1
                ORDER BY t.PublishDate ASC";

            var publishDate = DateTime.UtcNow.AddDays(days);
            var tenders = await connection.QueryAsync<Tender>(sql, new { PublishDate = publishDate, Status = TenderStatus.Draft });
            return tenders.ToList();
        }

        #endregion
    }
}
