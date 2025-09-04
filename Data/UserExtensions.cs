using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public static class UserExtensions
    {
        public static string GetRoleBadgeClass(this User user)
        {
            return user.Role switch
            {
                UserRole.Admin => "bg-danger",
                UserRole.Maker => "bg-primary",
                UserRole.Checker => "bg-warning",
                _ => "bg-secondary"
            };
        }

        public static string GetStatusBadgeClass(this User user)
        {
            return user.IsActive ? "bg-success" : "bg-danger";
        }

        public static bool CanEditUser(this User currentUser, User targetUser)
        {
            // Admin can edit anyone except cannot demote themselves
            if (currentUser.Role == UserRole.Admin)
            {
                return true;
            }

            // Employees and Vendors cannot edit other users
            return false;
        }

        public static bool CanDeleteUser(this User currentUser, User targetUser)
        {
            // Cannot delete yourself
            if (currentUser.Id == targetUser.Id)
                return false;

            // Only admin can delete users
            return currentUser.Role == UserRole.Admin;
        }

        public static List<UserRole> GetEditableRoles(this User currentUser)
        {
            return currentUser.Role switch
            {
                UserRole.Admin => new List<UserRole> { UserRole.Maker, UserRole.Checker, UserRole.Admin },
                _ => new List<UserRole> { currentUser.Role }
            };
        }
    }
}
