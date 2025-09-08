using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using System.Security.Claims;

namespace BiddingSystem.Controllers
{
    [Authorize]
    public class RefundController : Controller
    {
        private readonly IRefundService _refundService;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IPaymentLinkRepository _paymentLinkRepository;
        private readonly ILogger<RefundController> _logger;
        private readonly IInputValidationService _inputValidation;
        private readonly ISecurityAuditService _securityAudit;

        public RefundController(
            IRefundService refundService,
            ITenderBidRepository tenderBidRepository,
            IPaymentLinkRepository paymentLinkRepository,
            ILogger<RefundController> logger,
            IInputValidationService inputValidation,
            ISecurityAuditService securityAudit)
        {
            _refundService = refundService;
            _tenderBidRepository = tenderBidRepository;
            _paymentLinkRepository = paymentLinkRepository;
            _logger = logger;
            _inputValidation = inputValidation;
            _securityAudit = securityAudit;
        }

        #region Index and List Views

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> Index(
            string searchTerm = "", 
            RefundStatus? status = null, 
            RefundType? type = null, 
            RefundReason? reason = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            int page = 1, 
            int pageSize = 25)
        {
            try
            {
                var model = await _refundService.GetRefundRequestsAsync(
                    searchTerm, status, type, reason, fromDate, toDate, page, pageSize);

                ViewBag.SearchTerm = searchTerm;
                ViewBag.Status = status;
                ViewBag.Type = type;
                ViewBag.Reason = reason;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refund requests index");
                TempData["ErrorMessage"] = "An error occurred while loading refund requests.";
                return View(new RefundRequestListViewModel());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> MyRefunds(int page = 1, int pageSize = 25)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                if (string.IsNullOrEmpty(userEmail))
                {
                    TempData["ErrorMessage"] = "User email not found.";
                    return RedirectToAction("Index");
                }

                var model = await _refundService.GetRefundRequestsByUserAsync(userEmail, page, pageSize);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user refund requests");
                TempData["ErrorMessage"] = "An error occurred while loading your refund requests.";
                return View(new RefundRequestListViewModel());
            }
        }

        #endregion

        #region Details

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var refundRequest = await _refundService.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    TempData["ErrorMessage"] = "Refund request not found.";
                    return RedirectToAction("Index");
                }

                var transactions = await _refundService.GetRefundTransactionsByRequestIdAsync(id);
                
                var model = new RefundRequestDetailsViewModel
                {
                    RefundRequest = refundRequest,
                    Transactions = transactions.Select(t => new RefundTransactionViewModel
                    {
                        Id = t.Id,
                        RefundRequestId = t.RefundRequestId,
                        TransactionReference = t.TransactionReference,
                        Amount = t.Amount,
                        Status = t.Status,
                        ProcessedAt = t.ProcessedAt,
                        BankResponse = t.BankResponse,
                        FailureReason = t.FailureReason,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        CreatedBy = t.CreatedBy
                    }).ToList(),
                    CanEdit = CanEditRefund(refundRequest),
                    CanApprove = CanApproveRefund(refundRequest),
                    CanReject = CanRejectRefund(refundRequest),
                    CanProcess = CanProcessRefund(refundRequest),
                    // Checker-Maker Workflow Permissions
                    CanSubmitForFirstCheck = CanSubmitForFirstCheck(refundRequest),
                    CanFirstCheck = CanFirstCheck(refundRequest),
                    CanSubmitForSecondCheck = CanSubmitForSecondCheck(refundRequest),
                    CanSecondCheck = CanSecondCheck(refundRequest),
                    CanMarkReadyForProcessing = CanMarkReadyForProcessing(refundRequest)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refund request details: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading refund request details.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var refundRequest = await _refundService.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    TempData["ErrorMessage"] = "Refund request not found.";
                    return RedirectToAction("Index");
                }

