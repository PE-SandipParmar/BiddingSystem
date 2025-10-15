using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BiddingSystem.Data;
using BiddingSystem.ViewModels;
using BiddingSystem.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BiddingSystem.Controllers
{
    [Authorize]
    public class RefundController : Controller
    {
        private readonly IRefundRepository _refundRepository;
        private readonly ILogger<RefundController> _logger;

        public RefundController(IRefundRepository refundRepository, ILogger<RefundController> logger)
        {
            _refundRepository = refundRepository;
            _logger = logger;
        }

        // GET: Refund/Index - Main page
        public IActionResult Index()
        {
            var userRole = GetUserRole();

            ViewBag.UserRole = userRole;
            ViewBag.CanInitiate = CanInitiateRefund(userRole);
            ViewBag.CanApprove = CanApproveRefund(userRole);

            return View();
        }

        // AJAX: Get refunds list
        [HttpGet]
        public async Task<IActionResult> GetRefundsList(
            string? tenderId,
            string? tenderName,
            int? paymentType,
            int page = 1,
            int pageSize = 10,
            bool showPendingOnly = false)
        {
            try
            {
                var userRole = GetUserRole();

                // Create filter from request parameters
                var filter = new RefundSearchFilter
                {
                    TenderId = tenderId,
                    TenderName = tenderName,
                    PaymentType = paymentType.HasValue ? (PaymentType)paymentType.Value : (PaymentType?)null,
                    PageNumber = page,
                    PageSize = pageSize,
                    ShowPendingOnly = showPendingOnly || (userRole == "Checker" || userRole == "Approver")
                };

                // Call repository method with userRole parameter
                var result = await _refundRepository.GetRefundListAsync(filter, userRole);

                // Transform the data for frontend
                var transformedData = result.Items.Select(item => new
                {
                    id = item.RefundPaymentId ?? 0,
                    tenderBidId = item.TenderBidId,
                    tenderId = item.TenderId,
                    tenderIdString = item.TenderIdString,
                    tenderName = item.TenderTitle,
                    bidderName = item.BidderName,
                    companyName = item.CompanyName,
                    paymentType = item.PaymentTypeDisplay,
                    paymentTypeValue = (int)item.PaymentType,
                    amount = item.PaymentAmount,
                    paymentId = item.RazorpayPaymentId,
                    paymentLinkId = item.PaymentLinkId,
                    status = GetRefundStatusDisplay(item),
                    actionStatus = GetActionStatus(item, userRole),
                    canSelect = CanSelectRefund(item, userRole),
                    razorpayRefundId = item.RazorpayRefundId,
                    refundErrorMessage = item.RefundErrorMessage
                }).ToList();

                // Calculate statistics with proper failed count
                var stats = new
                {
                    pending = result.Items.Count(x =>
                        string.Equals(x.RefundStatus, "Pending", StringComparison.OrdinalIgnoreCase) ||
                        (!x.HasPendingRefund && userRole == "Maker")),
                    approved = result.Items.Count(x =>
                        string.Equals(x.RefundStatus, "Approved", StringComparison.OrdinalIgnoreCase)),
                    failed = result.Items.Count(x =>
                        string.Equals(x.RefundStatus, "Failed", StringComparison.OrdinalIgnoreCase)),
                    totalAmount = result.Items.Sum(x => x.PaymentAmount)
                };

                return Json(new
                {
                    success = true,
                    data = transformedData,
                    totalPages = result.TotalPages,
                    totalRecords = result.TotalCount,
                    currentPage = result.CurrentPage,
                    stats = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refunds list");
                return Json(new { success = false, message = "Failed to load refunds: " + ex.Message });
            }
        }

        // AJAX: Initiate refunds (for Maker role)
        [HttpPost]
        public async Task<IActionResult> InitiateRefunds([FromBody] InitiateRefundsRequest request)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();

                // Validate permissions
                if (!CanInitiateRefund(userRole))
                {
                    return Json(new { success = false, message = "You don't have permission to initiate refunds" });
                }

                if (request.RefundItems == null || !request.RefundItems.Any())
                {
                    return Json(new { success = false, message = "No items selected for refund" });
                }

                // Validate the refund request
                var validationResult = await _refundRepository.ValidateRefundRequestAsync(request.RefundItems);
                if (!validationResult.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = validationResult.Errors
                    });
                }

                // Initiate refunds
                var success = await _refundRepository.InitiateRefundsAsync(
                    request.RefundItems,
                    request.Reason ?? "Tender awarded to another bidder",
                    userId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully initiated {request.RefundItems.Count} refund(s) for approval"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to initiate refunds" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating refunds");
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // AJAX: Process refunds (Approve/Reject for Checker role)
        [HttpPost]
        public async Task<IActionResult> ProcessRefunds([FromBody] ProcessRefundsRequest request)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();

                // Validate permissions
                if (!CanApproveRefund(userRole))
                {
                    return Json(new { success = false, message = "You don't have permission to approve/reject refunds" });
                }

                if (request.RefundIds == null || !request.RefundIds.Any())
                {
                    return Json(new { success = false, message = "No refunds selected" });
                }

                // Process refunds - this now handles both Pending and Failed status
                bool success = await _refundRepository.ProcessRefundsAsync(
                    request.RefundIds,
                    request.Action,
                    request.Reason ?? "",
                    userId);

                string message = request.Action == "Approve"
                    ? $"Successfully approved {request.RefundIds.Count} refund(s)"
                    : $"Successfully rejected {request.RefundIds.Count} refund(s)";

                return Json(new { success = success, message = message });
            }
            catch (PartialSuccessException psEx)
            {
                // Handle partial success
                var message = $"Partially completed: {psEx.SuccessfulRefunds.Count} succeeded, {psEx.FailedRefunds.Count} failed";
                var errors = psEx.FailedRefunds.Select(f => $"RefundId {f.RefundId}: {f.Error}").ToList();

                return Json(new
                {
                    success = psEx.SuccessfulRefunds.Any(),
                    message = message,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refunds");
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // GET: Approved refunds page
        [Authorize]
        public IActionResult ApprovedRefunds()
        {
            var userRole = GetUserRole();

            // Only Checker, Approver, and Admin can view approved refunds
            if (!CanApproveRefund(userRole))
            {
                return RedirectToAction("Index");
            }

            ViewBag.UserRole = userRole;
            return View();
        }

        // AJAX: Get approved refunds
        [HttpGet]
        public async Task<IActionResult> GetApprovedRefunds(
            string? tenderId,
            string? tenderName,
            int? paymentType,
            DateTime? fromDate,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var filter = new RefundSearchFilter
                {
                    TenderId = tenderId,
                    TenderName = tenderName,
                    PaymentType = paymentType.HasValue ? (PaymentType)paymentType.Value : (PaymentType?)null,
                    RefundStatus = "Approved",
                    FromDate = fromDate,
                    PageNumber = page,
                    PageSize = pageSize
                };

                var result = await _refundRepository.GetRefundsByStatusAsync(filter);

                return Json(new
                {
                    success = true,
                    data = result.Items,
                    totalPages = result.TotalPages,
                    totalRecords = result.TotalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading approved refunds");
                return Json(new { success = false, message = "Failed to load approved refunds" });
            }
        }

        // AJAX: Retry failed refund (single)
        [HttpPost]
        public async Task<IActionResult> RetryFailedRefund([FromBody] RetryRefundRequest request)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();

                if (!CanApproveRefund(userRole))
                {
                    return Json(new { success = false, message = "You don't have permission to retry refunds" });
                }

                var result = await _refundRepository.RetryFailedRefundAsync(request.RefundPaymentId, userId);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    razorpayRefundId = result.RazorpayRefundId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying refund");
                return Json(new { success = false, message = "Failed to retry refund: " + ex.Message });
            }
        }

        // AJAX: Bulk retry failed refunds (NEW)
        [HttpPost]
        public async Task<IActionResult> BulkRetryFailedRefunds([FromBody] BulkRetryRequest request)
        {
            try
            {
                var userRole = GetUserRole();
                var userId = GetUserId();

                if (!CanApproveRefund(userRole))
                {
                    return Json(new { success = false, message = "You don't have permission to retry refunds" });
                }

                if (request.RefundPaymentIds == null || !request.RefundPaymentIds.Any())
                {
                    return Json(new { success = false, message = "No refunds selected for retry" });
                }

                var result = await _refundRepository.BulkRetryFailedRefundsAsync(request.RefundPaymentIds, userId);

                var message = result.SuccessfulCount > 0
                    ? $"Retry completed: {result.SuccessfulCount} succeeded, {result.FailedCount} failed"
                    : "All retry attempts failed";

                return Json(new
                {
                    success = result.SuccessfulCount > 0,
                    message = message,
                    successCount = result.SuccessfulCount,
                    failedCount = result.FailedCount,
                    results = result.Results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk retry");
                return Json(new { success = false, message = "Failed to retry refunds: " + ex.Message });
            }
        }

        // AJAX: Get refund statistics
        [HttpGet]
        public async Task<IActionResult> GetRefundStatistics()
        {
            try
            {
                var stats = await _refundRepository.GetRefundStatisticsAsync();
                var statsByType = await _refundRepository.GetRefundStatisticsByPaymentTypeAsync();

                return Json(new
                {
                    success = true,
                    overall = stats,
                    byPaymentType = statsByType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading statistics");
                return Json(new { success = false, message = "Failed to load statistics" });
            }
        }

        // Helper methods
        private string GetUserRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role))
            {
                if (User.IsInRole("Admin")) return "Admin";
                if (User.IsInRole("Checker")) return "Checker";
                if (User.IsInRole("Approver")) return "Approver";
                if (User.IsInRole("Maker")) return "Maker";
            }

            return role ?? "Maker";
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private bool CanInitiateRefund(string role)
        {
            return role == "Maker" || role == "Admin";
        }

        private bool CanApproveRefund(string role)
        {
            return role == "Checker" || role == "Maker" || role == "Admin";
        }

        private string GetRefundStatusDisplay(RefundListViewModel item)
        {
            if (item.HasPendingRefund)
            {
                return item.RefundStatus ?? "Pending";
            }
            return "Pending";
        }

        private string GetActionStatus(RefundListViewModel item, string userRole)
        {
            // Handle Failed status - UPDATED
            if (string.Equals(item.RefundStatus, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                return "Retry Required";
            }

            if (userRole == "Checker" || userRole == "Approver")
            {
                if (item.HasPendingRefund && string.Equals(item.RefundStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                {
                    return "Awaiting Action";
                }
                return "-";
            }
            else if (userRole == "Maker")
            {
                return !item.HasPendingRefund ? "Ready to Initiate" : "Awaiting Action";
            }
            return "-";
        }

        private bool CanSelectRefund(RefundListViewModel item, string userRole)
        {
            // Debug logging
            _logger.LogDebug($"CanSelectRefund - Role: {userRole}, HasPendingRefund: {item.HasPendingRefund}, Status: {item.RefundStatus}");

            if (userRole == "Checker" || userRole == "Approver")
            {
                // Checker can select both pending and failed refunds - UPDATED
                bool canSelect = item.HasPendingRefund &&
                                 (string.Equals(item.RefundStatus, "Pending", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(item.RefundStatus, "Failed", StringComparison.OrdinalIgnoreCase));
                _logger.LogDebug($"Checker/Approver can select: {canSelect}");
                return canSelect;
            }
            else if (userRole == "Maker")
            {
                // Maker can select items not yet initiated
                return !item.HasPendingRefund;
            }
            else if (userRole == "Admin")
            {
                // Admin can select any pending or failed item - UPDATED
                return !item.HasPendingRefund ||
                       string.Equals(item.RefundStatus, "Pending", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(item.RefundStatus, "Failed", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }

    // Request/Response models
    public class InitiateRefundsRequest
    {
        public List<RefundRequestItem> RefundItems { get; set; } = new List<RefundRequestItem>();
        public string? Reason { get; set; }
    }

    public class ProcessRefundsRequest
    {
        public List<int> RefundIds { get; set; } = new List<int>();
        public string Action { get; set; } = ""; // Approve or Reject
        public string? Reason { get; set; }
    }

    public class RetryRefundRequest
    {
        public int RefundPaymentId { get; set; }
    }

    // NEW: Bulk retry request model
    public class BulkRetryRequest
    {
        public List<int> RefundPaymentIds { get; set; } = new List<int>();
    }
}