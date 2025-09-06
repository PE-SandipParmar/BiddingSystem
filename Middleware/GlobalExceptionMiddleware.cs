using System.Net;
using System.Text.Json;

namespace BiddingSystem.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ErrorResponse();
            
            switch (exception)
            {
                case ArgumentException argEx:
                    response.Message = "Invalid input provided";
                    response.Details = _environment.IsDevelopment() ? argEx.Message : "Please check your input and try again";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                
                case UnauthorizedAccessException:
                    response.Message = "Access denied";
                    response.Details = "You do not have permission to perform this action";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                
                case FileNotFoundException:
                    response.Message = "File not found";
                    response.Details = "The requested file could not be found";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                
                case TimeoutException:
                    response.Message = "Request timeout";
                    response.Details = "The request took too long to process";
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;
                
                case InvalidOperationException invOpEx:
                    response.Message = "Invalid operation";
                    response.Details = _environment.IsDevelopment() ? invOpEx.Message : "The requested operation cannot be completed";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                
                default:
                    response.Message = "An error occurred while processing your request";
                    response.Details = _environment.IsDevelopment() ? exception.Message : "Please try again later or contact support";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            response.StatusCode = context.Response.StatusCode;
            response.Timestamp = DateTime.UtcNow;
            response.RequestId = context.TraceIdentifier;

            if (_environment.IsDevelopment())
            {
                response.StackTrace = exception.StackTrace;
                response.InnerException = exception.InnerException?.Message;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
    }
}
