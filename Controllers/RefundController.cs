using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BiddingSystem.ViewModels;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using BiddingSystem.Data;

namespace BiddingSystem.Controllers
{
    [Authorize] // Simplified authorization - any authenticated user
    public class RefundController : Controller
    {
        private readonly IRefundRepository _refundRepository;
        private readonly ILogger<RefundController> _logger;

        public RefundController(IRefundRepository refundRepository, ILogger<RefundController> logger)
        {
            _refundRepository = refundRepository;
            _logger = logger;
        }

        // GET: Refund/Index
        public IActionResult Index()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Maker";

            ViewBag.UserRole = userRole;
            ViewBag.CanInitiate = userRole == "Maker" || userRole == "Admin";
            ViewBag.CanApprove = userRole == "Checker" || userRole == "Admin" || userRole == "Approver";

            _logger.LogInformation($"Refund Index accessed by user with role: {userRole}");

            return View();
        }

        // AJAX: Get refund list - with debugging
        [HttpGet]
        public async Task<IActionResult> GetRefundList(RefundSearchFilter filter)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Maker";

                _logger.LogInformation($"GetRefundList called - Role: {userRole}, Filter: TenderId={filter.TenderId}, TenderName={filter.TenderName}, Page={filter.PageNumber}");

                // Determine display mode based on role
                string displayMode = "Maker";

                if (userRole == "Checker" || userRole == "Approver")
                {
                    displayMode = "Checker";
                }
                else if (userRole == "Admin" && filter.ShowPendingOnly)
                {
                    displayMode = "Checker";
                }

                _logger.LogInformation($"Display mode: {displayMode}");

                var refunds = await _refundRepository.GetRefundListAsync(filter, displayMode);

                _logger.LogInformation($"Refunds retrieved: {refunds?.Items?.Count ?? 0} items");

                
                return Json(new
                {
                    success = true,
                    data = refunds,
                    userRole = userRole,
                    canInitiate = userRole == "Maker" || userRole == "Admin",
                    canApprove = userRole == "Checker" || userRole == "Admin" || userRole == "Approver",
                    debug = new
                    {
                        totalCount = refunds?.TotalCount ?? 0,
                        itemCount = refunds?.Items?.Count ?? 0,
                        displayMode = displayMode
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRefundList");
                return Json(new
                {
                    success = false,
                    message = "Error loading refund list: " + ex.Message,
                    error = ex.ToString()
                });
            }
        }

        // Test method to check database connectivity
        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var filter = new RefundSearchFilter { PageNumber = 1, PageSize = 5 };
                var result = await _refundRepository.GetRefundListAsync(filter, "Maker");

                return Json(new
                {
                    success = true,
                    message = "Connection successful",
                    recordCount = result?.TotalCount ?? 0
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Connection failed: " + ex.Message
                });
            }
        }

        // GET: Refund/ApprovedRefunds - View for approved refunds (Checker/Admin only)
        [Authorize]
        public IActionResult ApprovedRefunds()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            // Only Checker, Approver, and Admin can view approved refunds
            if (userRole != "Checker" && userRole != "Admin" && userRole != "Approver")
            {
                return RedirectToAction("Index");
            }

            ViewBag.UserRole = userRole;
            return View();
        }

        // AJAX: Get approved refunds list
        [HttpGet]
        public async Task<IActionResult> GetApprovedRefundsList(RefundSearchFilter filter)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                // Check permission
                if (userRole != "Checker" && userRole != "Admin" && userRole != "Approver")
                {
                    return Json(new { success = false, message = "Unauthorized access" });
                }

                filter.RefundStatus = "Approved"; // Force to show only approved
                var refunds = await _refundRepository.GetRefundsByStatusAsync(filter);

                return Json(new
                {
                    success = true,
                    data = refunds,
                    userRole = userRole
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetApprovedRefundsList");
                return Json(new { success = false, message = "Error loading approved refunds: " + ex.Message });
            }
        }

        // Rest of the methods remain the same...

        [HttpPost]
        public async Task<IActionResult> InitiateRefunds([FromBody] InitiateRefundRequest request)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                if (userRole != "Maker" && userRole != "Admin")
                {
                    return Json(new { success = false, message = "You don't have permission to initiate refunds." });
                }

                if (request.TenderBidIds == null || !request.TenderBidIds.Any())
                {
                    return Json(new { success = false, message = "Please select at least one bidder for refund." });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
                var result = await _refundRepository.InitiateRefundsAsync(
                    request.TenderBidIds,
                    request.ReasonForRefund,
                    userId
                );

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Refund request(s) initiated successfully for {request.TenderBidIds.Count} bidder(s)!"
                    });
                }

                return Json(new { success = false, message = "Failed to initiate refund requests." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitiateRefunds");
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessRefunds([FromBody] ApproveRefundRequest request)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                if (userRole != "Checker" && userRole != "Admin" && userRole != "Approver")
                {
                    return Json(new { success = false, message = "You don't have permission to approve/reject refunds." });
                }

                if (request.RefundPaymentIds == null || !request.RefundPaymentIds.Any())
                {
                    return Json(new { success = false, message = "Please select at least one refund to process." });
                }

                if (string.IsNullOrEmpty(request.Action))
                {
                    return Json(new { success = false, message = "Please specify an action (Approve or Reject)." });
                }

                if (request.Action == "Reject" && string.IsNullOrWhiteSpace(request.CheckerRemarks))
                {
                    return Json(new { success = false, message = "Remarks are required when rejecting refunds." });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
                var result = await _refundRepository.ProcessRefundsAsync(
                    request.RefundPaymentIds,
                    request.Action,
                    request.CheckerRemarks ?? "",
                    userId
                );

                if (result)
                {
                    var actionText = request.Action == "Approve" ? "approved" : "rejected";
                    return Json(new
                    {
                        success = true,
                        message = $"Refund(s) {actionText} successfully for {request.RefundPaymentIds.Count} record(s)!"
                    });
                }

                return Json(new { success = false, message = "Failed to process refund requests." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessRefunds");
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}