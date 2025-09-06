using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface ISecurityAuditService
    {
        Task LogSecurityEventAsync(SecurityEvent securityEvent);
        Task LogUserActionAsync(UserAction userAction);
        Task LogFailedLoginAttemptAsync(string username, string ipAddress, string reason);
        Task LogSuccessfulLoginAsync(int userId, string ipAddress);
        Task LogFileUploadAsync(int userId, string fileName, string filePath, bool success);
        Task LogDataAccessAsync(int userId, string entityType, string action, int? entityId = null);
        Task<List<SecurityEvent>> GetSecurityEventsAsync(DateTime from, DateTime to, string? eventType = null);
        Task<List<UserAction>> GetUserActionsAsync(int userId, DateTime from, DateTime to);
    }

    public class SecurityEvent
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string? AdditionalData { get; set; }
        public SecurityEventSeverity Severity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public string? Resolution { get; set; }
    }

    public class UserAction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public enum SecurityEventSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
