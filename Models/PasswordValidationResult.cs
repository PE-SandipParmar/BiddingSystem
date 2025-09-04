namespace BiddingSystem.Models
{
    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public PasswordStrength Strength { get; set; }
        public double EntropyScore { get; set; }
        public List<string> Suggestions { get; set; } = new();
    }

    public class PasswordPolicyResult
    {
        public bool MeetsPolicy { get; set; }
        public List<string> PolicyViolations { get; set; } = new();
        public int Score { get; set; } // 0-100
        public PasswordStrength Strength { get; set; }
    }

    public class PasswordGenerationOptions
    {
        public int Length { get; set; } = 12;
        public bool IncludeUppercase { get; set; } = true;
        public bool IncludeLowercase { get; set; } = true;
        public bool IncludeNumbers { get; set; } = true;
        public bool IncludeSpecialChars { get; set; } = true;
        public bool ExcludeSimilarChars { get; set; } = true; // Exclude 0, O, l, 1, etc.
        public bool ExcludeAmbiguousChars { get; set; } = true; // Exclude {, }, [, ], (, ), /, \, ', ", `, ~, ,, ;, ., <, >
        public string? CustomCharacters { get; set; }
        public List<string> ExcludeWords { get; set; } = new();
    }

    public enum PasswordStrength
    {
        VeryWeak = 1,
        Weak = 2,
        Fair = 3,
        Good = 4,
        Strong = 5,
        VeryStrong = 6
    }
}
