namespace BiddingSystem.Models
{
    public class SecurityOptions
    {
        public const string SectionName = "Security";
        
        public PasswordRequirements PasswordRequirements { get; set; } = new();
        public LockoutSettings Lockout { get; set; } = new();
        public RateLimitingSettings RateLimiting { get; set; } = new();
        public FileUploadSettings FileUpload { get; set; } = new();
    }

    public class PasswordRequirements
    {
        public int RequiredLength { get; set; } = 12;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;
    }

    public class LockoutSettings
    {
        public TimeSpan DefaultLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(15);
        public int MaxFailedAccessAttempts { get; set; } = 3;
        public bool AllowedForNewUsers { get; set; } = true;
    }

    public class RateLimitingSettings
    {
        public int MaxRequestsPerMinute { get; set; } = 60;
        public int MaxLoginAttemptsPerMinute { get; set; } = 5;
        public int MaxFileUploadsPerHour { get; set; } = 10;
        public int MaxBidSubmissionsPerDay { get; set; } = 10;
    }

    public class FileUploadSettings
    {
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
        public string[] AllowedExtensions { get; set; } = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
        public string[] AllowedMimeTypes { get; set; } = { 
            "application/pdf", 
            "application/msword", 
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "image/jpeg", 
            "image/png" 
        };
        public bool ScanForMalware { get; set; } = true;
        public string UploadPath { get; set; } = "wwwroot/uploads";
    }
}
