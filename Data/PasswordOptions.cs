namespace BiddingSystem.Data
{
    public class PasswordOptions
    {
        public int MinimumLength { get; set; } = 8;
        public int MaximumLength { get; set; } = 128;
        public int RequiredUniqueChars { get; set; } = 1;

        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;

        public int PasswordHistoryCount { get; set; } = 5;
        public int PasswordExpirationDays { get; set; } = 90;

        public bool ForbidCommonPasswords { get; set; } = true;
        public bool ForbidUserInfoInPassword { get; set; } = true;
        public bool ForbidSequentialChars { get; set; } = true;
        public bool ForbidRepeatingChars { get; set; } = true;

        public int MaxConsecutiveIdenticalChars { get; set; } = 2;
        public int MinEntropyBits { get; set; } = 50;

        public string AllowedSpecialChars { get; set; } = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    }
}
