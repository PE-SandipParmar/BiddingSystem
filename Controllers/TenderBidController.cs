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
    public class TenderBidController : Controller
    {
        private readonly ITenderBidService _tenderBidService;
        private readonly ITenderRepository _tenderRepository;
        private readonly ILogger<TenderBidController> _logger;
        private readonly IInputValidationService _inputValidation;
        private readonly ISecurityAuditService _securityAudit;

        public TenderBidController(
            ITenderBidService tenderBidService,
            ITenderRepository tenderRepository,
            ILogger<TenderBidController> logger,
            IInputValidationService inputValidation,
            ISecurityAuditService securityAudit)
        {
            _tenderBidService = tenderBidService;
            _tenderRepository = tenderRepository;
            _logger = logger;
            _inputValidation = inputValidation;
            _securityAudit = securityAudit;
        }

        // GET: TenderBid

        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> Index(int? tenderId = null, string searchTerm = "", string status = "", string paymentStatus = "", int page = 1, int pageSize = 25)
        {
            try
            {
                // Input validation
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 25;

                // Get all tenders for dropdown
                var allTenders = await _tenderRepository.GetAllAsync();

                // If tenderId is provided, validate it exists
                Tender? selectedTender = null;
                if (tenderId.HasValue)
                {
                    selectedTender = await _tenderRepository.GetByIdAsync(tenderId.Value);
                    if (selectedTender == null)
                    {
                        TempData["ErrorMessage"] = "Selected tender not found.";
                        tenderId = null;
                    }
                }

                // Get bids - if tenderId is provided, filter by it
                IEnumerable<TenderBid> bids;
                int totalCount;

                if (tenderId.HasValue)
                {
                    // Get bids for specific tender
                    var allBidsForTender = await _tenderBidService.GetBidsByTenderIdAsync(tenderId.Value);

                    // Apply additional filters
                    var filteredBids = allBidsForTender.AsQueryable();

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        filteredBids = filteredBids.Where(b =>
                            b.BidderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            b.CompanyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            b.BidderEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        filteredBids = filteredBids.Where(b => b.Status == status);
                    }

                    if (!string.IsNullOrEmpty(paymentStatus))
                    {
                        filteredBids = filteredBids.Where(b => b.PaymentStatus == paymentStatus);
                    }

                    totalCount = filteredBids.Count();
                    bids = filteredBids
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {
                    // Get all bids with filters
                    var result = await _tenderBidService.GetBidsPagedAsync(page, pageSize, searchTerm, status, paymentStatus);
                    bids = result.bids;
                    totalCount = result.totalCount;
                }

                var viewModel = new TenderBidListViewModel
                {
                    Bids = bids.Select(MapToViewModel).ToList(),
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    Status = status,
                    PaymentStatus = paymentStatus,
                    SelectedTenderId = tenderId,
                    SelectedTenderName = selectedTender?.TenderTitle,
                    AvailableTenders = allTenders.Where(t => t.IsActive).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender bids index");
                TempData["ErrorMessage"] = "An error occurred while loading tender bids.";
                return View(new TenderBidListViewModel());
            }
        }

        // GET: TenderBid/Details/5
        [Authorize(Roles = "Admin,Maker,Checker")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var bid = await _tenderBidService.GetBidByIdAsync(id);
                if (bid == null)
                {
                    TempData["ErrorMessage"] = "Tender bid not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToViewModel(bid);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender bid details: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading tender bid details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: TenderBid/Create
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var tenders = await _tenderRepository.GetAllAsync();
                var viewModel = new TenderBidCreateViewModel
                {
                    AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create tender bid page");
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
                return RedirectToAction("Index");
            }
        }

        // POST: TenderBid/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Create(TenderBidCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var tenders = await _tenderRepository.GetAllAsync();
                    model.AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList();
                    return View(model);
                }

                // Validate tender exists and is accessible
                var tender = await _tenderRepository.GetByIdAsync(model.TenderId);
                if (tender == null)
                {
                    ModelState.AddModelError("TenderId", "Selected tender not found.");
                    var tenders = await _tenderRepository.GetAllAsync();
                    model.AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList();
                    return View(model);
                }

                // Enhanced input validation
                var emailValidation = _inputValidation.ValidateEmail(model.BidderEmail);
                if (!emailValidation.IsValid)
                {
                    ModelState.AddModelError("BidderEmail", string.Join(", ", emailValidation.Errors));
                }

                var phoneValidation = _inputValidation.ValidatePhoneNumber(model.BidderPhone);
                if (!phoneValidation.IsValid)
                {
                    ModelState.AddModelError("BidderPhone", string.Join(", ", phoneValidation.Errors));
                }

                var companyValidation = _inputValidation.ValidateCompanyName(model.CompanyName);
                if (!companyValidation.IsValid)
                {
                    ModelState.AddModelError("CompanyName", string.Join(", ", companyValidation.Errors));
                }

                var bidderValidation = _inputValidation.ValidateBidderName(model.BidderName);
                if (!bidderValidation.IsValid)
                {
                    ModelState.AddModelError("BidderName", string.Join(", ", bidderValidation.Errors));
                }

                if (!ModelState.IsValid)
                {
                    var tenders = await _tenderRepository.GetAllAsync();
                    model.AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList();
                    return View(model);
                }

                // Check for duplicate bid
                var isDuplicate = await _tenderBidService.CheckDuplicateBidAsync(model.TenderId, model.BidderEmail);
                if (isDuplicate)
                {
                    ModelState.AddModelError("BidderEmail", "A bid already exists for this tender with this email address.");
                    var tenders = await _tenderRepository.GetAllAsync();
                    model.AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList();
                    return View(model);
                }

                var bid = new TenderBid
                {
                    TenderId = model.TenderId,
                    BidderName = bidderValidation.SanitizedValue ?? model.BidderName,
                    BidderEmail = emailValidation.SanitizedValue ?? model.BidderEmail,
                    BidderPhone = phoneValidation.SanitizedValue ?? model.BidderPhone,
                    CompanyName = companyValidation.SanitizedValue ?? model.CompanyName,
                    CompanyAddress = model.CompanyAddress,
                    BidAmount = model.BidAmount,
                    EmdAmount = model.EmdAmount,
                    ProcessingFee = model.ProcessingFee,
                    TotalAmount = model.BidAmount + model.EmdAmount + model.ProcessingFee,
                    Status = "Submitted",
                    PaymentStatus = "Pending",
                    Remarks = model.Remarks
                };

                await _tenderBidService.CreateBidAsync(bid);

                // Log successful bid creation
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _securityAudit.LogUserActionAsync(new UserAction
                {
                    UserId = userId,
                    Action = "CreateBid",
                    EntityType = "TenderBid",
                    EntityId = bid.Id,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
                });

                TempData["SuccessMessage"] = "Tender bid created successfully.";
                return RedirectToAction(nameof(Details), new { id = bid.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tender bid");
                ModelState.AddModelError("", "An error occurred while creating the tender bid. Please try again.");
                var tenders = await _tenderRepository.GetAllAsync();
                model.AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList();
                return View(model);
            }
        }

        // GET: TenderBid/Edit/5
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var bid = await _tenderBidService.GetBidByIdAsync(id);
                if (bid == null)
                {
                    TempData["ErrorMessage"] = "Tender bid not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if tender is published - bids can only be edited when tender status is Published
                if (bid.Tender?.Status != TenderStatus.Published)
                {
                    TempData["ErrorMessage"] = "Tender bids can only be edited when the tender status is Published.";
                    return RedirectToAction(nameof(Index));
                }

                var tenders = await _tenderRepository.GetAllAsync();
                var viewModel = new TenderBidEditViewModel
                {
                    Id = bid.Id,
                    TenderId = bid.TenderId,
                    BidderName = bid.BidderName,
                    BidderEmail = bid.BidderEmail,
                    BidderPhone = bid.BidderPhone,
                    CompanyName = bid.CompanyName,
                    CompanyAddress = bid.CompanyAddress,
                    BidAmount = Convert.ToDecimal(bid.BidAmount),
                    EmdAmount = Convert.ToDecimal(bid.EmdAmount),
                    ProcessingFee = Convert.ToDecimal(bid.ProcessingFee),
                    Status = bid.Status,
                    PaymentStatus = bid.PaymentStatus,
                    PaymentReference = bid.PaymentReference,
                    PaymentDate = bid.PaymentDate,
                    Remarks = bid.Remarks,
                    AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender bid for edit: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the tender bid.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: TenderBid/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Edit(TenderBidEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var tenders = await _tenderRepository.GetAllAsync();
                    model.AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList();
                    return View(model);
                }

                var existingBid = await _tenderBidService.GetBidByIdAsync(model.Id);
                if (existingBid == null)
                {
                    TempData["ErrorMessage"] = "Tender bid not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if tender is published - bids can only be edited when tender status is Published
                if (existingBid.Tender?.Status != TenderStatus.Published)
                {
                    TempData["ErrorMessage"] = "Tender bids can only be edited when the tender status is Published.";
                    return RedirectToAction(nameof(Index));
                }

                // Update bid properties
                existingBid.TenderId = model.TenderId;
                existingBid.BidderName = model.BidderName;
                existingBid.BidderEmail = model.BidderEmail;
                existingBid.BidderPhone = model.BidderPhone;
                existingBid.CompanyName = model.CompanyName;
                existingBid.CompanyAddress = model.CompanyAddress;
                existingBid.BidAmount = model.BidAmount;
                existingBid.EmdAmount = model.EmdAmount;
                existingBid.ProcessingFee = model.ProcessingFee;
                existingBid.TotalAmount = model.BidAmount + model.EmdAmount + model.ProcessingFee;
                existingBid.Status = model.Status;
                existingBid.PaymentStatus = model.PaymentStatus;
                existingBid.PaymentReference = model.PaymentReference;
                existingBid.PaymentDate = model.PaymentDate;
                existingBid.Remarks = model.Remarks;

                await _tenderBidService.UpdateBidAsync(existingBid);

                TempData["SuccessMessage"] = "Tender bid updated successfully.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tender bid: {Id}", model.Id);
                ModelState.AddModelError("", "An error occurred while updating the tender bid. Please try again.");
                var tenders = await _tenderRepository.GetAllAsync();
                model.AvailableTenders = tenders.Where(t => t.IsActive && t.Status == TenderStatus.Published).ToList();
                return View(model);
            }
        }

        // GET: TenderBid/Delete/5
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var bid = await _tenderBidService.GetBidByIdAsync(id);
                if (bid == null)
                {
                    TempData["ErrorMessage"] = "Tender bid not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToViewModel(bid);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender bid for delete: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the tender bid.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: TenderBid/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _tenderBidService.DeleteBidAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Tender bid deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete tender bid.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tender bid: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the tender bid.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: TenderBid/UpdatePaymentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, string paymentStatus, string? paymentReference = null, DateTime? paymentDate = null)
        {
            try
            {
                var success = await _tenderBidService.UpdatePaymentStatusAsync(id, paymentStatus, paymentReference, paymentDate);
                if (success)
                {
                    TempData["SuccessMessage"] = "Payment status updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update payment status.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for tender bid: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while updating payment status.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: TenderBid/UploadDocument
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> UploadDocument(int bidId, IFormFile file, BidDocumentType documentType)
        {
            try
            {
                _logger.LogInformation("UploadDocument called with bidId: {BidId}, file: {FileName}, documentType: {DocumentType}", 
                    bidId, file?.FileName, documentType);

                if (file == null || file.Length == 0)
                {
                    TempData["ErrorMessage"] = "Please select a file to upload.";
                    return RedirectToAction(nameof(Details), new { id = bidId });
                }

                // Enhanced file validation
                var fileValidation = _inputValidation.ValidateFileUpload(file, FileUploadType.Document);
                if (!fileValidation.IsValid)
                {
                    TempData["ErrorMessage"] = string.Join(", ", fileValidation.Errors);
                    return RedirectToAction(nameof(Details), new { id = bidId });
                }

                var document = await _tenderBidService.AddBidDocumentAsync(bidId, file, documentType);
                
                _logger.LogInformation("Document uploaded successfully. Document ID: {DocumentId}, FilePath: {FilePath}", 
                    document.Id, document.FilePath);
                
                // Log file upload
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _securityAudit.LogFileUploadAsync(userId, file.FileName, document.FilePath, true);
                
                TempData["SuccessMessage"] = "Document uploaded successfully.";
                return RedirectToAction(nameof(Details), new { id = bidId });
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                
                // Log failed file upload
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _securityAudit.LogFileUploadAsync(userId, file?.FileName ?? "unknown", "", false);
                
                return RedirectToAction(nameof(Details), new { id = bidId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for bid: {BidId}", bidId);
                TempData["ErrorMessage"] = "An error occurred while uploading the document.";
                
                // Log failed file upload
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _securityAudit.LogFileUploadAsync(userId, file?.FileName ?? "unknown", "", false);
                
                return RedirectToAction(nameof(Details), new { id = bidId });
            }
        }

        // POST: TenderBid/DeleteDocument
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Maker")]
        public async Task<IActionResult> DeleteDocument(int documentId, int bidId)
        {
            try
            {
                var success = await _tenderBidService.DeleteBidDocumentAsync(documentId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Document deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete document.";
                }

                return RedirectToAction(nameof(Details), new { id = bidId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document: {DocumentId}", documentId);
                TempData["ErrorMessage"] = "An error occurred while deleting the document.";
                return RedirectToAction(nameof(Details), new { id = bidId });
            }
        }

        // Helper method to map TenderBid to TenderBidViewModel
        private TenderBidViewModel MapToViewModel(TenderBid bid)
        {
            return new TenderBidViewModel
            {
                Id = bid.Id,
                TenderId = bid.TenderId,
                TenderName = bid.Tender?.TenderTitle ?? "N/A",
                TenderStatus = bid.Tender?.Status.ToString() ?? "N/A",
                BidderName = bid.BidderName,
                BidderEmail = bid.BidderEmail,
                BidderPhone = bid.BidderPhone,
                CompanyName = bid.CompanyName,
                CompanyAddress = bid.CompanyAddress,
                BidAmount = Convert.ToDecimal(bid.BidAmount),
                EmdAmount = Convert.ToDecimal(bid.EmdAmount),
                ProcessingFee = Convert.ToDecimal(bid.ProcessingFee),
                TotalAmount = bid.TotalAmount,
                Status = bid.Status,
                PaymentStatus = bid.PaymentStatus,
                PaymentReference = bid.PaymentReference,
                PaymentDate = bid.PaymentDate,
                SubmittedAt = bid.SubmittedAt,
                IsActive = bid.IsActive,
                CreatedAt = bid.CreatedAt,
                UpdatedAt = bid.UpdatedAt,
                Remarks = bid.Remarks,
                Tender = bid.Tender,
                Documents = bid.Documents.ToList(),

                // Add refund information
                HasRefund = bid.RefundInfo != null,
                RefundInfo = bid.RefundInfo
            };
        }
    }
}
