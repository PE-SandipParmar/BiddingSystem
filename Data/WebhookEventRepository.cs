using BiddingSystem.Models;
using Dapper;
using System.Data.SqlClient;

namespace BiddingSystem.Data
{
    public class WebhookEventRepository : IWebhookEventRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<WebhookEventRepository> _logger;

        public WebhookEventRepository(IConfiguration configuration, ILogger<WebhookEventRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<WebhookEvent> CreateAsync(WebhookEvent webhookEvent)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var sql = @"
                    INSERT INTO WebhookEvents (
                        EventType, RazorpayPaymentId, RazorpayOrderId, PaymentLinkId,
                        Amount, Status, ProcessedSuccessfully, ErrorMessage, ReceivedAt, ProcessedAt
                    ) VALUES (
                        @EventType, @RazorpayPaymentId, @RazorpayOrderId, @PaymentLinkId,
                        @Amount, @Status, @ProcessedSuccessfully, @ErrorMessage, @ReceivedAt, @ProcessedAt
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var id = await connection.QuerySingleAsync<int>(sql, webhookEvent);
                webhookEvent.Id = id;
                return webhookEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating webhook event");
                throw;
            }
        }

        public async Task<WebhookEvent> UpdateAsync(WebhookEvent webhookEvent)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE WebhookEvents 
                SET ProcessedSuccessfully = @ProcessedSuccessfully, 
                    ErrorMessage = @ErrorMessage, 
                    ProcessedAt = @ProcessedAt
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, webhookEvent);
            return webhookEvent;
        }

        public async Task<WebhookEvent?> GetByPaymentIdAsync(string razorpayPaymentId, string eventType)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT * FROM WebhookEvents 
                       WHERE RazorpayPaymentId = @RazorpayPaymentId 
                       AND EventType = @EventType";

            return await connection.QueryFirstOrDefaultAsync<WebhookEvent>(sql,
                new { RazorpayPaymentId = razorpayPaymentId, EventType = eventType });
        }

        public async Task<List<WebhookEvent>> GetUnprocessedEventsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT * FROM WebhookEvents 
                       WHERE ProcessedSuccessfully = 0 
                       ORDER BY ReceivedAt ASC";

            var result = await connection.QueryAsync<WebhookEvent>(sql);
            return result.ToList();
        }
    }
}
