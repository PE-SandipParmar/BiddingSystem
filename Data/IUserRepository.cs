using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    //public interface IUserRepository
    //{
    //    Task<PagedResult<User>> GetUsersAsync(int pageNumber, int pageSize, string searchTerm = "");
    //    Task<User?> GetUserByIdAsync(int id);
    //    Task<int> CreateUserAsync(User user);
    //    Task<bool> UpdateUserAsync(User user);
    //    Task<bool> DeleteUserAsync(int id);
    //    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    //}

    public interface IUserRepository
    {
        // Basic CRUD operations
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<int> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string username, string email, int? excludeId = null);

        // Password related
        Task<User?> GetByResetTokenAsync(string token);
        Task<bool> UpdatePasswordAsync(int userId, string passwordHash, string salt);
        Task<bool> UpdateLastLoginAsync(int userId);

        // Role and user management
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(UserRole role);
        Task<bool> UpdateUserRoleAsync(int userId, UserRole role);
        Task<bool> ToggleUserStatusAsync(int userId, bool isActive);

        // Statistics and analytics
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<Dictionary<UserRole, int>> GetUserCountByRoleAsync();
        Task<List<User>> GetRecentUsersAsync(int days = 7, int limit = 10);
        Task<int> GetRecentRegistrationsCountAsync(int days = 7);

        // Search and filtering
        Task<List<User>> SearchUsersAsync(string searchTerm, UserRole? role = null, bool? isActive = null);
        Task<(List<User> Users, int TotalCount)> GetPagedUsersAsync(int page, int pageSize, string? searchTerm = null, UserRole? role = null, bool? isActive = null);
    }
}
