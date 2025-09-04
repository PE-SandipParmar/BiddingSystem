using Dapper;
using BiddingSystem.Models;
using System.Data;
using System.Data.SqlClient;

namespace BiddingSystem.Data
{
    //public class UserRepository : IUserRepository
    //{
    //    private readonly string _connectionString;

    //    public UserRepository(IConfiguration configuration)
    //    {
    //        _connectionString = configuration.GetConnectionString("DefaultConnection");
    //    }

    //    public async Task<PagedResult<User>> GetUsersAsync(int pageNumber, int pageSize, string searchTerm = "")
    //    {
    //        using var connection = new SqlConnection(_connectionString);

    //        var offset = (pageNumber - 1) * pageSize;
    //        var whereClause = string.IsNullOrEmpty(searchTerm) ? "" :
    //            "WHERE FirstName LIKE @SearchTerm OR LastName LIKE @SearchTerm OR Email LIKE @SearchTerm";

    //        var countQuery = $"SELECT COUNT(*) FROM Users {whereClause}";
    //        var dataQuery = $@"
    //            SELECT * FROM Users 
    //            {whereClause}
    //            ORDER BY CreatedAt DESC 
    //            OFFSET @Offset ROWS 
    //            FETCH NEXT @PageSize ROWS ONLY";

    //        var parameters = new
    //        {
    //            SearchTerm = $"%{searchTerm}%",
    //            Offset = offset,
    //            PageSize = pageSize
    //        };

    //        var totalItems = await connection.QuerySingleAsync<int>(countQuery, parameters);
    //        var users = await connection.QueryAsync<User>(dataQuery, parameters);

    //        return new PagedResult<User>
    //        {
    //            Items = users.ToList(),
    //            TotalItems = totalItems,
    //            PageNumber = pageNumber,
    //            PageSize = pageSize
    //        };
    //    }

    //    public async Task<User?> GetUserByIdAsync(int id)
    //    {
    //        using var connection = new SqlConnection(_connectionString);
    //        return await connection.QuerySingleOrDefaultAsync<User>(
    //            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    //    }

    //    public async Task<int> CreateUserAsync(User user)
    //    {
    //        using var connection = new SqlConnection(_connectionString);
    //        var sql = @"
    //            INSERT INTO Users (FirstName, LastName, Email, Phone, CreatedAt)
    //            OUTPUT INSERTED.Id
    //            VALUES (@FirstName, @LastName, @Email, @Phone, @CreatedAt)";

    //        user.CreatedAt = DateTime.UtcNow;
    //        return await connection.QuerySingleAsync<int>(sql, user);
    //    }

    //    public async Task<bool> UpdateUserAsync(User user)
    //    {
    //        using var connection = new SqlConnection(_connectionString);
    //        var sql = @"
    //            UPDATE Users 
    //            SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
    //                Phone = @Phone, UpdatedAt = @UpdatedAt
    //            WHERE Id = @Id";

    //        user.UpdatedAt = DateTime.UtcNow;
    //        var rowsAffected = await connection.ExecuteAsync(sql, user);
    //        return rowsAffected > 0;
    //    }

    //    public async Task<bool> DeleteUserAsync(int id)
    //    {
    //        using var connection = new SqlConnection(_connectionString);
    //        var rowsAffected = await connection.ExecuteAsync(
    //            "DELETE FROM Users WHERE Id = @Id", new { Id = id });
    //        return rowsAffected > 0;
    //    }

    //    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    //    {
    //        using var connection = new SqlConnection(_connectionString);
    //        var sql = excludeUserId.HasValue
    //            ? "SELECT COUNT(*) FROM Users WHERE Email = @Email AND Id != @ExcludeUserId"
    //            : "SELECT COUNT(*) FROM Users WHERE Email = @Email";

    //        var count = await connection.QuerySingleAsync<int>(sql,
    //            new { Email = email, ExcludeUserId = excludeUserId });
    //        return count > 0;
    //    }
    //}

    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                WHERE Username = @Username AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                WHERE Email = @Email AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(User user)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO Users (FirstName, LastName, Email, Username, PasswordHash, Salt, Role, 
                                 IsActive, CreatedAt, PhoneNumber, Department, CreatedBy)
                VALUES (@FirstName, @LastName, @Email, @Username, @PasswordHash, @Salt, @Role, 
                        @IsActive, @CreatedAt, @PhoneNumber, @Department, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.QuerySingleAsync<int>(sql, user);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Users 
                SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                    Username = @Username, Role = @Role, UpdatedAt = @UpdatedAt,
                    ResetPasswordToken = @ResetPasswordToken, ResetPasswordExpires = @ResetPasswordExpires,
                    PhoneNumber = @PhoneNumber, Department = @Department, IsActive = @IsActive
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, user);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Users 
                SET IsActive = 0, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(string username, string email, int? excludeId = null)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT COUNT(1) 
                FROM Users 
                WHERE (Username = @Username OR Email = @Email) AND IsActive = 1";

