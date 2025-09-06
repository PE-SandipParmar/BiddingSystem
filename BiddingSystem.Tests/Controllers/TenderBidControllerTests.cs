using BiddingSystem.Controllers;
using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BiddingSystem.Tests.Controllers
{
    public class TenderBidControllerTests
    {
        private readonly Mock<ITenderBidService> _mockTenderBidService;
        private readonly Mock<ITenderRepository> _mockTenderRepository;
        private readonly Mock<ILogger<TenderBidController>> _mockLogger;
        private readonly Mock<IInputValidationService> _mockInputValidation;
        private readonly Mock<ISecurityAuditService> _mockSecurityAudit;
        private readonly TenderBidController _controller;

        public TenderBidControllerTests()
        {
            _mockTenderBidService = new Mock<ITenderBidService>();
            _mockTenderRepository = new Mock<ITenderRepository>();
            _mockLogger = new Mock<ILogger<TenderBidController>>();
            _mockInputValidation = new Mock<IInputValidationService>();
            _mockSecurityAudit = new Mock<ISecurityAuditService>();

            _controller = new TenderBidController(
                _mockTenderBidService.Object,
                _mockTenderRepository.Object,
                _mockLogger.Object,
                _mockInputValidation.Object,
                _mockSecurityAudit.Object);

            // Setup HttpContext with user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // Setup TempData
            _controller.TempData = new TempDataDictionary(
                _controller.HttpContext,
                Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithBids()
        {
            // Arrange
            var bids = new List<TenderBid>
            {
                new TenderBid { Id = 1, BidderName = "Test Bidder", BidAmount = 1000 },
                new TenderBid { Id = 2, BidderName = "Another Bidder", BidAmount = 2000 }
            };

            _mockTenderBidService.Setup(x => x.GetBidsPagedAsync(1, 25, "", "", ""))
                .ReturnsAsync((bids, 2));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TenderBidListViewModel>(viewResult.Model);
            Assert.Equal(2, model.Bids.Count());
        }

        [Fact]
        public async Task Create_WithValidModel_ShouldCreateBidAndRedirect()
        {
            // Arrange
            var model = new TenderBidCreateViewModel
            {
                TenderId = 1,
                BidderName = "Test Bidder",
                BidderEmail = "test@example.com",
                BidderPhone = "1234567890",
                CompanyName = "Test Company",
                BidAmount = 1000,
                EmdAmount = 100,
                ProcessingFee = 50
            };

            var tender = new Tender { Id = 1, Status = TenderStatus.Published };
            _mockTenderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(tender);
            _mockTenderBidService.Setup(x => x.CheckDuplicateBidAsync(1, "test@example.com")).ReturnsAsync(false);

            // Setup validation to return success
            _mockInputValidation.Setup(x => x.ValidateEmail(It.IsAny<string>()))
                .Returns(ValidationResult.Success("test@example.com"));
            _mockInputValidation.Setup(x => x.ValidatePhoneNumber(It.IsAny<string>()))
                .Returns(ValidationResult.Success("1234567890"));
            _mockInputValidation.Setup(x => x.ValidateCompanyName(It.IsAny<string>()))
                .Returns(ValidationResult.Success("Test Company"));
            _mockInputValidation.Setup(x => x.ValidateBidderName(It.IsAny<string>()))
                .Returns(ValidationResult.Success("Test Bidder"));

            var createdBid = new TenderBid { Id = 1 };
            _mockTenderBidService.Setup(x => x.CreateBidAsync(It.IsAny<TenderBid>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["id"]);

            _mockTenderBidService.Verify(x => x.CreateBidAsync(It.IsAny<TenderBid>()), Times.Once);
            _mockSecurityAudit.Verify(x => x.LogUserActionAsync(It.IsAny<UserAction>()), Times.Once);
        }

        [Fact]
        public async Task Create_WithInvalidEmail_ShouldReturnViewWithError()
        {
            // Arrange
            var model = new TenderBidCreateViewModel
            {
                TenderId = 1,
                BidderEmail = "invalid-email"
            };

            var tender = new Tender { Id = 1, Status = TenderStatus.Published };
            _mockTenderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(tender);

            // Setup validation to return failure
            _mockInputValidation.Setup(x => x.ValidateEmail("invalid-email"))
                .Returns(ValidationResult.Failure("Invalid email format"));

            var tenders = new List<Tender> { tender };
            _mockTenderRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(tenders);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("BidderEmail", _controller.ModelState.Keys);
        }

        [Fact]
        public async Task Create_WithDuplicateBid_ShouldReturnViewWithError()
        {
            // Arrange
            var model = new TenderBidCreateViewModel
            {
                TenderId = 1,
                BidderEmail = "test@example.com"
            };

            var tender = new Tender { Id = 1, Status = TenderStatus.Published };
            _mockTenderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(tender);
            _mockTenderBidService.Setup(x => x.CheckDuplicateBidAsync(1, "test@example.com")).ReturnsAsync(true);

            // Setup validation to return success
            _mockInputValidation.Setup(x => x.ValidateEmail(It.IsAny<string>()))
                .Returns(ValidationResult.Success("test@example.com"));
            _mockInputValidation.Setup(x => x.ValidatePhoneNumber(It.IsAny<string>()))
                .Returns(ValidationResult.Success("1234567890"));
            _mockInputValidation.Setup(x => x.ValidateCompanyName(It.IsAny<string>()))
                .Returns(ValidationResult.Success("Test Company"));
            _mockInputValidation.Setup(x => x.ValidateBidderName(It.IsAny<string>()))
                .Returns(ValidationResult.Success("Test Bidder"));

            var tenders = new List<Tender> { tender };
            _mockTenderRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(tenders);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("BidderEmail", _controller.ModelState.Keys);
        }
    }
}
