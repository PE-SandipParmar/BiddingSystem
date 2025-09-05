using Microsoft.Extensions.Options;
using BiddingSystem.Models;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;

namespace BiddingSystem.Data
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordOptions _options;
        private readonly ILogger<PasswordService> _logger;

        // Common passwords list (top 1000 most common passwords)
        private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "123456", "password123", "admin", "qwerty", "letmein", "welcome",
            "monkey", "1234567890", "abc123", "111111", "dragon", "master", "sunshine",
            "superman", "123123", "football", "baseball", "princess", "shadow",
            "passw0rd", "pass123", "admin123", "root", "test", "guest", "user",
            "demo", "sample", "temp", "default", "changeme", "mustchange"
        };

        public PasswordService(IOptions<PasswordOptions> options, ILogger<PasswordService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public (string hash, string salt) HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                // Generate a cryptographically random salt
                using var rng = RandomNumberGenerator.Create();
                var saltBytes = new byte[32]; // 256-bit salt
                rng.GetBytes(saltBytes);
                var salt = Convert.ToBase64String(saltBytes);

                // Use PBKDF2 with SHA-256 for password hashing
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
                var hashBytes = pbkdf2.GetBytes(32); // 256-bit hash
                var hash = Convert.ToBase64String(hashBytes);

                _logger.LogDebug("Password hashed successfully");
                return (hash, salt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw new InvalidOperationException("Failed to hash password", ex);
            }
        }

        public bool VerifyPassword(string password, string hash, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
                return false;

            try
            {
                var saltBytes = Convert.FromBase64String(salt);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
                var testHashBytes = pbkdf2.GetBytes(32);
                var testHash = Convert.ToBase64String(testHashBytes);

                // Use constant-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(hash),
                    Convert.FromBase64String(testHash)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }

        public bool IsPasswordStrong(string password)
        {
            var result = ValidatePassword(password);
            return result.IsValid && result.Strength >= PasswordStrength.Good;
        }

        public PasswordValidationResult ValidatePassword(string password, string? username = null, string? email = null)
        {
            var result = new PasswordValidationResult();

            if (string.IsNullOrEmpty(password))
            {
                result.Errors.Add("Password is required");
                result.Strength = PasswordStrength.VeryWeak;
                return result;
            }

            // Check length
            if (password.Length < _options.MinimumLength)
                result.Errors.Add($"Password must be at least {_options.MinimumLength} characters long");

            if (password.Length > _options.MaximumLength)
                result.Errors.Add($"Password cannot exceed {_options.MaximumLength} characters");

            // Check character requirements
            if (_options.RequireUppercase && !Regex.IsMatch(password, @"[A-Z]"))
            {
                result.Errors.Add("Password must contain at least one uppercase letter");
                result.Suggestions.Add("Add uppercase letters (A-Z)");
            }

            if (_options.RequireLowercase && !Regex.IsMatch(password, @"[a-z]"))
            {
                result.Errors.Add("Password must contain at least one lowercase letter");
                result.Suggestions.Add("Add lowercase letters (a-z)");
            }

            if (_options.RequireDigit && !Regex.IsMatch(password, @"\d"))
            {
                result.Errors.Add("Password must contain at least one number");
                result.Suggestions.Add("Add numbers (0-9)");
            }

            if (_options.RequireNonAlphanumeric && !Regex.IsMatch(password, @"[^\da-zA-Z]"))
            {
                result.Errors.Add("Password must contain at least one special character");
                result.Suggestions.Add($"Add special characters ({_options.AllowedSpecialChars})");
            }

            // Check unique characters
            var uniqueChars = password.Distinct().Count();
            if (uniqueChars < _options.RequiredUniqueChars)
            {
                result.Errors.Add($"Password must contain at least {_options.RequiredUniqueChars} unique characters");
                result.Suggestions.Add("Use more varied characters");
            }

            // Check for common passwords
            if (_options.ForbidCommonPasswords && IsCommonPassword(password))
            {
                result.Errors.Add("Password is too common and easily guessable");
                result.Suggestions.Add("Choose a more unique password");
            }

            // Check for user information in password
            if (_options.ForbidUserInfoInPassword)
            {
                if (!string.IsNullOrEmpty(username) && password.ToLower().Contains(username.ToLower()))
                {
                    result.Errors.Add("Password cannot contain your username");
                    result.Suggestions.Add("Remove username from password");
                }

                if (!string.IsNullOrEmpty(email))
                {
                    var emailUser = email.Split('@')[0];
                    if (password.ToLower().Contains(emailUser.ToLower()))
                    {
                        result.Errors.Add("Password cannot contain your email address");
                        result.Suggestions.Add("Remove email parts from password");
                    }
                }
            }

            // Check for sequential characters
            if (_options.ForbidSequentialChars && HasSequentialChars(password))
            {
                result.Errors.Add("Password cannot contain sequential characters (e.g., 123, abc)");
                result.Suggestions.Add("Avoid sequential patterns");
            }

            // Check for repeating characters
            if (_options.ForbidRepeatingChars && HasExcessiveRepeatingChars(password))
            {
                result.Errors.Add($"Password cannot have more than {_options.MaxConsecutiveIdenticalChars} consecutive identical characters");
                result.Suggestions.Add("Reduce character repetition");
            }

            // Calculate strength and entropy
            result.EntropyScore = CalculatePasswordEntropy(password);
            result.Strength = CalculatePasswordStrength(password, result.EntropyScore);

            // Check minimum entropy requirement
            if (result.EntropyScore < _options.MinEntropyBits)
            {
                result.Errors.Add($"Password complexity is too low (entropy: {result.EntropyScore:F1} bits, minimum: {_options.MinEntropyBits} bits)");
                result.Suggestions.Add("Increase password complexity with more varied characters");
            }

            result.IsValid = result.Errors.Count == 0;

            // Add strength-based suggestions
            if (result.Strength < PasswordStrength.Good && result.IsValid)
            {
                result.Suggestions.Add("Consider making your password longer");
                result.Suggestions.Add("Use a mix of different character types");
                result.Suggestions.Add("Avoid common words and patterns");
            }

            return result;
        }

        public string GenerateRandomPassword(int length = 12)
        {
            return GenerateSecurePassword(new PasswordGenerationOptions { Length = length });
        }

        public string GenerateSecurePassword(PasswordGenerationOptions? options = null)
        {
            options ??= new PasswordGenerationOptions();

            var characterSets = new List<string>();

            if (options.IncludeUppercase)
                characterSets.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            if (options.IncludeLowercase)
                characterSets.Add("abcdefghijklmnopqrstuvwxyz");

            if (options.IncludeNumbers)
                characterSets.Add("0123456789");

            if (options.IncludeSpecialChars)
                characterSets.Add(_options.AllowedSpecialChars);

            if (!string.IsNullOrEmpty(options.CustomCharacters))
                characterSets.Add(options.CustomCharacters);

            if (characterSets.Count == 0)
                throw new ArgumentException("At least one character type must be selected");

            var allChars = string.Concat(characterSets);

            // Remove similar/ambiguous characters if requested
            if (options.ExcludeSimilarChars)
                allChars = RemoveSimilarChars(allChars);

            if (options.ExcludeAmbiguousChars)
                allChars = RemoveAmbiguousChars(allChars);

            using var rng = RandomNumberGenerator.Create();
            var password = new StringBuilder();

            // Ensure at least one character from each required character set
            foreach (var charSet in characterSets)
            {
                if (password.Length < options.Length)
                {
                    var availableChars = options.ExcludeSimilarChars ? RemoveSimilarChars(charSet) : charSet;
                    if (options.ExcludeAmbiguousChars)
                        availableChars = RemoveAmbiguousChars(availableChars);

                    if (availableChars.Length > 0)
                        password.Append(availableChars[GetRandomIndex(rng, availableChars.Length)]);
                }
            }

            // Fill the rest of the password
            while (password.Length < options.Length)
            {
                password.Append(allChars[GetRandomIndex(rng, allChars.Length)]);
            }

            // Shuffle the password to avoid predictable patterns
            var passwordArray = password.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = GetRandomIndex(rng, i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            var finalPassword = new string(passwordArray);

            // Validate generated password doesn't contain excluded words
            if (options.ExcludeWords.Any(word => finalPassword.ToLower().Contains(word.ToLower())))
            {
                // Regenerate if it contains excluded words
                return GenerateSecurePassword(options);
            }

            return finalPassword;
        }

        public string GenerateResetToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32]; // 256-bit token
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public string GenerateVerificationToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[16]; // 128-bit token
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public PasswordPolicyResult CheckPasswordPolicy(string password)
        {
            var result = new PasswordPolicyResult();
            var validationResult = ValidatePassword(password);

            result.MeetsPolicy = validationResult.IsValid;
            result.PolicyViolations = validationResult.Errors;
            result.Strength = validationResult.Strength;

            // Calculate policy score (0-100)
            var score = 0;

            if (password.Length >= _options.MinimumLength) score += 20;
            if (Regex.IsMatch(password, @"[A-Z]")) score += 15;
            if (Regex.IsMatch(password, @"[a-z]")) score += 15;
            if (Regex.IsMatch(password, @"\d")) score += 15;
            if (Regex.IsMatch(password, @"[^\da-zA-Z]")) score += 15;
            if (validationResult.EntropyScore >= _options.MinEntropyBits) score += 20;

            result.Score = Math.Min(100, score);

            return result;
        }

        public bool IsPasswordExpired(DateTime passwordCreatedDate, int maxDays = 90)
        {
            if (maxDays <= 0) return false; // No expiration policy

            var expirationDate = passwordCreatedDate.AddDays(maxDays);
            return DateTime.UtcNow > expirationDate;
        }

        public double CalculatePasswordEntropy(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            var characterSetSize = 0;

            if (Regex.IsMatch(password, @"[a-z]"))
                characterSetSize += 26; // lowercase letters

            if (Regex.IsMatch(password, @"[A-Z]"))
                characterSetSize += 26; // uppercase letters

            if (Regex.IsMatch(password, @"\d"))
                characterSetSize += 10; // digits

            if (Regex.IsMatch(password, @"[^\da-zA-Z]"))
                characterSetSize += 32; // special characters (approximate)

            if (characterSetSize == 0)
                return 0;

            // Entropy = log2(characterSetSize) * passwordLength
            // Adjusted for character repetition and patterns
            var baseEntropy = Math.Log2(characterSetSize) * password.Length;

            // Reduce entropy for character repetition
            var uniqueChars = password.Distinct().Count();
            var repetitionFactor = (double)uniqueChars / password.Length;

            // Reduce entropy for common patterns
            var patternFactor = 1.0;
            if (HasSequentialChars(password)) patternFactor *= 0.8;
            if (HasExcessiveRepeatingChars(password)) patternFactor *= 0.7;
            if (IsCommonPassword(password)) patternFactor *= 0.5;

            return baseEntropy * repetitionFactor * patternFactor;
        }

        public bool IsCommonPassword(string password)
        {
            return CommonPasswords.Contains(password);
        }

        public bool CheckPasswordHistory(string newPasswordHash, List<string> previousPasswordHashes, int historyCount = 5)
        {
            if (previousPasswordHashes == null || previousPasswordHashes.Count == 0)
                return true; // No history to check against

            var recentHashes = previousPasswordHashes
                .TakeLast(Math.Min(historyCount, previousPasswordHashes.Count))
                .ToList();

            return !recentHashes.Contains(newPasswordHash);
        }

        #region Private Helper Methods

        private static PasswordStrength CalculatePasswordStrength(string password, double entropy)
        {
            return entropy switch
            {
                < 25 => PasswordStrength.VeryWeak,
                < 35 => PasswordStrength.Weak,
                < 45 => PasswordStrength.Fair,
                < 60 => PasswordStrength.Good,
                < 80 => PasswordStrength.Strong,
                _ => PasswordStrength.VeryStrong
            };
        }

        private bool HasSequentialChars(string password)
        {
            for (int i = 0; i < password.Length - 2; i++)
            {
                var char1 = password[i];
                var char2 = password[i + 1];
                var char3 = password[i + 2];

                // Check for ascending sequence
                if (char2 == char1 + 1 && char3 == char2 + 1)
                    return true;

                // Check for descending sequence
                if (char2 == char1 - 1 && char3 == char2 - 1)
                    return true;
            }
            return false;
        }

        private bool HasExcessiveRepeatingChars(string password)
        {
            var consecutiveCount = 1;
            var currentChar = password[0];

            for (int i = 1; i < password.Length; i++)
            {
                if (password[i] == currentChar)
                {
                    consecutiveCount++;
                    if (consecutiveCount > _options.MaxConsecutiveIdenticalChars)
                        return true;
                }
                else
                {
                    consecutiveCount = 1;
                    currentChar = password[i];
                }
            }
            return false;
        }

        private static string RemoveSimilarChars(string input)
        {
            const string similarChars = "0Oo1lI";
            return new string(input.Where(c => !similarChars.Contains(c)).ToArray());
        }

        private static string RemoveAmbiguousChars(string input)
        {
            const string ambiguousChars = "{}[]()/'\"`,~;.<>";
            return new string(input.Where(c => !ambiguousChars.Contains(c)).ToArray());
        }

        private static int GetRandomIndex(RandomNumberGenerator rng, int maxValue)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);
            return (int)(value % maxValue);
        }

        #endregion
    }
}
