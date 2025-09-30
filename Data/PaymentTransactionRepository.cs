using BiddingSystem.Models;
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
    }
}