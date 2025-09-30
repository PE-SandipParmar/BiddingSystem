using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using Dapper;
using System.Data.SqlClient;

namespace BiddingSystem.Data
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<PaymentTransactionRepository> _logger;

        public PaymentTransactionRepository(IConfiguration configuration, ILogger<PaymentTransactionRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<PaymentTransaction> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT * FROM PaymentTransactions 
                WHERE Id = @Id";

            return await connection.QuerySingleOrDefaultAsync<PaymentTransaction>(sql, new { Id = id });
        }

        public async Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"
                    INSERT INTO PaymentTransactions (
                        PaymentLinkId, TenderId, TenderBidId, RazorpayPaymentId, RazorpayOrderId,
                        Amount, Status, PaymentMethod, BankName, CardLast4, UPIId, WalletName,
                        ErrorCode, ErrorDescription, CustomerEmail, CustomerPhone, TransactionDate, CreatedAt
                    ) VALUES (
                        @PaymentLinkId, @TenderId, @TenderBidId, @RazorpayPaymentId, @RazorpayOrderId,
                        @Amount, @Status, @PaymentMethod, @BankName, @CardLast4, @UPIId, @WalletName,
                        @ErrorCode, @ErrorDescription, @CustomerEmail, @CustomerPhone, @TransactionDate, @CreatedAt
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var id = await connection.QuerySingleAsync<int>(sql, transaction);
                transaction.Id = id;
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment transaction");
                throw;
            }
        }

        public async Task<PaymentTransaction?> GetByPaymentIdAsync(string razorpayPaymentId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM PaymentTransactions WHERE RazorpayPaymentId = @RazorpayPaymentId";
            return await connection.QueryFirstOrDefaultAsync<PaymentTransaction>(sql, new { RazorpayPaymentId = razorpayPaymentId });
        }

        // Returns a list of all transactions for a payment link
        public async Task<List<PaymentTransaction>> GetByPaymentLinkIdAsync(string paymentLinkId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM PaymentTransactions WHERE PaymentLinkId = @PaymentLinkId ORDER BY CreatedAt DESC";
            var result = await connection.QueryAsync<PaymentTransaction>(sql, new { PaymentLinkId = paymentLinkId });
            return result.ToList();
        }

        public async Task<bool> ExistsAsync(string razorpayPaymentId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT COUNT(1) FROM PaymentTransactions WHERE RazorpayPaymentId = @RazorpayPaymentId";
            var count = await connection.QuerySingleAsync<int>(sql, new { RazorpayPaymentId = razorpayPaymentId });
            return count > 0;
        }

        public async Task<List<PaymentTransaction>> GetByTenderIdAsync(int tenderId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM PaymentTransactions WHERE TenderId = @TenderId ORDER BY CreatedAt DESC";
            var result = await connection.QueryAsync<PaymentTransaction>(sql, new { TenderId = tenderId });
            return result.ToList();
        }

        // New method for transaction list with filters
        public async Task<PaginatedList<PaymentTransactionViewModel>> GetTransactionListAsync(PaymentTransactionSearchFilter filter)
        {
            using var connection = new SqlConnection(_connectionString);
            var offset = (filter.PageNumber - 1) * filter.PageSize;

            var sql = @"
                WITH TransactionCTE AS (
                    SELECT 
                        pt.Id,
                        pt.PaymentLinkId,
                        pt.TenderId,
                        t.TenderId as TenderIdString,
                        t.TenderTitle,
                        pt.TenderBidId,
                        tb.BidderName,
                        tb.CompanyName,
                        pt.RazorpayPaymentId,
                        pt.RazorpayOrderId,
                        pt.Amount,
                        pt.Status,
                        pt.PaymentMethod,
                        pt.BankName,
                        pt.CardLast4,
                        pt.UPIId,
                        pt.WalletName,
                        pt.ErrorCode,
                        pt.ErrorDescription,
                        pt.CustomerEmail,
                        pt.CustomerPhone,
                        pt.TransactionDate,
                        pt.CreatedAt,
                        pt.PaymentType,
                        ROW_NUMBER() OVER (ORDER BY pt.TransactionDate DESC) as RowNum,
                        COUNT(*) OVER() as TotalCount
                    FROM PaymentTransactions pt
                    LEFT JOIN Tenders t ON pt.TenderId = t.Id
                    LEFT JOIN TenderBids tb ON pt.TenderBidId = tb.Id
                    WHERE 1=1
                        AND (@TenderId IS NULL OR @TenderId = '' OR t.TenderId LIKE '%' + @TenderId + '%')
                        AND (@TenderName IS NULL OR @TenderName = '' OR t.TenderTitle LIKE '%' + @TenderName + '%')
                        AND (@BidderName IS NULL OR @BidderName = '' OR tb.BidderName LIKE '%' + @BidderName + '%')
                        AND (@RazorpayPaymentId IS NULL OR @RazorpayPaymentId = '' OR pt.RazorpayPaymentId LIKE '%' + @RazorpayPaymentId + '%')
                        AND (@CustomerEmail IS NULL OR @CustomerEmail = '' OR pt.CustomerEmail LIKE '%' + @CustomerEmail + '%')
                        AND (@CustomerPhone IS NULL OR @CustomerPhone = '' OR pt.CustomerPhone LIKE '%' + @CustomerPhone + '%')
                        AND (@PaymentType IS NULL OR pt.PaymentType = @PaymentType)
                        AND (@Status IS NULL OR @Status = '' OR pt.Status = @Status)
                        AND (@PaymentMethod IS NULL OR @PaymentMethod = '' OR pt.PaymentMethod = @PaymentMethod)
                        AND (@FromDate IS NULL OR pt.TransactionDate >= @FromDate)
                        AND (@ToDate IS NULL OR pt.TransactionDate <= DATEADD(day, 1, @ToDate))
                )
                SELECT * FROM TransactionCTE
                WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
                ORDER BY TransactionDate DESC";

            var parameters = new
            {
                TenderId = filter.TenderId ?? "",
                TenderName = filter.TenderName ?? "",
                BidderName = filter.BidderName ?? "",
                RazorpayPaymentId = filter.RazorpayPaymentId ?? "",
                CustomerEmail = filter.CustomerEmail ?? "",
                CustomerPhone = filter.CustomerPhone ?? "",
                PaymentType = filter.PaymentType.HasValue ? (int)filter.PaymentType.Value : (int?)null,
                Status = filter.Status ?? "",
                PaymentMethod = filter.PaymentMethod ?? "",
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                Offset = offset,
                PageSize = filter.PageSize
            };

            var results = await connection.QueryAsync<dynamic>(sql, parameters);

            var list = new PaginatedList<PaymentTransactionViewModel>
            {
                Items = new List<PaymentTransactionViewModel>(),
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize
            };

            foreach (var item in results)
            {
                list.TotalCount = (int)item.TotalCount;

                list.Items.Add(new PaymentTransactionViewModel
                {
                    Id = item.Id,
                    PaymentLinkId = item.PaymentLinkId,
                    TenderId = item.TenderId,
                    TenderIdString = item.TenderIdString,
                    TenderTitle = item.TenderTitle,
                    TenderBidId = item.TenderBidId,
                    BidderName = item.BidderName,
                    CompanyName = item.CompanyName,
                    RazorpayPaymentId = item.RazorpayPaymentId,
                    RazorpayOrderId = item.RazorpayOrderId,
                    Amount = item.Amount,
                    Status = item.Status,
                    PaymentMethod = item.PaymentMethod,
                    BankName = item.BankName,
                    CardLast4 = item.CardLast4,
                    UPIId = item.UPIId,
                    WalletName = item.WalletName,
                    ErrorCode = item.ErrorCode,
                    ErrorDescription = item.ErrorDescription,
                    CustomerEmail = item.CustomerEmail,
                    CustomerPhone = item.CustomerPhone,
                    TransactionDate = item.TransactionDate,
                    CreatedAt = item.CreatedAt,
                    PaymentType = item.PaymentType != null ? (PaymentType)item.PaymentType : (PaymentType?)null
                });
            }

            list.TotalPages = (int)Math.Ceiling(list.TotalCount / (double)filter.PageSize);
            return list;
        }

        public async Task<TransactionStatistics> GetTransactionStatisticsAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    COUNT(*) as TotalTransactions,
                    COUNT(CASE WHEN Status = 'Success' THEN 1 END) as SuccessfulTransactions,
                    COUNT(CASE WHEN Status = 'Failed' THEN 1 END) as FailedTransactions,
                    COUNT(CASE WHEN Status = 'Pending' THEN 1 END) as PendingTransactions,
                    COUNT(CASE WHEN Status = 'Refunded' THEN 1 END) as RefundedTransactions,
                    ISNULL(SUM(CASE WHEN Amount > 0 THEN Amount ELSE 0 END), 0) as TotalAmount,
                    ISNULL(SUM(CASE WHEN Status = 'Success' AND Amount > 0 THEN Amount ELSE 0 END), 0) as SuccessfulAmount,
                    ISNULL(SUM(CASE WHEN Status = 'Failed' AND Amount > 0 THEN Amount ELSE 0 END), 0) as FailedAmount,
                    ISNULL(SUM(CASE WHEN Status = 'Refunded' THEN ABS(Amount) ELSE 0 END), 0) as RefundedAmount,
                    ISNULL(SUM(CASE WHEN CAST(TransactionDate as DATE) = CAST(GETDATE() as DATE) AND Amount > 0 THEN Amount ELSE 0 END), 0) as TodayAmount,
                    COUNT(CASE WHEN CAST(TransactionDate as DATE) = CAST(GETDATE() as DATE) THEN 1 END) as TodayTransactions
                FROM PaymentTransactions";

            var stats = await connection.QuerySingleOrDefaultAsync<TransactionStatistics>(sql);
            return stats ?? new TransactionStatistics();
        }
    }
}