                if (!CanEditRefund(refundRequest))
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit this refund request.";
                    return RedirectToAction("Details", new { id });
                }

                var model = new RefundRequestEditViewModel
                {
                    Id = refundRequest.Id,
                    RefundId = refundRequest.RefundId,
                    RequestedBy = refundRequest.RequestedBy,
                    RequestedAt = refundRequest.RequestedAt,
                    Type = refundRequest.Type,
                    Reason = refundRequest.Reason,
                    RequestedAmount = refundRequest.RequestedAmount,
                    ApprovedAmount = refundRequest.ApprovedAmount,
                    Status = refundRequest.Status,
                    RefundReference = refundRequest.RefundReference,
                    Remarks = refundRequest.Remarks,
                    BankDetails = refundRequest.BankDetails,
                    TenderBid = refundRequest.TenderBid != null ? new TenderBidViewModel
                    {
                        Id = refundRequest.TenderBid.Id,
                        TenderId = refundRequest.TenderBid.TenderId,
                        BidderName = refundRequest.TenderBid.BidderName,
                        CompanyName = refundRequest.TenderBid.CompanyName,
                        TotalAmount = refundRequest.TenderBid.TotalAmount,
                        ProcessingFee = refundRequest.TenderBid.ProcessingFee,
                        Status = refundRequest.TenderBid.Status,
                        PaymentStatus = refundRequest.TenderBid.PaymentStatus
                    } : null,
                    AvailableTypes = Enum.GetValues<RefundType>().Select(t => new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = t.GetDisplayName()
                    }).ToList(),
                    AvailableReasons = Enum.GetValues<RefundReason>().Select(r => new SelectListItem
                    {
                        Value = ((int)r).ToString(),
                        Text = r.GetDisplayName()
                    }).ToList(),
                    AvailableStatuses = Enum.GetValues<RefundStatus>().Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.GetDisplayName()
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refund request for edit: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the refund request.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Edit(RefundRequestEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableTypes = Enum.GetValues<RefundType>().Select(t => new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = t.GetDisplayName()
                    }).ToList();
                    model.AvailableReasons = Enum.GetValues<RefundReason>().Select(r => new SelectListItem
                    {
                        Value = ((int)r).ToString(),
                        Text = r.GetDisplayName()
                    }).ToList();
                    model.AvailableStatuses = Enum.GetValues<RefundStatus>().Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.GetDisplayName()
                    }).ToList();
                    return View(model);
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var result = await _refundService.UpdateRefundRequestAsync(model, userId);

                TempData["SuccessMessage"] = "Refund request updated successfully.";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund request: {Id}", model.Id);
                TempData["ErrorMessage"] = "An error occurred while updating the refund request.";
                    model.AvailableTypes = Enum.GetValues<RefundType>().Select(t => new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = t.GetDisplayName()
                    }).ToList();
                    model.AvailableReasons = Enum.GetValues<RefundReason>().Select(r => new SelectListItem
                    {
                        Value = ((int)r).ToString(),
                        Text = r.GetDisplayName()
                    }).ToList();
                model.AvailableStatuses = Enum.GetValues<RefundStatus>().Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.GetDisplayName()
                }).ToList();
                return View(model);
            }
        }

        #endregion

        #region Create

        [HttpGet]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Create(int? tenderBidId = null, int? paymentLinkId = null)
        {
            try
            {
                var model = new RefundRequestCreateViewModel
                {
                    AvailableTypes = Enum.GetValues<RefundType>().ToList(),
                    AvailableReasons = Enum.GetValues<RefundReason>().ToList()
                };

                if (tenderBidId.HasValue)
                {
                    model.TenderBidId = tenderBidId.Value;
                    var tenderBid = await _tenderBidRepository.GetBidByIdAsync(tenderBidId.Value);
                    if (tenderBid != null)
                    {
                        model.TenderBid = tenderBid;
                        model.RequestedAmount = tenderBid.TotalAmount; // Default to full amount
                    }
                }

                if (paymentLinkId.HasValue)
                {
                    model.PaymentLinkId = paymentLinkId.Value;
                    var paymentLink = await _paymentLinkRepository.GetByIdAsync(paymentLinkId.Value);
                    if (paymentLink != null)
                    {
                        model.PaymentLink = paymentLink;
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create refund request form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Create(RefundRequestCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableTypes = Enum.GetValues<RefundType>().ToList();
                    model.AvailableReasons = Enum.GetValues<RefundReason>().ToList();
                    return View(model);
                }

                // Input validation
                var amountValidation = _inputValidation.ValidateAmount(model.RequestedAmount);
                if (!amountValidation.IsValid)
                {
                    ModelState.AddModelError("RequestedAmount", "Invalid amount format.");
                    model.AvailableTypes = Enum.GetValues<RefundType>().ToList();
                    model.AvailableReasons = Enum.GetValues<RefundReason>().ToList();
                    return View(model);
                }

                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    ModelState.AddModelError("", "User email not found.");
                    model.AvailableTypes = Enum.GetValues<RefundType>().ToList();
                    model.AvailableReasons = Enum.GetValues<RefundReason>().ToList();
                    return View(model);
                }

                // Set the CreatedBy field to track who created the refund request
                model.CreatedBy = userId;
                var refundRequest = await _refundService.CreateRefundRequestAsync(model, userEmail);

                // Log the action
                await _securityAudit.LogUserActionAsync(new UserAction
                {
                    UserId = userId,
                    Action = "RefundRequest_Created",
                    EntityType = "RefundRequest",
                    EntityId = refundRequest.Id,
                    NewValues = $"Created refund request: {refundRequest.RefundId}",
                    Timestamp = DateTime.UtcNow
                });

                TempData["SuccessMessage"] = $"Refund request {refundRequest.RefundId} has been submitted successfully.";
                return RedirectToAction("Details", new { id = refundRequest.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.AvailableTypes = Enum.GetValues<RefundType>().ToList();
                model.AvailableReasons = Enum.GetValues<RefundReason>().ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund request");
                ModelState.AddModelError("", "An error occurred while creating the refund request.");
                model.AvailableTypes = Enum.GetValues<RefundType>().ToList();
                model.AvailableReasons = Enum.GetValues<RefundReason>().ToList();
                return View(model);
            }
        }

        #endregion


        #region Status Management Actions

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> Approve(int id, decimal approvedAmount, string? remarks = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.ApproveRefundRequestAsync(id, approvedAmount, userId, remarks);

                if (success)
                {
                    // Log the action
                    await _securityAudit.LogUserActionAsync(new UserAction
                    {
                        UserId = userId,
                        Action = "RefundRequest_Approved",
                        EntityType = "RefundRequest",
                        EntityId = id,
                        NewValues = $"Approved refund request ID: {id}, Amount: {approvedAmount:C}",
                        Timestamp = DateTime.UtcNow
                    });

                    TempData["SuccessMessage"] = "Refund request has been approved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve refund request.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving refund request: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while approving the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> Reject(int id, string? remarks = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.RejectRefundRequestAsync(id, userId, remarks);

                if (success)
                {
                    // Log the action
                    await _securityAudit.LogUserActionAsync(new UserAction
                    {
                        UserId = userId,
                        Action = "RefundRequest_Rejected",
                        EntityType = "RefundRequest",
                        EntityId = id,
                        NewValues = $"Rejected refund request ID: {id}",
                        Timestamp = DateTime.UtcNow
                    });

                    TempData["SuccessMessage"] = "Refund request has been rejected.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to reject refund request.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting refund request: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while rejecting the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> Process(int id, string? refundReference = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.ProcessRefundRequestAsync(id, userId, refundReference);

                if (success)
                {
                    // Log the action
                    await _securityAudit.LogUserActionAsync(new UserAction
                    {
                        UserId = userId,
                        Action = "RefundRequest_Processing",
                        EntityType = "RefundRequest",
                        EntityId = id,
                        NewValues = $"Started processing refund request ID: {id}",
                        Timestamp = DateTime.UtcNow
                    });

                    TempData["SuccessMessage"] = "Refund processing has been initiated.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to initiate refund processing.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund request: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while processing the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> Complete(int id, string? remarks = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.CompleteRefundRequestAsync(id, userId, remarks);

                if (success)
                {
                    // Log the action
                    await _securityAudit.LogUserActionAsync(new UserAction
                    {
                        UserId = userId,
                        Action = "RefundRequest_Completed",
                        EntityType = "RefundRequest",
                        EntityId = id,
                        NewValues = $"Completed refund request ID: {id}",
                        Timestamp = DateTime.UtcNow
                    });

                    TempData["SuccessMessage"] = "Refund has been completed successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to complete refund request.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing refund request: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while completing the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> Fail(int id, string? failureReason = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.FailRefundRequestAsync(id, userId, failureReason);

                if (success)
                {
                    // Log the action
                    await _securityAudit.LogUserActionAsync(new UserAction
                    {
                        UserId = userId,
                        Action = "RefundRequest_Failed",
                        EntityType = "RefundRequest",
                        EntityId = id,
                        NewValues = $"Failed refund request ID: {id}",
                        Timestamp = DateTime.UtcNow
                    });

                    TempData["SuccessMessage"] = "Refund request has been marked as failed.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to mark refund request as failed.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error failing refund request: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while marking the refund request as failed.";
                return RedirectToAction("Details", new { id });
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _refundService.DeleteRefundRequestAsync(id);

                if (success)
                {
                    // Log the action
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    await _securityAudit.LogUserActionAsync(new UserAction
                    {
                        UserId = userId,
                        Action = "RefundRequest_Deleted",
                        EntityType = "RefundRequest",
                        EntityId = id,
                        NewValues = $"Deleted refund request ID: {id}",
                        Timestamp = DateTime.UtcNow
                    });

                    TempData["SuccessMessage"] = "Refund request has been deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete refund request.";
                }

                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting refund request: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        #endregion

        #region API Endpoints

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> GetRefundStatistics(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var statistics = await _refundService.GetRefundStatisticsAsync(fromDate, toDate);
                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund statistics");
                return Json(new { error = "Failed to retrieve statistics" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> GetStatusCounts()
        {
            try
            {
                var statusCounts = await _refundService.GetStatusCountsAsync();
                return Json(statusCounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status counts");
                return Json(new { error = "Failed to retrieve status counts" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> ValidateRefundEligibility(int tenderBidId)
        {
            try
            {
                var isEligible = await _refundService.IsRefundRequestEligibleAsync(tenderBidId);
                return Json(new { isEligible });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refund eligibility: {TenderBidId}", tenderBidId);
                return Json(new { error = "Failed to validate eligibility" });
            }
        }

        #endregion

        #region Checker-Maker Workflow Actions

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> SubmitForFirstCheck(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.SubmitForFirstCheckAsync(id, userId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Refund request has been submitted for first check.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to submit refund request for first check.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting refund for first check: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while submitting the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> FirstCheckApprove(int id, decimal approvedAmount, string? remarks = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.FirstCheckApproveAsync(id, userId, approvedAmount, remarks);

                if (success)
                {
                    TempData["SuccessMessage"] = "Refund request has been approved in first check.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve refund request in first check.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving refund in first check: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while approving the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> FirstCheckReject(int id, string? remarks = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.FirstCheckRejectAsync(id, userId, remarks);

                if (success)
                {
                    TempData["SuccessMessage"] = "Refund request has been rejected in first check.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to reject refund request in first check.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting refund in first check: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while rejecting the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> SubmitForSecondCheck(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.SubmitForSecondCheckAsync(id, userId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Refund request has been submitted for second check.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to submit refund request for second check.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting refund for second check: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while submitting the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> SecondCheckApprove(int id, string? remarks = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.SecondCheckApproveAsync(id, userId, remarks);

                if (success)
                {
                    TempData["SuccessMessage"] = "Refund request has been approved in second check.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve refund request in second check.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving refund in second check: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while approving the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> SecondCheckReject(int id, string? remarks = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _refundService.SecondCheckRejectAsync(id, userId, remarks);

                if (success)
                {
                    TempData["SuccessMessage"] = "Refund request has been rejected in second check.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to reject refund request in second check.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting refund in second check: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while rejecting the refund request.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkReadyForProcessing(int id)
        {
            try
            {
                var success = await _refundService.MarkReadyForProcessingAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Refund request has been marked ready for processing.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to mark refund request ready for processing.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking refund ready for processing: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while marking the refund request ready.";
                return RedirectToAction("Details", new { id });
            }
        }

        #endregion

        #region Checker-Maker Views

        [HttpGet]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> PendingFirstCheck()
        {
            try
            {
                var refunds = await _refundService.GetRefundsPendingFirstCheckAsync();
                return View(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refunds pending first check");
                TempData["ErrorMessage"] = "An error occurred while loading refunds pending first check.";
                return View(new List<RefundRequestViewModel>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Checker")]
        public async Task<IActionResult> PendingSecondCheck()
        {
            try
            {
                var refunds = await _refundService.GetRefundsPendingSecondCheckAsync();
                return View(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refunds pending second check");
                TempData["ErrorMessage"] = "An error occurred while loading refunds pending second check.";
                return View(new List<RefundRequestViewModel>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReadyForProcessing()
        {
            try
            {
                var refunds = await _refundService.GetRefundsReadyForProcessingAsync();
                return View(refunds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refunds ready for processing");
                TempData["ErrorMessage"] = "An error occurred while loading refunds ready for processing.";
                return View(new List<RefundRequestViewModel>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckerMakerStatistics()
        {
            try
            {
                var statistics = await _refundService.GetCheckerMakerStatisticsAsync();
                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checker-maker statistics");
                return Json(new { error = "Failed to retrieve statistics" });
            }
        }

        #endregion

        #region Helper Methods

        private bool CanEditRefund(RefundRequestViewModel refundRequest)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            // Admin and Maker can edit any refund request
            if (userRole == "Admin" || userRole == "Maker")
                return true;

            // Users can edit their own pending refund requests
            if (userRole == "Checker" && refundRequest.RequestedBy == userEmail && refundRequest.Status == RefundStatus.Pending)
                return true;

            return false;
        }

        private bool CanApproveRefund(RefundRequestViewModel refundRequest)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return (userRole == "Admin" || userRole == "Checker") && refundRequest.Status == RefundStatus.Pending;
        }

        private bool CanRejectRefund(RefundRequestViewModel refundRequest)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return (userRole == "Admin" || userRole == "Checker") && refundRequest.Status == RefundStatus.Pending;
        }

        private bool CanProcessRefund(RefundRequestViewModel refundRequest)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return (userRole == "Admin" || userRole == "Checker") && refundRequest.Status == RefundStatus.Approved;
        }

        // Checker-Maker Workflow Helper Methods
        private bool CanSubmitForFirstCheck(RefundRequestViewModel refundRequest)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return (userRole == "Admin" || userRole == "Maker") && 
                   refundRequest.WorkflowStatus == RefundWorkflowStatus.Draft;
        }

        private bool CanFirstCheck(RefundRequestViewModel refundRequest)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return (userRole == "Admin" || userRole == "Checker") && 
                   refundRequest.WorkflowStatus == RefundWorkflowStatus.SubmittedForFirstCheck;
        }

        private bool CanSubmitForSecondCheck(RefundRequestViewModel refundRequest)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return (userRole == "Admin" || userRole == "Maker") && 
                   refundRequest.WorkflowStatus == RefundWorkflowStatus.FirstCheckApproved;
        }

        private bool CanSecondCheck(RefundRequestViewModel refundRequest)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return (userRole == "Admin" || userRole == "Checker") && 
                   refundRequest.WorkflowStatus == RefundWorkflowStatus.SubmittedForSecondCheck;
        }

        private bool CanMarkReadyForProcessing(RefundRequestViewModel refundRequest)
        {
            // This is now automatic after second check approval, so always return false
            return false;
        }

        #endregion

    }
}
