using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using System.Security.Claims;

namespace BiddingSystem.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IEmailService emailService,
            ILogger<AdminController> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var currentUser = await _userRepository.GetByIdAsync(userId);

                var model = new ViewModels.DashboardViewModel
                {
                   
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: /Admin/UserManagement
        public async Task<IActionResult> UserManagement(int page = 1, string? search = null, UserRole? role = null, bool? isActive = null)
        {
            try
            {
                const int pageSize = 10;
                var (users, totalCount) = await _userRepository.GetPagedUsersAsync(page, pageSize, search, role, isActive);

                var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue("UserRole")!);

                var model = new UserManagementViewModel
                {
                    Users = users,
                    CurrentUserRole = currentUserRole,
                    TotalUsers = totalCount,
                    ActiveUsers = await _userRepository.GetActiveUsersCountAsync(),
                    UsersByRole = await _userRepository.GetUserCountByRoleAsync()
                };

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                ViewBag.SearchTerm = search;
                ViewBag.SelectedRole = role;
                ViewBag.SelectedStatus = isActive;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user management");
                return RedirectToAction("Dashboard");
            }
        }

        // GET: /Admin/CreateUser
        public IActionResult CreateUser()
        {
            var model = new RegisterViewModel
            {
                IsAdminRegistration = true,
                Role = UserRole.Maker
            };
            return View(model);
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            model.IsAdminRegistration = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var existingUser = await _userRepository.ExistsAsync(model.Username, model.Email);
                if (existingUser)
                {
                    ModelState.AddModelError("", "A user with this username or email already exists.");
                    return View(model);
                }

                // Generate temporary password if not provided
                var password = string.IsNullOrEmpty(model.Password) ?
                    _passwordService.GenerateRandomPassword() : model.Password;

                var (passwordHash, salt) = _passwordService.HashPassword(password);

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Role = model.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PhoneNumber = model.PhoneNumber,
                    Department = model.Department,
                    CreatedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                };

                var userId = await _userRepository.CreateAsync(user);
                if (userId > 0)
                {
                    _logger.LogInformation($"User {model.Username} created by admin {User.Identity?.Name}");

                    // Send welcome email with temporary password if generated
                    var tempPassword = string.IsNullOrEmpty(model.Password) ? password : "";
                    await _emailService.SendWelcomeEmailAsync(
                        model.Email,
                        $"{model.FirstName} {model.LastName}",
                        model.Username,
                        tempPassword);

                    TempData["SuccessMessage"] = $"User {model.Username} has been created successfully.";
                    return RedirectToAction("UserManagement");
                }

                ModelState.AddModelError("", "Failed to create user. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "An error occurred while creating the user. Please try again.");
            }

            return View(model);
        }

        // GET: /Admin/EditUser/5
        public async Task<IActionResult> EditUser(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("UserManagement");
                }

                var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue("UserRole")!);

                var model = new EditUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Username = user.Username,
                    PhoneNumber = user.PhoneNumber,
                    Department = user.Department,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CurrentUserRole = currentUserRole,
                    AvailableRoles = UserRoleExtensions.GetSelectableRoles(currentUserRole)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading user for edit: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the user.";
                return RedirectToAction("UserManagement");
            }
        }

        // POST: /Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue("UserRole")!);
                model.CurrentUserRole = currentUserRole;
                model.AvailableRoles = UserRoleExtensions.GetSelectableRoles(currentUserRole);
                return View(model);
            }

            try
            {

                var user = await _userRepository.GetByIdAsync(model.Id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("UserManagement");
                }

                var originalRole = user.Role;

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Username = model.Username;
                user.PhoneNumber = model.PhoneNumber;
                user.Department = model.Department;
                user.Role = model.Role;
                user.IsActive = model.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                var success = await _userRepository.UpdateAsync(user);
                if (success)
                {
                    _logger.LogInformation($"User {model.Username} updated by admin {User.Identity?.Name}");

                    // Send notification if role changed
                    if (originalRole != model.Role)
                    {
                        await _emailService.SendRoleChangeNotificationAsync(
                            model.Email,
                            $"{model.FirstName} {model.LastName}",
                            model.Role.GetDisplayName());
                    }

                    TempData["SuccessMessage"] = $"User {model.Username} has been updated successfully.";
                    return RedirectToAction("UserManagement");
                }

                ModelState.AddModelError("", "Failed to update user. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user: {model.Id}");
                ModelState.AddModelError("", "An error occurred while updating the user. Please try again.");
            }

            var currentRole = Enum.Parse<UserRole>(User.FindFirstValue("UserRole")!);
            model.CurrentUserRole = currentRole;
            model.AvailableRoles = UserRoleExtensions.GetSelectableRoles(currentRole);
            return View(model);
        }

        // POST: /Admin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("UserManagement");
                }

                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (user.Id == currentUserId)
                {
                    TempData["ErrorMessage"] = "You cannot delete your own account.";
                    return RedirectToAction("UserManagement");
                }

                var success = await _userRepository.DeleteAsync(id);
                if (success)
                {
                    _logger.LogInformation($"User {user.Username} deleted by admin {User.Identity?.Name}");
                    TempData["SuccessMessage"] = $"User {user.Username} has been deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user: {id}");
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction("UserManagement");
        }

        // POST: /Admin/ToggleUserStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (user.Id == currentUserId)
                {
                    return Json(new { success = false, message = "You cannot deactivate your own account." });
                }

                var newStatus = !user.IsActive;
                var success = await _userRepository.ToggleUserStatusAsync(id, newStatus);

                if (success)
                {
                    _logger.LogInformation($"User {user.Username} status changed to {(newStatus ? "Active" : "Inactive")} by admin {User.Identity?.Name}");

                    if (newStatus)
                    {
                        await _emailService.SendAccountActivationEmailAsync(user.Email, $"{user.FirstName} {user.LastName}");
                    }

                    return Json(new
                    {
                        success = true,
                        message = $"User {user.Username} has been {(newStatus ? "activated" : "deactivated")} successfully.",
                        newStatus = newStatus ? "Active" : "Inactive"
                    });
                }

                return Json(new { success = false, message = "Failed to update user status." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling user status: {id}");
                return Json(new { success = false, message = "An error occurred while updating the user status." });
            }
        }

        // POST: /Admin/ResetUserPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var newPassword = _passwordService.GenerateRandomPassword();
                var (passwordHash, salt) = _passwordService.HashPassword(newPassword);

                var success = await _userRepository.UpdatePasswordAsync(user.Id, passwordHash, salt);
                if (success)
                {
                    _logger.LogInformation($"Password reset for user {user.Username} by admin {User.Identity?.Name}");

                    // Send email with new password
                    await _emailService.SendWelcomeEmailAsync(
                        user.Email,
                        $"{user.FirstName} {user.LastName}",
                        user.Username,
                        newPassword);

                    return Json(new
                    {
                        success = true,
                        message = $"Password has been reset for {user.Username}. A new password has been sent to their email."
                    });
                }

                return Json(new { success = false, message = "Failed to reset password." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for user: {id}");
                return Json(new { success = false, message = "An error occurred while resetting the password." });
            }
        }

        // GET: /Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("UserManagement");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading user details: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the user details.";
                return RedirectToAction("UserManagement");
            }
        }
    }
}
