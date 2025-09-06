using System.Collections.Concurrent;
using System.Net;

namespace BiddingSystem.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly ConcurrentDictionary<string, RateLimitInfo> _requests = new();
        private readonly ConcurrentDictionary<string, RateLimitInfo> _loginAttempts = new();
        private readonly ConcurrentDictionary<string, RateLimitInfo> _fileUploads = new();

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Check rate limits based on endpoint
            if (path.Contains("/account/login"))
            {
                if (!CheckRateLimit(_loginAttempts, clientIp, 5, TimeSpan.FromMinutes(1)))
                {
                    await HandleRateLimitExceeded(context, "Too many login attempts. Please try again later.");
                    return;
                }
            }
            else if (path.Contains("/upload") || path.Contains("/document"))
            {
                if (!CheckRateLimit(_fileUploads, clientIp, 10, TimeSpan.FromHours(1)))
                {
                    await HandleRateLimitExceeded(context, "Too many file uploads. Please try again later.");
                    return;
                }
            }
            else
            {
                if (!CheckRateLimit(_requests, clientIp, 60, TimeSpan.FromMinutes(1)))
                {
                    await HandleRateLimitExceeded(context, "Too many requests. Please slow down.");
                    return;
                }
            }

            await _next(context);
        }

        private bool CheckRateLimit(ConcurrentDictionary<string, RateLimitInfo> rateLimitDict, string key, int maxRequests, TimeSpan window)
        {
            var now = DateTime.UtcNow;
            var rateLimitInfo = rateLimitDict.AddOrUpdate(key,
                new RateLimitInfo { Count = 1, WindowStart = now },
                (k, existing) =>
                {
                    if (now - existing.WindowStart > window)
                    {
                        return new RateLimitInfo { Count = 1, WindowStart = now };
                    }
                    return new RateLimitInfo { Count = existing.Count + 1, WindowStart = existing.WindowStart };
                });

            if (rateLimitInfo.Count > maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for {Key}: {Count} requests in {Window}", 
                    key, rateLimitInfo.Count, window);
                return false;
            }

            return true;
        }

        private async Task HandleRateLimitExceeded(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", "60");
            
            if (context.Request.Headers.Accept.ToString().Contains("application/json"))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\": \"{message}\", \"retryAfter\": 60}}");
            }
            else
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync($@"
                    <html>
                        <head><title>Rate Limit Exceeded</title></head>
                        <body>
                            <h1>Rate Limit Exceeded</h1>
                            <p>{message}</p>
                            <p>Please wait 60 seconds before trying again.</p>
                        </body>
                    </html>");
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first (for load balancers/proxies)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    public class RateLimitInfo
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
