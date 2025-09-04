using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using System.Security.Claims;

namespace BiddingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public IActionResult Register(bool isAdmin = false)
        {
            if (User.Identity?.IsAuthenticated == true && !isAdmin)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (isAdmin && !User.IsInRole(Roles.Admin))
            {
                return Forbid();
            }

            var model = new RegisterViewModel
            {
                IsAdminRegistration = isAdmin
            };

            return View(model);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if this is admin registration and user has permission
            if (model.IsAdminRegistration && !User.IsInRole(Roles.Admin))
            {
                return Forbid();
            }

            // Validate role selection for admin registration
            if (model.IsAdminRegistration && !User.IsInRole(Roles.Admin) && model.Role == UserRole.Admin)
            {
                ModelState.AddModelError("Role", "You don't have permission to create admin users.");
                return View(model);
            }

            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.ExistsAsync(model.Username, model.Email);
                if (existingUser)
                {
                    ModelState.AddModelError("", "A user with this username or email already exists.");
                    return View(model);
                }

                // Hash password
                var (passwordHash, salt) = _passwordService.HashPassword(model.Password);

                // Create user
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Role = model.IsAdminRegistration ? model.Role : UserRole.Checker,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PhoneNumber = model.PhoneNumber,
                    Department = model.Department,
                    CreatedBy = model.IsAdminRegistration ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) : null
                };

                var userId = await _userRepository.CreateAsync(user);
                if (userId > 0)
                {
                    _logger.LogInformation($"User {model.Username} registered successfully by {(model.IsAdminRegistration ? User.Identity?.Name : "self-registration")}");

                    // Send welcome email
                    await _emailService.SendWelcomeEmailAsync(model.Email, $"{model.FirstName} {model.LastName}", model.Username);

                    if (model.IsAdminRegistration)
                    {
                        TempData["SuccessMessage"] = $"User {model.Username} has been created successfully.";
                        return RedirectToAction("UserManagement", "Admin");
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Registration successful! Please log in.";
                        return RedirectToAction("Login");
                    }
                }

                ModelState.AddModelError("", "Registration failed. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userRepository.GetByUsernameAsync(model.Username);
                if (user == null || !_passwordService.VerifyPassword(model.Password, user.PasswordHash, user.Salt))
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact administrator.");
                    return View(model);
                }

                // Update last login
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Create claims
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Role, user.Role.ToString()),
                    new("FullName", $"{user.FirstName} {user.LastName}"),
                    new("Department", user.Department ?? ""),
                    new("UserRole", user.Role.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                _logger.LogInformation($"User {model.Username} ({user.Role}) logged in successfully");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Role-based redirect
                return user.Role switch
                {
                    UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                    UserRole.Checker =>  RedirectToAction("Index", "Dashboard"),
                    UserRole.Maker => RedirectToAction("Index", "Dashboard"),
                    _ => RedirectToAction("Index","Dashboard")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
            }

            return View(model);
        }

        // GET: /Account/Dashboard
        [Authorize] // ✅ Requires authentication
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var currentUser = await _userRepository.GetByIdAsync(userId);

                if (currentUser == null)
                {
                    return RedirectToAction("Login");
                }

                var model = new ViewModels.DashboardViewModel
                {
                    CurrentUser = currentUser
                };

                // Add role-specific data
                if (currentUser.Role == UserRole.Admin)
                {
                    model.TotalUsers = await _userRepository.GetTotalUsersCountAsync();
                    model.RecentRegistrations = await _userRepository.GetRecentRegistrationsCountAsync();
                    model.RecentUsers = await _userRepository.GetRecentUsersAsync();
                    model.UsersByRole = await _userRepository.GetUserCountByRoleAsync();
                }



                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // ✅ Requires authentication
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation($"User {userName} logged out");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userRepository.GetByEmailAsync(model.Email);
                if (user != null && user.IsActive)
                {
                    // Generate reset token
                    var resetToken = _passwordService.GenerateResetToken();
                    user.ResetPasswordToken = resetToken;
                    user.ResetPasswordExpires = DateTime.UtcNow.AddHours(1);
                    user.UpdatedAt = DateTime.UtcNow;

                    await _userRepository.UpdateAsync(user);

                    // Send email
                    var resetLink = Url.Action("ResetPassword", "Account",
                        new { token = resetToken, email = model.Email }, Request.Scheme);
                    await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink!, user.FullName);

                    _logger.LogInformation($"Password reset email sent to {model.Email}");
                }

                TempData["SuccessMessage"] = "If an account with that email exists, a password reset link has been sent.";
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                ModelState.AddModelError("", "An error occurred. Please try again.");
            }

            return View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userRepository.GetByResetTokenAsync(model.Token);
                if (user == null || user.Email != model.Email)
                {
                    ModelState.AddModelError("", "Invalid or expired reset token.");
                    return View(model);
                }

                var (passwordHash, salt) = _passwordService.HashPassword(model.Password);
                var success = await _userRepository.UpdatePasswordAsync(user.Id, passwordHash, salt);

                if (success)
                {
                    _logger.LogInformation($"Password reset successfully for user {user.Username}");
                    TempData["SuccessMessage"] = "Your password has been reset successfully. Please log in.";
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError("", "Failed to reset password. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                ModelState.AddModelError("", "An error occurred. Please try again.");
            }

            return View(model);
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        [Authorize] // ✅ Requires authentication
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // ✅ Requires authentication
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                if (!_passwordService.VerifyPassword(model.CurrentPassword, user.PasswordHash, user.Salt))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                var (passwordHash, salt) = _passwordService.HashPassword(model.NewPassword);
                var success = await _userRepository.UpdatePasswordAsync(user.Id, passwordHash, salt);

                if (success)
                {
                    _logger.LogInformation($"Password changed successfully for user {user.Username}");
                    TempData["SuccessMessage"] = "Your password has been changed successfully.";
                    return RedirectToAction("Profile");
                }

                ModelState.AddModelError("", "Failed to change password. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                ModelState.AddModelError("", "An error occurred. Please try again.");
            }

            return View(model);
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize] // ✅ Requires authentication
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                return RedirectToAction("Dashboard");
            }
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [AllowAnonymous] // ✅ CRITICAL: Allow anonymous access
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
