using System.ComponentModel.DataAnnotations;

namespace BiddingSystem.Models
{
    public enum UserRole
    {
        [Display(Name = "Maker")]
        Maker = 1,

        [Display(Name = "Checker")]
        Checker = 2,

        [Display(Name = "Admin")]
        Admin = 3
    }

    public static class UserRoleExtensions
    {
        public static string GetDisplayName(this UserRole role)
        {
            return role switch
            {
                UserRole.Maker => "Maker",
                UserRole.Checker => "Checker",
                UserRole.Admin => "Admin",
                _ => "Unknown"
            };
        }

        public static List<UserRole> GetSelectableRoles(UserRole currentUserRole)
        {
            return currentUserRole switch
            {
                UserRole.Admin => new List<UserRole> { UserRole.Checker, UserRole.Maker, UserRole.Admin },
                UserRole.Checker => new List<UserRole> { UserRole.Checker },
                UserRole.Maker => new List<UserRole> { UserRole.Maker },
                _ => new List<UserRole> { UserRole.Maker }
            };
        }
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Checker = "Checker";
        public const string Maker = "Maker";

        public const string AdminOrEmployee = Admin + "," + Maker;
        public const string AdminOrVendor = Admin + "," + Checker;
        public const string All = Admin + "," + Maker + "," + Checker;
    }
}
