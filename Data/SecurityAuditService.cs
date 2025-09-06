using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public class SecurityAuditService : ISecurityAuditService
    {
        private readonly string _connectionString;
        private readonly ILogger<SecurityAuditService> _logger;

        public SecurityAuditService(IConfiguration configuration, ILogger<SecurityAuditService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
        {
            try
            {
                using var connection = CreateConnection();
                const string sql = @"
                    INSERT INTO SecurityEvents 
                    (EventType, Description, UserId, Username, IpAddress, UserAgent, AdditionalData, Severity, Timestamp, IsResolved, Resolution)
                    VALUES 
                    (@EventType, @Description, @UserId, @Username, @IpAddress, @UserAgent, @AdditionalData, @Severity, @Timestamp, @IsResolved, @Resolution)";

                await connection.ExecuteAsync(sql, securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event: {EventType}", securityEvent.EventType);
            }
        }

        public async Task LogUserActionAsync(UserAction userAction)
        {
            try
            {
                using var connection = CreateConnection();
                const string sql = @"
                    INSERT INTO UserActions 
                    (UserId, Action, EntityType, EntityId, OldValues, NewValues, IpAddress, UserAgent, Timestamp)
                    VALUES 
                    (@UserId, @Action, @EntityType, @EntityId, @OldValues, @NewValues, @IpAddress, @UserAgent, @Timestamp)";

                await connection.ExecuteAsync(sql, userAction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user action: {Action}", userAction.Action);
            }
        }

        public async Task LogFailedLoginAttemptAsync(string username, string ipAddress, string reason)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = "FailedLogin",
                Description = $"Failed login attempt for user: {username}. Reason: {reason}",
                Username = username,
                IpAddress = ipAddress,
                Severity = SecurityEventSeverity.Medium,
                AdditionalData = JsonSerializer.Serialize(new { Reason = reason })
            };

            await LogSecurityEventAsync(securityEvent);
        }

        public async Task LogSuccessfulLoginAsync(int userId, string ipAddress)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = "SuccessfulLogin",
                Description = "User logged in successfully",
                UserId = userId,
                IpAddress = ipAddress,
                Severity = SecurityEventSeverity.Low
            };

            await LogSecurityEventAsync(securityEvent);
        }

        public async Task LogFileUploadAsync(int userId, string fileName, string filePath, bool success)
        {
            var action = new UserAction
            {
                UserId = userId,
                Action = success ? "FileUpload" : "FileUploadFailed",
                EntityType = "File",
                NewValues = JsonSerializer.Serialize(new { FileName = fileName, FilePath = filePath, Success = success })
            };

            await LogUserActionAsync(action);

            if (!success)
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = "FileUploadFailed",
                    Description = $"Failed file upload attempt: {fileName}",
                    UserId = userId,
                    Severity = SecurityEventSeverity.Medium,
                    AdditionalData = JsonSerializer.Serialize(new { FileName = fileName, FilePath = filePath })
                };

                await LogSecurityEventAsync(securityEvent);
            }
        }

        public async Task LogDataAccessAsync(int userId, string entityType, string action, int? entityId = null)
        {
            var userAction = new UserAction
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId
            };

            await LogUserActionAsync(userAction);
        }

        public async Task<List<SecurityEvent>> GetSecurityEventsAsync(DateTime from, DateTime to, string? eventType = null)
        {
            using var connection = CreateConnection();
            var whereClause = "WHERE Timestamp BETWEEN @From AND @To";
            var parameters = new { From = from, To = to };

            if (!string.IsNullOrEmpty(eventType))
            {
                whereClause += " AND EventType = @EventType";
                //parameters = new { From = from, To = to, EventType = eventType };
            }

            var sql = $@"
                SELECT Id, EventType, Description, UserId, Username, IpAddress, UserAgent, 
                       AdditionalData, Severity, Timestamp, IsResolved, Resolution
                FROM SecurityEvents 
                {whereClause}
                ORDER BY Timestamp DESC";

            var events = await connection.QueryAsync<SecurityEvent>(sql, parameters);
            return events.ToList();
        }

        public async Task<List<UserAction>> GetUserActionsAsync(int userId, DateTime from, DateTime to)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Id, UserId, Action, EntityType, EntityId, OldValues, NewValues, 
                       IpAddress, UserAgent, Timestamp
                FROM UserActions 
                WHERE UserId = @UserId AND Timestamp BETWEEN @From AND @To
                ORDER BY Timestamp DESC";

            var actions = await connection.QueryAsync<UserAction>(sql, new { UserId = userId, From = from, To = to });
            return actions.ToList();
        }
    }
}
