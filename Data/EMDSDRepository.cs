using BiddingSystem.Models;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace BiddingSystem.Data
{
    public class EMDSDRepository : IEMDSDRepository
    {
        private readonly string _connectionString;

        public EMDSDRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<EMDSDDeposit>> GetAllDepositsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                ORDER BY d.CreatedAt DESC";

            var results = await connection.QueryAsync<dynamic>(sql);
            var deposits = new List<EMDSDDeposit>();

            foreach (var result in results)
            {
                var deposit = new EMDSDDeposit
                {
                    Id = result.Id,
                    DepositId = result.DepositId,
                    TenderId = result.TenderId,
                    Amount = result.Amount,
                    BidderName = result.BidderName,
                    CompanyName = result.CompanyName,
                    TransactionDate = result.TransactionDate,
                    BankName = result.BankName,
                    FSSAIBranchName = result.FSSAIBranchName,
                    TransactionId = result.TransactionId,
                    Status = result.Status,
                    Type = result.Type,
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
                        Status = (TenderStatus)result.Tender_Status,
                        CreatedBy = result.Tender_CreatedBy,
                        CreatedAt = result.Tender_CreatedAt,
                        UpdatedAt = result.Tender_UpdatedAt,
                        PublishedAt = result.Tender_PublishedAt,
                        IsActive = result.Tender_IsActive
                    } : null
                };

                // Load transactions for each deposit
                deposit.Transactions = (await GetTransactionsByDepositIdAsync(deposit.Id)).ToList();
                deposits.Add(deposit);
            }

            return deposits;
        }

        public async Task<EMDSDDeposit?> GetDepositByIdAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                WHERE d.Id = @Id";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
            if (result == null) return null;

            var deposit = new EMDSDDeposit
            {
                Id = result.Id,
                DepositId = result.DepositId,
                TenderId = result.TenderId,
                TenderBidId = result.TenderBidId,
                Amount = result.Amount,
                BidderName = result.BidderName,
                CompanyName = result.CompanyName,
                TransactionDate = result.TransactionDate,
                BankName = result.BankName,
                FSSAIBranchName = result.FSSAIBranchName,
                TransactionId = result.TransactionId,
                Status = result.Status,
                Type = result.Type,
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
                    Status = (TenderStatus)result.Tender_Status,
                    CreatedBy = result.Tender_CreatedBy,
                    CreatedAt = result.Tender_CreatedAt,
                    UpdatedAt = result.Tender_UpdatedAt,
                    PublishedAt = result.Tender_PublishedAt,
                    IsActive = result.Tender_IsActive
                } : null
            };

            deposit.Transactions = (await GetTransactionsByDepositIdAsync(deposit.Id)).ToList();
            return deposit;
        }

        public async Task<EMDSDDeposit?> GetDepositByDepositIdAsync(string depositId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                WHERE d.DepositId = @DepositId";

            var result = await connection.QueryAsync<EMDSDDeposit, Tender, EMDSDDeposit>(sql,
                (deposit, tender) =>
                {
                    deposit.Tender = tender;
                    return deposit;
                },
                new { DepositId = depositId },
                splitOn: "Tender_Id");

            var deposit = result.FirstOrDefault();
            if (deposit != null)
            {
                deposit.Transactions = (await GetTransactionsByDepositIdAsync(deposit.Id)).ToList();
            }

            return deposit;
        }

        public async Task<IEnumerable<EMDSDDeposit>> SearchDepositsAsync(string? depositId, string? tenderName, string? status, string? type)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(depositId))
            {
                sql += " AND d.DepositId LIKE @DepositId";
                parameters.Add("DepositId", $"%{depositId}%");
            }

            if (!string.IsNullOrEmpty(tenderName))
            {
                sql += " AND t.TenderTitle LIKE @TenderName";
                parameters.Add("TenderName", $"%{tenderName}%");
            }

            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND d.Status = @Status";
                parameters.Add("Status", status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                sql += " AND d.Type = @Type";
                parameters.Add("Type", type);
            }

            sql += " ORDER BY d.CreatedAt DESC";

            var results = await connection.QueryAsync<dynamic>(sql, parameters);
            var deposits = new List<EMDSDDeposit>();

            foreach (var result in results)
            {
                var deposit = new EMDSDDeposit
                {
                    Id = result.Id,
                    DepositId = result.DepositId,
                    TenderId = result.TenderId,
                    Amount = result.Amount,
                    BidderName = result.BidderName,
                    CompanyName = result.CompanyName,
                    TransactionDate = result.TransactionDate,
                    BankName = result.BankName,
                    FSSAIBranchName = result.FSSAIBranchName,
                    TransactionId = result.TransactionId,
                    Status = result.Status,
                    Type = result.Type,
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
                        Status = (TenderStatus)result.Tender_Status,
                        CreatedBy = result.Tender_CreatedBy,
                        CreatedAt = result.Tender_CreatedAt,
                        UpdatedAt = result.Tender_UpdatedAt,
                        PublishedAt = result.Tender_PublishedAt,
                        IsActive = result.Tender_IsActive
                    } : null
                };

                deposit.Transactions = (await GetTransactionsByDepositIdAsync(deposit.Id)).ToList();
                deposits.Add(deposit);
            }

            return deposits;
        }

        public async Task<EMDSDDeposit> CreateDepositAsync(EMDSDDeposit deposit)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO EMDSDDeposits 
                (DepositId, TenderId,TenderBidId, Amount, BidderName, CompanyName, TransactionDate, 
                 BankName, FSSAIBranchName, TransactionId, Status, Type, Remarks, CreatedAt)
                VALUES 
                (@DepositId, @TenderId,@TenderBidId, @Amount, @BidderName, @CompanyName, @TransactionDate, 
                 @BankName, @FSSAIBranchName, @TransactionId, @Status, @Type, @Remarks, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await connection.QuerySingleAsync<int>(sql, deposit);
            deposit.Id = id;
            return deposit;
        }

        public async Task<EMDSDDeposit> UpdateDepositAsync(EMDSDDeposit deposit)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE EMDSDDeposits 
                SET TenderId = @TenderId, TenderBidId = @TenderBidId, Amount = @Amount, BidderName = @BidderName, 
                    CompanyName = @CompanyName, TransactionDate = @TransactionDate, 
                    BankName = @BankName, FSSAIBranchName = @FSSAIBranchName, 
                    TransactionId = @TransactionId, Status = @Status, Type = @Type, 
                    Remarks = @Remarks, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, deposit);
            return deposit;
        }

        public async Task<bool> DeleteDepositAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = "DELETE FROM EMDSDDeposits WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<EMDSDDeposit>> GetDepositsByTenderIdAsync(int tenderId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                WHERE d.TenderId = @TenderId
                ORDER BY d.CreatedAt DESC";

            var result = await connection.QueryAsync<EMDSDDeposit, Tender, EMDSDDeposit>(sql,
                (deposit, tender) =>
                {
                    deposit.Tender = tender;
                    return deposit;
                },
                new { TenderId = tenderId },
                splitOn: "Tender_Id");

            var deposits = result.ToList();
            foreach (var deposit in deposits)
            {
                deposit.Transactions = (await GetTransactionsByDepositIdAsync(deposit.Id)).ToList();
            }

            return deposits;
        }

        public async Task<IEnumerable<EMDSDDeposit>> GetDepositsByStatusAsync(string status)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                WHERE d.Status = @Status
                ORDER BY d.CreatedAt DESC";

            var result = await connection.QueryAsync<EMDSDDeposit, Tender, EMDSDDeposit>(sql,
                (deposit, tender) =>
                {
                    deposit.Tender = tender;
                    return deposit;
                },
                new { Status = status },
                splitOn: "Tender_Id");

            var deposits = result.ToList();
            foreach (var deposit in deposits)
            {
                deposit.Transactions = (await GetTransactionsByDepositIdAsync(deposit.Id)).ToList();
            }

            return deposits;
        }

        public async Task<IEnumerable<EMDSDDeposit>> GetDepositsByTypeAsync(string type)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                WHERE d.Type = @Type
                ORDER BY d.CreatedAt DESC";

            var result = await connection.QueryAsync<EMDSDDeposit, Tender, EMDSDDeposit>(sql,
                (deposit, tender) =>
                {
                    deposit.Tender = tender;
                    return deposit;
                },
                new { Type = type },
                splitOn: "Tender_Id");

            var deposits = result.ToList();
            foreach (var deposit in deposits)
            {
                deposit.Transactions = (await GetTransactionsByDepositIdAsync(deposit.Id)).ToList();
            }

            return deposits;
        }

        public async Task<(IEnumerable<EMDSDDeposit> deposits, int totalCount)> GetDepositsPagedAsync(int page, int pageSize, string? searchTerm = null, string? status = null, string? type = null)
        {
            using var connection = CreateConnection();
            
            // Build the WHERE clause
            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += " AND (d.DepositId LIKE @SearchTerm OR t.TenderTitle LIKE @SearchTerm OR d.BidderName LIKE @SearchTerm OR d.CompanyName LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (!string.IsNullOrEmpty(status))
            {
                whereClause += " AND d.Status = @Status";
                parameters.Add("Status", status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                whereClause += " AND d.Type = @Type";
                parameters.Add("Type", type);
            }

            // Get total count
            var countSql = $@"
                SELECT COUNT(*)
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                {whereClause}";

            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

            // Get paged results
            var offset = (page - 1) * pageSize;
            var sql = $@"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                {whereClause}
                ORDER BY d.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var results = await connection.QueryAsync<dynamic>(sql, parameters);
            var deposits = new List<EMDSDDeposit>();

            foreach (var result in results)
            {
                var deposit = new EMDSDDeposit
                {
                    Id = result.Id,
                    DepositId = result.DepositId,
                    TenderId = result.TenderId,
                    Amount = result.Amount,
                    BidderName = result.BidderName,
                    CompanyName = result.CompanyName,
                    TransactionDate = result.TransactionDate,
                    BankName = result.BankName,
                    FSSAIBranchName = result.FSSAIBranchName,
                    TransactionId = result.TransactionId,
                    Status = result.Status,
                    Type = result.Type,
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
                        Status = (TenderStatus)result.Tender_Status,
                        CreatedBy = result.Tender_CreatedBy,
                        CreatedAt = result.Tender_CreatedAt,
                        UpdatedAt = result.Tender_UpdatedAt,
                        PublishedAt = result.Tender_PublishedAt,
                        IsActive = result.Tender_IsActive
                    } : null
                };

                deposit.Transactions = (await GetTransactionsByDepositIdAsync(deposit.Id)).ToList();
                deposits.Add(deposit);
            }

            return (deposits, totalCount);
        }

        public async Task<IEnumerable<EMDSDTransaction>> GetTransactionsByDepositIdAsync(int depositId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM EMDSDTransactions 
                WHERE EMDSDDepositId = @DepositId 
                ORDER BY CreatedAt DESC";

            return await connection.QueryAsync<EMDSDTransaction>(sql, new { DepositId = depositId });
        }

        public async Task<EMDSDTransaction> CreateTransactionAsync(EMDSDTransaction transaction)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO EMDSDTransactions 
                (EMDSDDepositId, TransactionReference, Amount, TransactionType, Status, 
                 TransactionDate, BankReference, Remarks, CreatedAt, CreatedBy)
                VALUES 
                (@EMDSDDepositId, @TransactionReference, @Amount, @TransactionType, @Status, 
                 @TransactionDate, @BankReference, @Remarks, @CreatedAt, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await connection.QuerySingleAsync<int>(sql, transaction);
            transaction.Id = id;
            return transaction;
        }

        public async Task<EMDSDTransaction> UpdateTransactionAsync(EMDSDTransaction transaction)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE EMDSDTransactions 
                SET TransactionReference = @TransactionReference, Amount = @Amount, 
                    TransactionType = @TransactionType, Status = @Status, 
                    TransactionDate = @TransactionDate, BankReference = @BankReference, 
                    Remarks = @Remarks, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, transaction);
            return transaction;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = "DELETE FROM EMDSDTransactions WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<string> GenerateUniqueDepositIdAsync()
        {
            using var connection = CreateConnection();
            var year = DateTime.Now.Year;
            var prefix = $"EMD-{year}-";
            
            const string sql = @"
                SELECT COUNT(*) 
                FROM EMDSDDeposits 
                WHERE DepositId LIKE @Pattern";
            
            var count = await connection.QuerySingleAsync<int>(sql, new { Pattern = $"{prefix}%" });
            var nextNumber = count + 1;
            
            return $"{prefix}{nextNumber:D6}";
        }

        public async Task<EMDSDDeposit?> GetDepositByTenderBidAndPaymentLinkAsync(int tenderBidId, int paymentLinkId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT d.*, t.Id as Tender_Id, t.TenderId as Tender_TenderId, t.TenderTitle as Tender_TenderTitle, 
                       t.Description as Tender_Description, t.Department as Tender_Department,
                       t.PublishDate as Tender_PublishDate, t.EmdAmount as Tender_EmdAmount,
                       t.SdAmount as Tender_SdAmount, t.ProcessingFee as Tender_ProcessingFee,
                       t.EstimatedValue as Tender_EstimatedValue, t.LastDateEmd as Tender_LastDateEmd,
                       t.TenderClosingDate as Tender_TenderClosingDate, t.TenderOpeningDate as Tender_TenderOpeningDate,
                       t.Status as Tender_Status, t.CreatedBy as Tender_CreatedBy, t.CreatedAt as Tender_CreatedAt,
                       t.UpdatedAt as Tender_UpdatedAt, t.PublishedAt as Tender_PublishedAt, t.IsActive as Tender_IsActive
                FROM EMDSDDeposits d
                LEFT JOIN Tenders t ON d.TenderId = t.Id
                WHERE d.TenderBidId = @TenderBidId 
                AND d.TransactionId IN (
                    SELECT pl.TransactionId 
                    FROM PaymentLinks pl 
                    WHERE pl.Id = @PaymentLinkId AND pl.TransactionId IS NOT NULL
                )
                ORDER BY d.CreatedAt DESC";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { TenderBidId = tenderBidId, PaymentLinkId = paymentLinkId });
            
            if (result == null) return null;

            var deposit = new EMDSDDeposit
            {
                Id = result.Id,
                DepositId = result.DepositId,
                TenderId = result.TenderId,
                TenderBidId = result.TenderBidId,
                Amount = result.Amount,
                BidderName = result.BidderName,
                CompanyName = result.CompanyName,
                TransactionDate = result.TransactionDate,
                BankName = result.BankName,
                FSSAIBranchName = result.FSSAIBranchName,
                TransactionId = result.TransactionId,
                Status = result.Status,
                Type = result.Type,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                Remarks = result.Remarks,
                Tender = new Tender
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
                }
            };

            return deposit;
        }
    }
}