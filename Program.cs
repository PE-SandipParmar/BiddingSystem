using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register Payment Service
builder.Services.AddScoped<IRazorpayService, RazorpayService>();
builder.Services.AddScoped<IRazorpayPaymentService, RazorpayPaymentService>();
builder.Services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITenderRepository, TenderRepository>();

// Register repositories
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IEMDSDRepository, EMDSDRepository>();
builder.Services.AddScoped<ITenderBidRepository, TenderBidRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();

// Register services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEMDSDService, EMDSDService>();
builder.Services.AddScoped<ITenderBidService, TenderBidService>();


// Configure options
builder.Services.Configure<PasswordOptions>(
    builder.Configuration.GetSection("PasswordOptions"));

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<SecurityOptions>(
    builder.Configuration.GetSection("Security"));

// Register security services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IPaymentLinkRepository, PaymentLinkRepository>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IInputValidationService, InputValidationService>();
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

// Configure email options
//builder.Services.Configure<BiddingSystem.Services.EmailOptions>(
//    builder.Configuration.GetSection("EmailSettings"));

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "BiddingSystemAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // ✅ Changed from Always for development
        options.Cookie.SameSite = SameSiteMode.Lax; // ✅ Changed from Strict for better compatibility
    });

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(Roles.Admin));

    options.AddPolicy("AdminOrChecker", policy =>
        policy.RequireRole(Roles.Admin, Roles.Checker));

    options.AddPolicy("AllRoles", policy =>
        policy.RequireRole(Roles.Admin, Roles.Checker, Roles.Maker));
});



// Add security headers
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Match authentication cookie
    options.Cookie.SameSite = SameSiteMode.Lax; // Match authentication cookie
});


// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//// Add global exception handling middleware
//app.UseMiddleware<GlobalExceptionMiddleware>();

//// Add rate limiting middleware
//app.UseMiddleware<RateLimitingMiddleware>();

app.UseRouting();

// Security headers
//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
//    context.Response.Headers.Add("X-Frame-Options", "DENY");
//    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
//    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
//        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; img-src 'self' data:; font-src 'self' https://fonts.gstatic.com; connect-src 'self' ws: wss: http: https:;");
//    context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
//    await next();
//});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=index}/{id?}");

app.MapControllers(); // For API controllers

// Create default admin user if none exists
using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var adminUsers = await userRepository.GetUsersByRoleAsync(BiddingSystem.Models.UserRole.Admin);
        if (!adminUsers.Any())
        {
            var (passwordHash, salt) = passwordService.HashPassword("Admin@123");
            var adminUser = new BiddingSystem.Models.User
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@BiddingSystem.com",
                Username = "admin",
                PasswordHash = passwordHash,
                Salt = salt,
                Role = BiddingSystem.Models.UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userRepository.CreateAsync(adminUser);
            logger.LogInformation("Default admin user created: admin / Admin@123");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating default admin user");
    }
}

app.Run();
