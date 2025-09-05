using Dapper;
using System.Data.SqlClient;

namespace BiddingSystem.Data
{
    public interface IAuditService
    {
        Task LogUserActionAsync(int userId, string action, string details, int? performedBy = null, string? ipAddress = null, string? userAgent = null);
        Task LogLoginAsync(int userId, string ipAddress, string userAgent, bool successful = true);
        Task LogLogoutAsync(int userId);
        Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, int limit = 50);
    }

    public class AuditService : IAuditService
    {
        private readonly string _connectionString;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IConfiguration configuration, ILogger<AuditService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
        }

        public async Task LogUserActionAsync(int userId, string action, string details, int? performedBy = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                const string sql = @"
                    INSERT INTO UserAuditLog (UserId, Action, Details, PerformedBy, IpAddress, UserAgent, Timestamp)
                    VALUES (@UserId, @Action, @Details, @PerformedBy, @IpAddress, @UserAgent, @Timestamp)";

                await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    PerformedBy = performedBy,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user action");
            }
        }

        public async Task LogLoginAsync(int userId, string ipAddress, string userAgent, bool successful = true)
        {
            var action = successful ? "LOGIN_SUCCESS" : "LOGIN_FAILED";
            var details = successful ? "User logged in successfully" : "Failed login attempt";
            await LogUserActionAsync(userId, action, details, null, ipAddress, userAgent);
        }

        public async Task LogLogoutAsync(int userId)
        {
            await LogUserActionAsync(userId, "LOGOUT", "User logged out");
        }

        public async Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, int limit = 50)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT TOP(@Limit) Id, UserId, Action, Details, PerformedBy, IpAddress, UserAgent, Timestamp
                FROM UserAuditLog 
                WHERE UserId = @UserId 
                ORDER BY Timestamp DESC";

            var logs = await connection.QueryAsync<AuditLog>(sql, new { UserId = userId, Limit = limit });
            return logs.ToList();
        }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int? PerformedBy { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
