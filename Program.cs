using BiddingSystem.Data;
using BiddingSystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using BiddingSystem.Data;
using BiddingSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configure options
builder.Services.Configure<PasswordOptions>(
    builder.Configuration.GetSection("PasswordOptions"));

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();

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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
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
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        Path.Combine(builder.Environment.ContentRootPath, "Content")),
//    RequestPath = "/Content"
//});
app.UseRouting();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});


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
