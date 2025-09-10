using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using System.Security.Claims;

namespace BiddingSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IRefundRepository _refundRepository;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService,
            IRefundRepository refundRepository,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _refundRepository = refundRepository;
            _logger = logger;
        }

        // GET: Dashboard/Index
        [HttpGet]
        public async Task<IActionResult> Index(int? year)
        {
            try
            {
                var currentYear = year ?? DateTime.Now.Year;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                var dashboardData = await _dashboardService.GetDashboardDataAsync(userId, userRole, currentYear);
                var refundData = await _refundRepository.GetRefundStatisticsAsync();
                var viewModel = new DashboardViewModel
                {
                    Statistics = dashboardData.Statistics,
                    RecentTenders = dashboardData.RecentTenders,
                    RecentBids = dashboardData.RecentBids,
                    PendingRefunds = dashboardData.PendingRefunds,
                    ActiveTenders = dashboardData.ActiveTenders,
                    ExpiringTenders = dashboardData.ExpiringTenders,
                    SelectedYear = currentYear,
                    AvailableYears = await _dashboardService.GetAvailableYearsAsync(),
                    RefundStatistics = refundData
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["ErrorMessage"] = "An error occurred while loading dashboard data.";
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics(int year)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                
                var statistics = await _dashboardService.GetStatisticsAsync(userId, userRole, year);
                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading statistics for year {Year}", year);
                return Json(new { error = "Failed to load statistics" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                
                var recentActivity = await _dashboardService.GetRecentActivityAsync(userId, userRole);
                return Json(recentActivity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent activity");
                return Json(new { error = "Failed to load recent activity" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTenderStatusDistribution()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                
                var distribution = await _dashboardService.GetTenderStatusDistributionAsync(userId, userRole);
                return Json(distribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender status distribution");
                return Json(new { error = "Failed to load tender status distribution" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBidStatusDistribution()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                
                var distribution = await _dashboardService.GetBidStatusDistributionAsync(userId, userRole);
                return Json(distribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bid status distribution");
                return Json(new { error = "Failed to load bid status distribution" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyTrends(int year)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                
                var trends = await _dashboardService.GetMonthlyTrendsAsync(userId, userRole, year);
                return Json(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading monthly trends for year {Year}", year);
                return Json(new { error = "Failed to load monthly trends" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugRefunds()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                
                var debugInfo = await _dashboardService.GetRefundDebugInfoAsync();
                return Json(debugInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refund debug info");
                return Json(new { error = "Failed to load refund debug info" });
            }
        }
    }
}
