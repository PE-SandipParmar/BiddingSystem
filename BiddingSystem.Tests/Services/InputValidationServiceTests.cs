using BiddingSystem.Data;
using BiddingSystem.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BiddingSystem.Tests.Services
{
    public class InputValidationServiceTests
    {
        private readonly Mock<ILogger<InputValidationService>> _mockLogger;
        private readonly Mock<IOptions<SecurityOptions>> _mockOptions;
        private readonly InputValidationService _service;

        public InputValidationServiceTests()
        {
            _mockLogger = new Mock<ILogger<InputValidationService>>();
            _mockOptions = new Mock<IOptions<SecurityOptions>>();
            
            var securityOptions = new SecurityOptions
            {
                FileUpload = new FileUploadSettings
                {
                    MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
                    AllowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".png" },
                    AllowedMimeTypes = new[] { "application/pdf", "image/jpeg", "image/png" }
                }
            };
            
            _mockOptions.Setup(x => x.Value).Returns(securityOptions);
            _service = new InputValidationService(_mockLogger.Object, _mockOptions.Object);
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("user.name+tag@domain.co.uk", true)]
        [InlineData("invalid-email", false)]
        [InlineData("@domain.com", false)]
        [InlineData("user@", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidateEmail_ShouldReturnCorrectResult(string email, bool expectedValid)
        {
            // Act
            var result = _service.ValidateEmail(email);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        [Theory]
        [InlineData("+1234567890", true)]
        [InlineData("1234567890", true)]
        [InlineData("+91-9876543210", false)]
        [InlineData("123", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidatePhoneNumber_ShouldReturnCorrectResult(string phone, bool expectedValid)
        {
            // Act
            var result = _service.ValidatePhoneNumber(phone);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        [Theory]
        [InlineData("ABC123", true)]
        [InlineData("TENDER-2024-001", true)]
        [InlineData("TENDER_2024_001", true)]
        [InlineData("abc", false)] // too short
        [InlineData("TENDER@2024", false)] // invalid character
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidateTenderId_ShouldReturnCorrectResult(string tenderId, bool expectedValid)
        {
            // Act
            var result = _service.ValidateTenderId(tenderId);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        [Theory]
        [InlineData("Acme Corporation", true)]
        [InlineData("ABC Corp & Associates", true)]
        [InlineData("Company-Name Ltd.", true)]
        [InlineData("A", false)] // too short
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidateCompanyName_ShouldReturnCorrectResult(string companyName, bool expectedValid)
        {
            // Act
            var result = _service.ValidateCompanyName(companyName);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        [Theory]
        [InlineData("John Doe", true)]
        [InlineData("Mary Jane Smith", true)]
        [InlineData("A", false)] // too short
        [InlineData("John123", false)] // contains numbers
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidateBidderName_ShouldReturnCorrectResult(string bidderName, bool expectedValid)
        {
            // Act
            var result = _service.ValidateBidderName(bidderName);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        [Theory]
        [InlineData(100.50, 0, 1000, true)]
        [InlineData(0, 0, 1000, true)]
        [InlineData(-10, 0, 1000, false)]
        [InlineData(1500, 0, 1000, false)]
        public void ValidateAmount_ShouldReturnCorrectResult(decimal amount, decimal minAmount, decimal maxAmount, bool expectedValid)
        {
            // Act
            var result = _service.ValidateAmount(amount, minAmount, maxAmount);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        [Fact]
        public void SanitizeHtml_ShouldRemoveDangerousContent()
        {
            // Arrange
            var html = "<script>alert('xss')</script><p>Safe content</p>";

            // Act
            var result = _service.SanitizeHtml(html);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain("script", result.SanitizedValue);
            Assert.Contains("Safe content", result.SanitizedValue);
        }
    }
}