            var parameters = new { Username = username, Email = email };

            //if (excludeId.HasValue)
            //{
            //    sql += " AND Id != @ExcludeId";
            //    parameters = new { Username = username, Email = email, ExcludeId = excludeId.Value };
            //}

            var count = await connection.QuerySingleAsync<int>(sql, parameters);
            return count > 0;
        }

        public async Task<User?> GetByResetTokenAsync(string token)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                WHERE ResetPasswordToken = @Token AND ResetPasswordExpires > @Now AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Token = token, Now = DateTime.UtcNow });
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash, string salt)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Users 
                SET PasswordHash = @PasswordHash, Salt = @Salt, UpdatedAt = @UpdatedAt,
                    ResetPasswordToken = NULL, ResetPasswordExpires = NULL
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = userId,
                PasswordHash = passwordHash,
                Salt = salt,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Users 
                SET LastLoginAt = @LastLoginAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = userId,
                LastLoginAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                ORDER BY CreatedAt DESC";

            var users = await connection.QueryAsync<User>(sql);
            return users.ToList();
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                WHERE Role = @Role AND IsActive = 1
                ORDER BY CreatedAt DESC";

            var users = await connection.QueryAsync<User>(sql, new { Role = role });
            return users.ToList();
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, UserRole role)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Users 
                SET Role = @Role, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = userId,
                Role = role,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Users 
                SET IsActive = @IsActive, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = userId,
                IsActive = isActive,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            using var connection = CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Users";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            using var connection = CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Users WHERE IsActive = 1";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<Dictionary<UserRole, int>> GetUserCountByRoleAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT Role, COUNT(1) as Count
                FROM Users 
                WHERE IsActive = 1
                GROUP BY Role";

            var result = await connection.QueryAsync<(UserRole Role, int Count)>(sql);
            return result.ToDictionary(x => x.Role, x => x.Count);
        }

        public async Task<List<User>> GetRecentUsersAsync(int days = 7, int limit = 10)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT TOP(@Limit) Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                WHERE CreatedAt >= @FromDate
                ORDER BY CreatedAt DESC";

            var fromDate = DateTime.UtcNow.AddDays(-days);
            var users = await connection.QueryAsync<User>(sql, new { Limit = limit, FromDate = fromDate });
            return users.ToList();
        }

        public async Task<int> GetRecentRegistrationsCountAsync(int days = 7)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT COUNT(1) 
                FROM Users 
                WHERE CreatedAt >= @FromDate";

            var fromDate = DateTime.UtcNow.AddDays(-days);
            return await connection.QuerySingleAsync<int>(sql, new { FromDate = fromDate });
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm, UserRole? role = null, bool? isActive = null)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                sql += " AND (FirstName LIKE @SearchTerm OR LastName LIKE @SearchTerm OR Email LIKE @SearchTerm OR Username LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (role.HasValue)
            {
                sql += " AND Role = @Role";
                parameters.Add("Role", role.Value);
            }

            if (isActive.HasValue)
            {
                sql += " AND IsActive = @IsActive";
                parameters.Add("IsActive", isActive.Value);
            }

            sql += " ORDER BY CreatedAt DESC";

            var users = await connection.QueryAsync<User>(sql, parameters);
            return users.ToList();
        }

        public async Task<(List<User> Users, int TotalCount)> GetPagedUsersAsync(int page, int pageSize, string? searchTerm = null, UserRole? role = null, bool? isActive = null)
        {
            using var connection = CreateConnection();

            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += " AND (FirstName LIKE @SearchTerm OR LastName LIKE @SearchTerm OR Email LIKE @SearchTerm OR Username LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (role.HasValue)
            {
                whereClause += " AND Role = @Role";
                parameters.Add("Role", role.Value);
            }

            if (isActive.HasValue)
            {
                whereClause += " AND IsActive = @IsActive";
                parameters.Add("IsActive", isActive.Value);
            }

            // Get total count
            var countSql = $"SELECT COUNT(1) FROM Users {whereClause}";
            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

            // Get paged users
            var offset = (page - 1) * pageSize;
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var usersSql = $@"
                SELECT Id, FirstName, LastName, Email, Username, PasswordHash, Salt, 
                       Role, IsActive, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpires,
                       PhoneNumber, Department, CreatedBy, LastLoginAt
                FROM Users 
                {whereClause}
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var users = await connection.QueryAsync<User>(usersSql, parameters);
            return (users.ToList(), totalCount);
        }
    }
}
