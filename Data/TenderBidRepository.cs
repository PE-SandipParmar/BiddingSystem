using Dapper;
using BiddingSystem.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace BiddingSystem.Data
{
    public class TenderBidRepository : ITenderBidRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TenderBidRepository> _logger;

        public TenderBidRepository(IConfiguration configuration, ILogger<TenderBidRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<TenderBid>> GetAllBidsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT tb.*, t.*
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                WHERE tb.IsActive = 1
                ORDER BY tb.SubmittedAt DESC";

            var bids = await connection.QueryAsync<TenderBid, Tender, TenderBid>(sql, (bid, tender) =>
            {
                bid.Tender = tender;
                return bid;
            }, splitOn: "Id");

            return bids;
        }

        public async Task<TenderBid?> GetBidByIdAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT tb.*, t.*
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                WHERE tb.Id = @Id AND tb.IsActive = 1";

            var result = await connection.QueryAsync<TenderBid, Tender, TenderBid>(sql, (bid, tender) =>
            {
                bid.Tender = tender;
                return bid;
            }, new { Id = id }, splitOn: "Id");

            return result.FirstOrDefault();
        }

        public async Task<TenderBid?> GetBidByPaymentReferenceAsync(string paymentReference)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT tb.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                WHERE tb.PaymentReference = @PaymentReference AND tb.IsActive = 1";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { PaymentReference = paymentReference });
            if (result == null) return null;

            var bid = new TenderBid
            {
                Id = result.Id,
                TenderId = result.TenderId,
                BidderName = result.BidderName,
                BidderEmail = result.BidderEmail,
                BidderPhone = result.BidderPhone,
                CompanyName = result.CompanyName,
                CompanyAddress = result.CompanyAddress,
                BidAmount = result.BidAmount,
                EmdAmount = result.EmdAmount,
                ProcessingFee = result.ProcessingFee,
                TotalAmount = result.TotalAmount,
                Status = result.Status,
                PaymentStatus = result.PaymentStatus,
                PaymentReference = result.PaymentReference,
                PaymentDate = result.PaymentDate,
                SubmittedAt = result.SubmittedAt,
                IsActive = result.IsActive,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                Remarks = result.Remarks,
                Tender = result.Tender_Id != null ? new Tender
                {
                    Id = result.Tender_Id,
                    TenderId = result.Tender_TenderId,
                    TenderTitle = result.Tender_TenderTitle,
                    Description = result.Tender_Description,
                    Department = result.Tender_Department,
                    PublishDate = result.Tender_PublishDate,
                    EmdAmount = result.Tender_EmdAmount,
                    SdAmount = result.Tender_SdAmount,
                    ProcessingFee = result.Tender_ProcessingFee,
                    EstimatedValue = result.Tender_EstimatedValue,
                    LastDateEmd = result.Tender_LastDateEmd,
                    TenderClosingDate = result.Tender_TenderClosingDate,
                    TenderOpeningDate = result.Tender_TenderOpeningDate,
                    Status = result.Tender_Status,
                    CreatedBy = result.Tender_CreatedBy,
                    CreatedAt = result.Tender_CreatedAt,
                    UpdatedAt = result.Tender_UpdatedAt,
                    PublishedAt = result.Tender_PublishedAt,
                    IsActive = result.Tender_IsActive
                } : null
            };

            return bid;
        }

        public async Task<IEnumerable<TenderBid>> SearchBidsAsync(string? bidderName, string? companyName, string? status, string? paymentStatus)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT tb.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                WHERE tb.IsActive = 1";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(bidderName))
            {
                sql += " AND tb.BidderName LIKE @BidderName";
                parameters.Add("BidderName", $"%{bidderName}%");
            }

            if (!string.IsNullOrEmpty(companyName))
            {
                sql += " AND tb.CompanyName LIKE @CompanyName";
                parameters.Add("CompanyName", $"%{companyName}%");
            }

            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND tb.Status = @Status";
                parameters.Add("Status", status);
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                sql += " AND tb.PaymentStatus = @PaymentStatus";
                parameters.Add("PaymentStatus", paymentStatus);
            }

            sql += " ORDER BY tb.SubmittedAt DESC";

            var results = await connection.QueryAsync<dynamic>(sql, parameters);
            var bids = new List<TenderBid>();

            foreach (var result in results)
            {
                var bid = new TenderBid
                {
                    Id = result.Id,
                    TenderId = result.TenderId,
                    BidderName = result.BidderName,
                    BidderEmail = result.BidderEmail,
                    BidderPhone = result.BidderPhone,
                    CompanyName = result.CompanyName,
                    CompanyAddress = result.CompanyAddress,
                    BidAmount = result.BidAmount,
                    EmdAmount = result.EmdAmount,
                    ProcessingFee = result.ProcessingFee,
                    TotalAmount = result.TotalAmount,
                    Status = result.Status,
                    PaymentStatus = result.PaymentStatus,
                    PaymentReference = result.PaymentReference,
                    PaymentDate = result.PaymentDate,
                    SubmittedAt = result.SubmittedAt,
                    IsActive = result.IsActive,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt,
                    Remarks = result.Remarks,
                    Tender = result.Tender_Id != null ? new Tender
                    {
                        Id = result.Tender_Id,
                        TenderId = result.Tender_TenderId,
                        TenderTitle = result.Tender_TenderTitle,
                        Description = result.Tender_Description,
                        Department = result.Tender_Department,
                        PublishDate = result.Tender_PublishDate,
                        EmdAmount = result.Tender_EmdAmount,
                        SdAmount = result.Tender_SdAmount,
                        ProcessingFee = result.Tender_ProcessingFee,
                        EstimatedValue = result.Tender_EstimatedValue,
                        LastDateEmd = result.Tender_LastDateEmd,
                        TenderClosingDate = result.Tender_TenderClosingDate,
                        TenderOpeningDate = result.Tender_TenderOpeningDate,
                        Status = result.Tender_Status,
                        CreatedBy = result.Tender_CreatedBy,
                        CreatedAt = result.Tender_CreatedAt,
                        UpdatedAt = result.Tender_UpdatedAt,
                        PublishedAt = result.Tender_PublishedAt,
                        IsActive = result.Tender_IsActive
                    } : null
                };

                bids.Add(bid);
            }

            return bids;
        }

        public async Task<TenderBid> CreateBidAsync(TenderBid bid)
        {
            using var connection = CreateConnection();
            
            // Ensure Status and PaymentStatus are properly set as strings
            bid.Status = bid.Status ?? "Submitted";
            bid.PaymentStatus = bid.PaymentStatus ?? "Pending";
            bid.CreatedAt = DateTime.UtcNow;
            bid.SubmittedAt = DateTime.UtcNow;
            
            const string sql = @"
                INSERT INTO TenderBids 
                (TenderId, BidderName, BidderEmail, BidderPhone, CompanyName, CompanyAddress, 
                 BidAmount, EmdAmount, ProcessingFee, TotalAmount, Status, PaymentStatus, 
                 PaymentReference, PaymentDate, SubmittedAt, IsActive, CreatedAt, Remarks)
                VALUES 
                (@TenderId, @BidderName, @BidderEmail, @BidderPhone, @CompanyName, @CompanyAddress, 
                 @BidAmount, @EmdAmount, @ProcessingFee, @TotalAmount, @Status, @PaymentStatus, 
                 @PaymentReference, @PaymentDate, @SubmittedAt, @IsActive, @CreatedAt, @Remarks);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            try
            {
                var id = await connection.QuerySingleAsync<int>(sql, bid);
                bid.Id = id;
                return bid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tender bid. Status: {Status}, PaymentStatus: {PaymentStatus}", 
                    bid.Status, bid.PaymentStatus);
                throw;
            }
        }

        public async Task<TenderBid> UpdateBidAsync(TenderBid bid)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE TenderBids 
                SET TenderId = @TenderId, BidderName = @BidderName, BidderEmail = @BidderEmail, 
                    BidderPhone = @BidderPhone, CompanyName = @CompanyName, CompanyAddress = @CompanyAddress, 
                    BidAmount = @BidAmount, EmdAmount = @EmdAmount, ProcessingFee = @ProcessingFee, 
                    TotalAmount = @TotalAmount, Status = @Status, PaymentStatus = @PaymentStatus, 
                    PaymentReference = @PaymentReference, PaymentDate = @PaymentDate, 
                    SubmittedAt = @SubmittedAt, IsActive = @IsActive, UpdatedAt = @UpdatedAt, Remarks = @Remarks
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, bid);
            return bid;
        }

        public async Task<bool> DeleteBidAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = "UPDATE TenderBids SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<TenderBid>> GetBidsByTenderIdAsync(int tenderId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT tb.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                WHERE tb.TenderId = @TenderId AND tb.IsActive = 1
                ORDER BY tb.SubmittedAt DESC";

            var results = await connection.QueryAsync<dynamic>(sql, new { TenderId = tenderId });
            var bids = new List<TenderBid>();

            foreach (var result in results)
            {
                var bid = new TenderBid
                {
                    Id = result.Id,
                    TenderId = result.TenderId,
                    BidderName = result.BidderName,
                    BidderEmail = result.BidderEmail,
                    BidderPhone = result.BidderPhone,
                    CompanyName = result.CompanyName,
                    CompanyAddress = result.CompanyAddress,
                    BidAmount = result.BidAmount,
                    EmdAmount = result.EmdAmount,
                    ProcessingFee = result.ProcessingFee,
                    TotalAmount = result.TotalAmount,
                    Status = result.Status,
                    PaymentStatus = result.PaymentStatus,
                    PaymentReference = result.PaymentReference,
                    PaymentDate = result.PaymentDate,
                    SubmittedAt = result.SubmittedAt,
                    IsActive = result.IsActive,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt,
                    Remarks = result.Remarks,
                    Tender = result.Tender_Id != null ? new Tender
                    {
                        Id = result.Tender_Id,
                        TenderId = result.Tender_TenderId,
                        TenderTitle = result.Tender_TenderTitle,
                        Description = result.Tender_Description,
                        Department = result.Tender_Department,
                        PublishDate = result.Tender_PublishDate,
                        EmdAmount = result.Tender_EmdAmount,
                        SdAmount = result.Tender_SdAmount,
                        ProcessingFee = result.Tender_ProcessingFee,
                        EstimatedValue = result.Tender_EstimatedValue,
                        LastDateEmd = result.Tender_LastDateEmd,
                        TenderClosingDate = result.Tender_TenderClosingDate,
                        TenderOpeningDate = result.Tender_TenderOpeningDate,
                        Status = result.Tender_Status,
                        CreatedBy = result.Tender_CreatedBy,
                        CreatedAt = result.Tender_CreatedAt,
                        UpdatedAt = result.Tender_UpdatedAt,
                        PublishedAt = result.Tender_PublishedAt,
                        IsActive = result.Tender_IsActive
                    } : null
                };

                bids.Add(bid);
            }

            return bids;
        }

        public async Task<IEnumerable<TenderBid>> GetBidsByStatusAsync(string status)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT tb.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                WHERE tb.Status = @Status AND tb.IsActive = 1
                ORDER BY tb.SubmittedAt DESC";

            var results = await connection.QueryAsync<dynamic>(sql, new { Status = status });
            var bids = new List<TenderBid>();

            foreach (var result in results)
            {
                var bid = new TenderBid
                {
                    Id = result.Id,
                    TenderId = result.TenderId,
                    BidderName = result.BidderName,
                    BidderEmail = result.BidderEmail,
                    BidderPhone = result.BidderPhone,
                    CompanyName = result.CompanyName,
                    CompanyAddress = result.CompanyAddress,
                    BidAmount = result.BidAmount,
                    EmdAmount = result.EmdAmount,
                    ProcessingFee = result.ProcessingFee,
                    TotalAmount = result.TotalAmount,
                    Status = result.Status,
                    PaymentStatus = result.PaymentStatus,
                    PaymentReference = result.PaymentReference,
                    PaymentDate = result.PaymentDate,
                    SubmittedAt = result.SubmittedAt,
                    IsActive = result.IsActive,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt,
                    Remarks = result.Remarks,
                    Tender = result.Tender_Id != null ? new Tender
                    {
                        Id = result.Tender_Id,
                        TenderId = result.Tender_TenderId,
                        TenderTitle = result.Tender_TenderTitle,
                        Description = result.Tender_Description,
                        Department = result.Tender_Department,
                        PublishDate = result.Tender_PublishDate,
                        EmdAmount = result.Tender_EmdAmount,
                        SdAmount = result.Tender_SdAmount,
                        ProcessingFee = result.Tender_ProcessingFee,
                        EstimatedValue = result.Tender_EstimatedValue,
                        LastDateEmd = result.Tender_LastDateEmd,
                        TenderClosingDate = result.Tender_TenderClosingDate,
                        TenderOpeningDate = result.Tender_TenderOpeningDate,
                        Status = result.Tender_Status,
                        CreatedBy = result.Tender_CreatedBy,
                        CreatedAt = result.Tender_CreatedAt,
                        UpdatedAt = result.Tender_UpdatedAt,
                        PublishedAt = result.Tender_PublishedAt,
                        IsActive = result.Tender_IsActive
                    } : null
                };

                bids.Add(bid);
            }

            return bids;
        }

        public async Task<IEnumerable<TenderBid>> GetBidsByPaymentStatusAsync(string paymentStatus)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT tb.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                WHERE tb.PaymentStatus = @PaymentStatus AND tb.IsActive = 1
                ORDER BY tb.SubmittedAt DESC";

            var results = await connection.QueryAsync<dynamic>(sql, new { PaymentStatus = paymentStatus });
            var bids = new List<TenderBid>();

            foreach (var result in results)
            {
                var bid = new TenderBid
                {
                    Id = result.Id,
                    TenderId = result.TenderId,
                    BidderName = result.BidderName,
                    BidderEmail = result.BidderEmail,
                    BidderPhone = result.BidderPhone,
                    CompanyName = result.CompanyName,
                    CompanyAddress = result.CompanyAddress,
                    BidAmount = result.BidAmount,
                    EmdAmount = result.EmdAmount,
                    ProcessingFee = result.ProcessingFee,
                    TotalAmount = result.TotalAmount,
                    Status = result.Status,
                    PaymentStatus = result.PaymentStatus,
                    PaymentReference = result.PaymentReference,
                    PaymentDate = result.PaymentDate,
                    SubmittedAt = result.SubmittedAt,
                    IsActive = result.IsActive,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt,
                    Remarks = result.Remarks,
                    Tender = result.Tender_Id != null ? new Tender
                    {
                        Id = result.Tender_Id,
                        TenderId = result.Tender_TenderId,
                        TenderTitle = result.Tender_TenderTitle,
                        Description = result.Tender_Description,
                        Department = result.Tender_Department,
                        PublishDate = result.Tender_PublishDate,
                        EmdAmount = result.Tender_EmdAmount,
                        SdAmount = result.Tender_SdAmount,
                        ProcessingFee = result.Tender_ProcessingFee,
                        EstimatedValue = result.Tender_EstimatedValue,
                        LastDateEmd = result.Tender_LastDateEmd,
                        TenderClosingDate = result.Tender_TenderClosingDate,
                        TenderOpeningDate = result.Tender_TenderOpeningDate,
                        Status = result.Tender_Status,
                        CreatedBy = result.Tender_CreatedBy,
                        CreatedAt = result.Tender_CreatedAt,
                        UpdatedAt = result.Tender_UpdatedAt,
                        PublishedAt = result.Tender_PublishedAt,
                        IsActive = result.Tender_IsActive
                    } : null
                };

                bids.Add(bid);
            }

            return bids;
        }

        public async Task<(IEnumerable<TenderBid> bids, int totalCount)> GetBidsPagedAsync(int page, int pageSize, string? searchTerm = null, string? status = null, string? paymentStatus = null)
        {
            using var connection = CreateConnection();
            
            var whereClause = "WHERE tb.IsActive = 1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += " AND (tb.BidderName LIKE @SearchTerm OR tb.CompanyName LIKE @SearchTerm OR t.TenderTitle LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (!string.IsNullOrEmpty(status))
            {
                whereClause += " AND tb.Status = @Status";
                parameters.Add("Status", status);
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                whereClause += " AND tb.PaymentStatus = @PaymentStatus";
                parameters.Add("PaymentStatus", paymentStatus);
            }

            // Get total count
            var countSql = $@"
                SELECT COUNT(*)
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                {whereClause}";

            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

            // Get paged results
            var offset = (page - 1) * pageSize;
            var sql = $@"
                SELECT tb.*, t.*
                FROM TenderBids tb
                LEFT JOIN Tenders t ON tb.TenderId = t.Id
                {whereClause}
                ORDER BY tb.SubmittedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var bids = await connection.QueryAsync<TenderBid, Tender, TenderBid>(sql, (bid, tender) =>
            {
                bid.Tender = tender;
                return bid;
            }, parameters, splitOn: "Id");

            return (bids, totalCount);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus, string? paymentReference = null, DateTime? paymentDate = null)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE TenderBids 
                SET PaymentStatus = @PaymentStatus, PaymentReference = @PaymentReference, 
                    PaymentDate = @PaymentDate, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsActive = 1";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                PaymentStatus = paymentStatus,
                PaymentReference = paymentReference,
                PaymentDate = paymentDate,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }

        public async Task<string> GenerateUniquePaymentReferenceAsync()
        {
            using var connection = CreateConnection();
            var year = DateTime.Now.Year;
            var prefix = $"BID-{year}-";
            
            const string sql = @"
                SELECT COUNT(*) 
                FROM TenderBids 
                WHERE PaymentReference LIKE @Pattern";
            
            var count = await connection.QuerySingleAsync<int>(sql, new { Pattern = $"{prefix}%" });
            var nextNumber = count + 1;
            
            return $"{prefix}{nextNumber:D6}";
        }

        public async Task<IEnumerable<TenderBidDocument>> GetBidDocumentsAsync(int bidId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM TenderBidDocuments 
                WHERE TenderBidId = @BidId AND IsActive = 1
                ORDER BY UploadedAt DESC";

            return await connection.QueryAsync<TenderBidDocument>(sql, new { BidId = bidId });
        }

        public async Task<TenderBidDocument> AddBidDocumentAsync(TenderBidDocument document)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO TenderBidDocuments 
                (TenderBidId, DocumentName, FileName, FilePath, FileSize, ContentType, DocumentType, IsRequired, UploadedAt, IsActive)
                VALUES 
                (@TenderBidId, @DocumentName, @FileName, @FilePath, @FileSize, @ContentType, @DocumentType, @IsRequired, @UploadedAt, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await connection.QuerySingleAsync<int>(sql, document);
            document.Id = id;
            return document;
        }

        public async Task<bool> DeleteBidDocumentAsync(int documentId)
        {
            using var connection = CreateConnection();
            const string sql = "UPDATE TenderBidDocuments SET IsActive = 0 WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = documentId });
            return rowsAffected > 0;
        }

        public async Task<bool> CheckDuplicateBidAsync(int tenderId, string bidderEmail)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT COUNT(*) 
                FROM TenderBids 
                WHERE TenderId = @TenderId AND BidderEmail = @BidderEmail AND IsActive = 1";
            
            var count = await connection.QuerySingleAsync<int>(sql, new { TenderId = tenderId, BidderEmail = bidderEmail });
            return count > 0;
        }
    }
}
