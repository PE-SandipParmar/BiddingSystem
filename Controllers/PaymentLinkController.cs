using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using System.Security.Claims;
using static BiddingSystem.Models.PaymentTypeExtensions;

namespace BiddingSystem.Controllers
{
    [Authorize]
    public class PaymentLinkController : Controller
    {
        private readonly IPaymentLinkRepository _paymentLinkRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly ISecurityService _securityService;
        private readonly IEMDSDRepository _emdSdRepository;
        private readonly ILogger<PaymentLinkController> _logger;

        public PaymentLinkController(
            IPaymentLinkRepository paymentLinkRepository,
            ITenderRepository tenderRepository,
            ITenderBidRepository tenderBidRepository,
            ISecurityService securityService,
            IEMDSDRepository emdSdRepository,
            ILogger<PaymentLinkController> logger)
        {
            _paymentLinkRepository = paymentLinkRepository;
            _tenderRepository = tenderRepository;
            _tenderBidRepository = tenderBidRepository;
            _securityService = securityService;
            _emdSdRepository = emdSdRepository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> Index(string searchTerm = "", PaymentLinkStatus? status = null, 
            PaymentType? paymentType = null, DateTime? fromDate = null, DateTime? toDate = null, 
            int page = 1, int pageSize = 25)
        {
            try
            {
                // Input validation
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 25;

                var paymentLinks = await _paymentLinkRepository.SearchAsync(searchTerm, status, paymentType, fromDate, toDate, page, pageSize);
                var totalCount = await _paymentLinkRepository.GetSearchCountAsync(searchTerm, status, paymentType, fromDate, toDate);
                var statusCounts = await _paymentLinkRepository.GetStatusCountsAsync();

                var viewModel = new PaymentLinkListViewModel
                {
                    PaymentLinks = paymentLinks.Select(MapToViewModel).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    Status = status,
                    PaymentType = paymentType,
                    FromDate = fromDate,
                    ToDate = toDate,
                    StatusOptions = GetStatusOptions(),
                    PaymentTypeOptions = GetPaymentTypeOptions(),
                    TenderOptions = await GetTenderOptionsAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment links index");
                TempData["ErrorMessage"] = "An error occurred while loading payment links.";
                return View(new PaymentLinkListViewModel());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new PaymentLinkViewModel
                {
                    ExpiryDate = DateTime.UtcNow.AddDays(30)
                };

                ViewBag.TenderOptions = await GetTenderOptionsAsync();
                ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create payment link page");
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Create(PaymentLinkViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.TenderOptions = await GetTenderOptionsAsync();
                    ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                    return View(model);
                }

                // Validate tender exists and is accessible
                var tender = await _tenderRepository.GetByIdAsync(model.TenderId);
                if (tender == null)
                {
                    ModelState.AddModelError("TenderId", "Selected tender not found.");
                    ViewBag.TenderOptions = await GetTenderOptionsAsync();
                    ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                    return View(model);
                }

                // Validate expiry date
                if (!_securityService.IsValidExpiryDate(model.ExpiryDate))
                {
                    ModelState.AddModelError("ExpiryDate", "Expiry date must be in the future and within 1 year.");
                    ViewBag.TenderOptions = await GetTenderOptionsAsync();
                    ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                    return View(model);
                }

                // Validate amount
                if (!_securityService.IsValidAmount(model.Amount))
                {
                    ModelState.AddModelError("Amount", "Amount must be positive and within valid limits.");
                    ViewBag.TenderOptions = await GetTenderOptionsAsync();
                    ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                    return View(model);
                }

                // Generate unique link ID and security token
                var linkId = await _paymentLinkRepository.GenerateUniqueLinkIdAsync();
                var securityToken = PaymentLink.GenerateSecureToken();
                var paymentUrl = GeneratePaymentUrl(linkId, securityToken);

                var paymentLink = new PaymentLink
                {
                    LinkId = linkId,
                    TenderId = model.TenderId,
                    TenderBidId = model.TenderBidId,
                    Amount = model.Amount,
                    PaymentType = model.PaymentType,
                    PaymentUrl = paymentUrl,
                    SecurityToken = securityToken,
                    Status = PaymentLinkStatus.Active,
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = model.ExpiryDate,
                    CreatedBy = GetCurrentUserId(),
                    IsActive = true,
                    Notes = model.Notes
                };

                await _paymentLinkRepository.CreateAsync(paymentLink);

                TempData["SuccessMessage"] = "Payment link generated successfully.";
                return RedirectToAction("Details", new { id = paymentLink.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment link");
                ModelState.AddModelError("", "An error occurred while creating the payment link. Please try again.");
                ViewBag.TenderOptions = await GetTenderOptionsAsync();
                ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var paymentLink = await _paymentLinkRepository.GetByIdAsync(id);
                if (paymentLink == null)
                {
                    TempData["ErrorMessage"] = "Payment link not found.";
                    return RedirectToAction("Index");
                }

                var viewModel = MapToDetailsViewModel(paymentLink);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment link details: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading payment link details.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var paymentLink = await _paymentLinkRepository.GetByIdAsync(id);
                if (paymentLink == null)
                {
                    TempData["ErrorMessage"] = "Payment link not found.";
                    return RedirectToAction("Index");
                }

                // Only allow editing of active, non-expired links
                if (paymentLink.Status != PaymentLinkStatus.Active || paymentLink.IsExpired)
                {
                    TempData["ErrorMessage"] = "This payment link cannot be edited.";
                    return RedirectToAction("Details", new { id });
                }

                var viewModel = new PaymentLinkViewModel
                {
                    Id = paymentLink.Id,
                    TenderId = paymentLink.TenderId,
                    TenderBidId = paymentLink.TenderBidId,
                    Amount = paymentLink.Amount,
                    PaymentType = paymentLink.PaymentType,
                    ExpiryDate = paymentLink.ExpiryDate,
                    Notes = paymentLink.Notes,
                    TenderTitle = paymentLink.Tender.TenderTitle,
                    TenderIdString = paymentLink.Tender.TenderId,
                    BidderName = paymentLink.TenderBid?.BidderName,
                    CompanyName = paymentLink.TenderBid?.CompanyName,
                    LinkId = paymentLink.LinkId,
                    Status = paymentLink.Status
                };

                ViewBag.TenderOptions = await GetTenderOptionsAsync();
                ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                ViewBag.TenderBidOptions = await GetTenderBidOptionsAsync(paymentLink.TenderId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment link for edit: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the payment link.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Edit(PaymentLinkViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.TenderOptions = await GetTenderOptionsAsync();
                    ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                    return View(model);
                }

                var existingPaymentLink = await _paymentLinkRepository.GetByIdAsync(model.Id);
                if (existingPaymentLink == null)
                {
                    TempData["ErrorMessage"] = "Payment link not found.";
                    return RedirectToAction("Index");
                }

                // Only allow editing of active, non-expired links
                if (existingPaymentLink.Status != PaymentLinkStatus.Active || existingPaymentLink.IsExpired)
                {
                    TempData["ErrorMessage"] = "This payment link cannot be edited.";
                    return RedirectToAction("Details", new { id = model.Id });
                }

                // Validate expiry date
                if (model.ExpiryDate <= DateTime.UtcNow)
                {
                    ModelState.AddModelError("ExpiryDate", "Expiry date must be in the future.");
                    ViewBag.TenderOptions = await GetTenderOptionsAsync();
                    ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                    return View(model);
                }

                // Update payment link
                existingPaymentLink.TenderBidId = model.TenderBidId;
                existingPaymentLink.Amount = model.Amount;
                existingPaymentLink.PaymentType = model.PaymentType;
                existingPaymentLink.ExpiryDate = model.ExpiryDate;
                existingPaymentLink.Notes = model.Notes;

                await _paymentLinkRepository.UpdateAsync(existingPaymentLink);

                TempData["SuccessMessage"] = "Payment link updated successfully.";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment link: {Id}", model.Id);
                ModelState.AddModelError("", "An error occurred while updating the payment link. Please try again.");
                ViewBag.TenderOptions = await GetTenderOptionsAsync();
                ViewBag.PaymentTypeOptions = GetPaymentTypeOptions();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    TempData["ErrorMessage"] = "Cancellation reason is required.";
                    return RedirectToAction("Details", new { id });
                }

                var success = await _paymentLinkRepository.CancelAsync(id, reason);
                if (success)
                {
                    TempData["SuccessMessage"] = "Payment link cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel payment link.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment link: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while cancelling the payment link.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _paymentLinkRepository.DeleteAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Payment link deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete payment link.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment link: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the payment link.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Pay(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return View("PaymentError", "Invalid payment link.");
                }

                var paymentLink = await _paymentLinkRepository.GetBySecurityTokenAsync(token);
                if (paymentLink == null || !paymentLink.IsUsable)
                {
                    return View("PaymentError", "Payment link is invalid or has expired.");
                }

                var viewModel = new PaymentLinkDetailsViewModel
                {
                    Id = paymentLink.Id,
                    LinkId = paymentLink.LinkId,
                    TenderTitle = paymentLink.Tender.TenderTitle,
                    TenderId = paymentLink.Tender.Id.ToString(),
                    TenderIdString = paymentLink.Tender.TenderId,
                    Amount = paymentLink.Amount,
                    PaymentType = paymentLink.PaymentType,
                    PaymentUrl = paymentLink.PaymentUrl,
                    Status = paymentLink.Status,
                    CreatedDate = paymentLink.CreatedDate,
                    ExpiryDate = paymentLink.ExpiryDate,
                    StatusDisplayName = paymentLink.StatusDisplayName,
                    StatusBadgeClass = paymentLink.StatusBadgeClass,
                    AmountDisplay = paymentLink.AmountDisplay,
                    IsExpired = paymentLink.IsExpired,
                    IsUsable = paymentLink.IsUsable
                };

                return View("Payment", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment page for token");
                return View("PaymentError", "An error occurred while loading the payment page.");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ProcessPayment(int id, string transactionId)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    return Json(new { success = false, message = "Transaction ID is required." });
                }

                // Sanitize and validate transaction ID
                transactionId = _securityService.SanitizeInput(transactionId);
                if (!_securityService.IsValidTransactionId(transactionId))
                {
                    return Json(new { success = false, message = "Invalid transaction ID format." });
                }

                // Verify payment link exists and is usable
                var paymentLink = await _paymentLinkRepository.GetByIdAsync(id);
                if (paymentLink == null || !paymentLink.IsUsable)
                {
                    return Json(new { success = false, message = "Payment link is invalid or has expired." });
                }

                // Check for duplicate transaction ID
                if (await _paymentLinkRepository.TransactionIdExistsAsync(transactionId))
                {
                    return Json(new { success = false, message = "Transaction ID has already been used." });
                }

                var success = await _paymentLinkRepository.MarkAsUsedAsync(id, null, transactionId); // null for anonymous user
                if (success)
                {
                    // Create EMD/SD deposit record
                    try
                    {
                        var depositId = await _emdSdRepository.GenerateUniqueDepositIdAsync();
                        
                        // Get bidder information if linked to a specific bid
                        string bidderName = "Anonymous User";
                        string companyName = "Payment Link User";
                        
                        if (paymentLink.TenderBidId.HasValue)
                        {
                            var tenderBid = await _tenderBidRepository.GetBidByIdAsync(paymentLink.TenderBidId.Value);
                            if (tenderBid != null)
                            {
                                bidderName = tenderBid.BidderName;
                                companyName = tenderBid.CompanyName;
                                
                                // Update the tender bid payment status
                                tenderBid.PaymentStatus = "Paid";
                                tenderBid.PaymentReference = transactionId;
                                tenderBid.PaymentDate = DateTime.UtcNow;
                                await _tenderBidRepository.UpdateBidAsync(tenderBid);
                            }
                        }
                        
                        var emdSdDeposit = new EMDSDDeposit
                        {
                            DepositId = depositId,
                            TenderId = paymentLink.TenderId,
                            Amount = paymentLink.Amount,
                            BidderName = bidderName,
                            CompanyName = companyName,
                            TransactionDate = DateTime.UtcNow,
                            BankName = "Online Payment Gateway",
                            FSSAIBranchName = "FSSAI Main Branch",
                            TransactionId = transactionId,
                            Status = "Paid",
                            Type = paymentLink.PaymentType == PaymentType.EMD ? "EMD" : "SD",
                            CreatedAt = DateTime.UtcNow,
                            Remarks = $"Payment processed via Payment Link: {paymentLink.LinkId}" + 
                                     (paymentLink.TenderBidId.HasValue ? $" (Linked to Bid ID: {paymentLink.TenderBidId})" : "")
                        };

                        await _emdSdRepository.CreateDepositAsync(emdSdDeposit);
                        _logger.LogInformation("EMD/SD deposit record created for payment link {LinkId} with deposit ID {DepositId}", 
                            paymentLink.LinkId, depositId);
                    }
                    catch (Exception emdEx)
                    {
                        _logger.LogError(emdEx, "Error creating EMD/SD deposit record for payment link {LinkId}", paymentLink.LinkId);
                        // Don't fail the payment processing if EMD/SD record creation fails
                    }

                    _logger.LogInformation("Payment processed successfully for link {LinkId} with transaction {TransactionId}", 
                        paymentLink.LinkId, transactionId);
                    return Json(new { success = true, message = "Payment processed successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to process payment." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for link: {Id}", id);
                return Json(new { success = false, message = "An error occurred while processing the payment." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> GetTenderBids(int tenderId)
        {
            try
            {
                var bids = await _tenderBidRepository.GetBidsByTenderIdAsync(tenderId);
                var result = bids.Select(bid => new
                {
                    id = bid.Id,
                    bidderName = bid.BidderName,
                    companyName = bid.CompanyName,
                    bidAmount = bid.BidAmount,
                    paymentStatus = bid.PaymentStatus
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender bids for tender: {TenderId}", tenderId);
                return Json(new List<object>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> CopyLink(int id)
        {
            try
            {
                var paymentLink = await _paymentLinkRepository.GetByIdAsync(id);
                if (paymentLink == null)
                {
                    return Json(new { success = false, message = "Payment link not found." });
                }

                return Json(new { success = true, url = paymentLink.PaymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment link URL: {Id}", id);
                return Json(new { success = false, message = "An error occurred while getting the payment link." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateExpiredStatus()
        {
            try
            {
                var success = await _paymentLinkRepository.UpdateExpiredStatusAsync();
                if (success)
                {
                    TempData["SuccessMessage"] = "Expired payment links updated successfully.";
                }
                else
                {
                    TempData["InfoMessage"] = "No expired payment links found to update.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expired payment link statuses");
                TempData["ErrorMessage"] = "An error occurred while updating expired payment links.";
                return RedirectToAction("Index");
            }
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private string GeneratePaymentUrl(string linkId, string securityToken)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return $"{baseUrl}/PaymentLink/Pay?token={securityToken}";
        }

        private async Task<List<SelectListItem>> GetTenderOptionsAsync()
        {
            try
            {
                var tenders = await _tenderRepository.GetAllAsync();
                return tenders
                    .Where(t => t.IsActive && t.Status == TenderStatus.Published)
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = $"{t.TenderId} - {t.TenderTitle}"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender options");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetStatusOptions()
        {
            return Enum.GetValues<PaymentLinkStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.GetDisplayName()
                })
                .ToList();
        }

        private List<SelectListItem> GetPaymentTypeOptions()
        {
            return Enum.GetValues<PaymentType>()
                .Select(p => new SelectListItem
                {
                    Value = ((int)p).ToString(),
                    Text = p.GetDisplayName()
                })
                .ToList();
        }

        private async Task<List<SelectListItem>> GetTenderBidOptionsAsync(int tenderId)
        {
            try
            {
                var bids = await _tenderBidRepository.GetBidsByTenderIdAsync(tenderId);
                return bids.Select(bid => new SelectListItem
                {
                    Value = bid.Id.ToString(),
                    Text = $"{bid.BidderName} - {bid.CompanyName} (₹{bid.BidAmount})"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender bid options for tender: {TenderId}", tenderId);
                return new List<SelectListItem>();
            }
        }

        private PaymentLinkViewModel MapToViewModel(PaymentLink paymentLink)
        {
            return new PaymentLinkViewModel
            {
                Id = paymentLink.Id,
                TenderId = paymentLink.TenderId,
                TenderBidId = paymentLink.TenderBidId,
                Amount = paymentLink.Amount,
                PaymentType = paymentLink.PaymentType,
                ExpiryDate = paymentLink.ExpiryDate,
                Notes = paymentLink.Notes,
                TenderTitle = paymentLink.Tender.TenderTitle,
                TenderIdString = paymentLink.Tender.TenderId,
                BidderName = paymentLink.TenderBid?.BidderName,
                CompanyName = paymentLink.TenderBid?.CompanyName,
                LinkId = paymentLink.LinkId,
                PaymentUrl = paymentLink.PaymentUrl,
                Status = paymentLink.Status,
                CreatedDate = paymentLink.CreatedDate,
                UsedDate = paymentLink.UsedDate,
                CreatedByUserName = paymentLink.CreatedByUser.Username,
                UsedByUserName = paymentLink.UsedByUser?.Username,
                TransactionId = paymentLink.TransactionId
            };
        }

        private PaymentLinkDetailsViewModel MapToDetailsViewModel(PaymentLink paymentLink)
        {
            return new PaymentLinkDetailsViewModel
            {
                Id = paymentLink.Id,
                LinkId = paymentLink.LinkId,
                TenderTitle = paymentLink.Tender.TenderTitle,
                TenderId = paymentLink.Tender.TenderId,
                BidderName = paymentLink.TenderBid?.BidderName,
                CompanyName = paymentLink.TenderBid?.CompanyName,
                Amount = paymentLink.Amount,
                PaymentType = paymentLink.PaymentType,
                PaymentUrl = paymentLink.PaymentUrl,
                Status = paymentLink.Status,
                CreatedDate = paymentLink.CreatedDate,
                ExpiryDate = paymentLink.ExpiryDate,
                UsedDate = paymentLink.UsedDate,
                CreatedByUserName = paymentLink.CreatedByUser.Username,
                UsedByUserName = paymentLink.UsedByUser?.Username,
                TransactionId = paymentLink.TransactionId,
                Notes = paymentLink.Notes,
                IsExpired = paymentLink.IsExpired,
                IsUsable = paymentLink.IsUsable,
                StatusDisplayName = paymentLink.StatusDisplayName,
                StatusBadgeClass = paymentLink.StatusBadgeClass,
                AmountDisplay = paymentLink.AmountDisplay
            };
        }
    }
}
