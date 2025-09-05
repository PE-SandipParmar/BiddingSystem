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
    public class TenderController : Controller
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TenderController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TenderController(
            ITenderRepository tenderRepository,
            IUserRepository userRepository,
            ILogger<TenderController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _tenderRepository = tenderRepository;
            _userRepository = userRepository;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Dashboard

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var statistics = await _tenderRepository.GetDashboardStatisticsAsync();
                var recentTenders = await _tenderRepository.GetRecentTendersAsync();
                var expiringTenders = await _tenderRepository.GetExpiringTendersAsync();
                var upcomingTenders = await _tenderRepository.GetUpcomingTendersAsync();
                var statusCounts = await _tenderRepository.GetTenderCountByStatusAsync();

                var viewModel = new TenderDashboardViewModel
                {
                    TotalTenders = Convert.ToInt32(statistics.GetValueOrDefault("TotalTenders", 0)),
                    PublishedTenders = Convert.ToInt32(statistics.GetValueOrDefault("PublishedTenders", 0)),
                    TotalBids = Convert.ToInt32(statistics.GetValueOrDefault("TotalBids", 0)),
                    PaidBids = Convert.ToInt32(statistics.GetValueOrDefault("PaidBids", 0)),
                    TotalEmdCollected = Convert.ToDecimal(statistics.GetValueOrDefault("TotalEmdCollected", 0)),
                    TotalProcessingFeesCollected = Convert.ToDecimal(statistics.GetValueOrDefault("TotalProcessingFeesCollected", 0)),
                    RecentTenders = recentTenders.Select(MapToViewModel).ToList(),
                    ExpiringTenders = expiringTenders.Select(MapToViewModel).ToList(),
                    UpcomingTenders = upcomingTenders.Select(MapToViewModel).ToList(),
                    TenderCountByStatus = statusCounts
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion

        #region Tender Management

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? searchTerm = null, TenderStatus? status = null, string? department = null)
        {
            try
            {
                var (tenders, totalCount) = await _tenderRepository.GetPagedTendersAsync(page, pageSize, searchTerm, status, department);
                var departments = await GetDepartmentsAsync();
                var statusCounts = await _tenderRepository.GetTenderCountByStatusAsync();

                var viewModel = new TenderListViewModel
                {
                    Tenders = tenders.Select(MapToViewModel).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    Status = status,
                    Department = department,
                    Departments = departments,
                    StatusCounts = statusCounts
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tenders list");
                TempData["ErrorMessage"] = "An error occurred while loading tenders.";
                return View(new TenderListViewModel());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new TenderViewModel
            {
                PublishDate = DateTime.Today,
                LastDateEmd = DateTime.Today.AddDays(30)
            };

            ViewBag.Departments = GetDepartmentOptions();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TenderViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Departments = GetDepartmentOptions();
                    return View(model);
                }

                // Check if tender ID already exists
                if (await _tenderRepository.ExistsAsync(model.TenderId))
                {
                    ModelState.AddModelError("TenderId", "A tender with this ID already exists.");
                    ViewBag.Departments = GetDepartmentOptions();
                    return View(model);
                }

                // Validate date ranges
                if (model.LastDateEmd <= model.PublishDate)
                {
                    ModelState.AddModelError("LastDateEmd", "Last date to pay EMD must be after publish date.");
                    ViewBag.Departments = GetDepartmentOptions();
                    return View(model);
                }

                var tender = new Tender
                {
                    TenderId = model.TenderId,
                    TenderTitle = model.TenderTitle,
                    Description = model.Description,
                    Department = model.Department,
                    PublishDate = model.PublishDate,
                    EmdAmount = model.EmdAmount,
                    SdAmount = model.SdAmount,
                    ProcessingFee = model.ProcessingFee,
                    EstimatedValue = model.EstimatedValue,
                    LastDateEmd = model.LastDateEmd,
                    TenderClosingDate = model.TenderClosingDate,
                    TenderOpeningDate = model.TenderOpeningDate,
                    Status = TenderStatus.Draft,
                    CreatedBy = GetCurrentUserId(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var tenderId = await _tenderRepository.CreateAsync(tender);
                _logger.LogInformation($"Tender {model.TenderId} created successfully by user {GetCurrentUserId()}");

                TempData["SuccessMessage"] = $"Tender {model.TenderId} has been created successfully.";
                return RedirectToAction("Details", new { id = tenderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tender");
                ModelState.AddModelError("", "An error occurred while creating the tender. Please try again.");
                ViewBag.Departments = GetDepartmentOptions();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(id);
                if (tender == null)
                {
                    TempData["ErrorMessage"] = "Tender not found.";
                    return RedirectToAction("Index");
                }

                var documents = await _tenderRepository.GetTenderDocumentsAsync(id);
                var bids = await _tenderRepository.GetTenderBidsAsync(id);

                var viewModel = MapToViewModel(tender);
                viewModel.Documents = documents;
                viewModel.Bids = bids;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender details");
                TempData["ErrorMessage"] = "An error occurred while loading tender details.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(id);
                if (tender == null)
                {
                    TempData["ErrorMessage"] = "Tender not found.";
                    return RedirectToAction("Index");
                }

                if (!tender.IsEditable)
                {
                    TempData["ErrorMessage"] = "This tender cannot be edited as it has been published.";
                    return RedirectToAction("Details", new { id });
                }

                var viewModel = MapToViewModel(tender);
                ViewBag.Departments = GetDepartmentOptions();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender for edit");
                TempData["ErrorMessage"] = "An error occurred while loading the tender.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TenderViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Departments = GetDepartmentOptions();
                    return View(model);
                }

                var existingTender = await _tenderRepository.GetByIdAsync(model.Id);
                if (existingTender == null)
                {
                    TempData["ErrorMessage"] = "Tender not found.";
                    return RedirectToAction("Index");
                }

                if (!existingTender.IsEditable)
                {
                    TempData["ErrorMessage"] = "This tender cannot be edited as it has been published.";
                    return RedirectToAction("Details", new { id = model.Id });
                }

                // Check if tender ID already exists (excluding current tender)
                if (await _tenderRepository.ExistsAsync(model.TenderId, model.Id))
                {
                    ModelState.AddModelError("TenderId", "A tender with this ID already exists.");
                    ViewBag.Departments = GetDepartmentOptions();
                    return View(model);
                }

                // Validate date ranges
                if (model.LastDateEmd <= model.PublishDate)
                {
                    ModelState.AddModelError("LastDateEmd", "Last date to pay EMD must be after publish date.");
                    ViewBag.Departments = GetDepartmentOptions();
                    return View(model);
                }

                existingTender.TenderId = model.TenderId;
                existingTender.TenderTitle = model.TenderTitle;
                existingTender.Description = model.Description;
                existingTender.Department = model.Department;
                existingTender.PublishDate = model.PublishDate;
                existingTender.EmdAmount = model.EmdAmount;
                existingTender.SdAmount = model.SdAmount;
                existingTender.ProcessingFee = model.ProcessingFee;
                existingTender.EstimatedValue = model.EstimatedValue;
                existingTender.LastDateEmd = model.LastDateEmd;
                existingTender.TenderClosingDate = model.TenderClosingDate;
                existingTender.TenderOpeningDate = model.TenderOpeningDate;
                existingTender.UpdatedAt = DateTime.UtcNow;

                var success = await _tenderRepository.UpdateAsync(existingTender);
                if (success)
                {
                    _logger.LogInformation($"Tender {model.TenderId} updated successfully by user {GetCurrentUserId()}");
                    TempData["SuccessMessage"] = $"Tender {model.TenderId} has been updated successfully.";
                    return RedirectToAction("Details", new { id = model.Id });
                }

                ModelState.AddModelError("", "Failed to update the tender. Please try again.");
                ViewBag.Departments = GetDepartmentOptions();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tender");
                ModelState.AddModelError("", "An error occurred while updating the tender. Please try again.");
                ViewBag.Departments = GetDepartmentOptions();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(id);
                if (tender == null)
                {
                    TempData["ErrorMessage"] = "Tender not found.";
                    return RedirectToAction("Index");
                }

                if (!tender.IsEditable)
                {
                    TempData["ErrorMessage"] = "This tender cannot be deleted as it has been published.";
                    return RedirectToAction("Details", new { id });
                }

                var success = await _tenderRepository.DeleteAsync(id);
                if (success)
                {
                    _logger.LogInformation($"Tender {tender.TenderId} deleted successfully by user {GetCurrentUserId()}");
                    TempData["SuccessMessage"] = $"Tender {tender.TenderId} has been deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete the tender. Please try again.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tender");
                TempData["ErrorMessage"] = "An error occurred while deleting the tender.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Tender Status Operations

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(id);
                if (tender == null)
                {
                    TempData["ErrorMessage"] = "Tender not found.";
                    return RedirectToAction("Index");
                }

                if (tender.Status != TenderStatus.Draft)
                {
                    TempData["ErrorMessage"] = "Only draft tenders can be published.";
                    return RedirectToAction("Details", new { id });
                }

                var success = await _tenderRepository.PublishTenderAsync(id);
                if (success)
                {
                    _logger.LogInformation($"Tender {tender.TenderId} published successfully by user {GetCurrentUserId()}");
                    TempData["SuccessMessage"] = $"Tender {tender.TenderId} has been published successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to publish the tender. Please try again.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing tender");
                TempData["ErrorMessage"] = "An error occurred while publishing the tender.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(id);
                if (tender == null)
                {
                    TempData["ErrorMessage"] = "Tender not found.";
                    return RedirectToAction("Index");
                }

                if (tender.Status != TenderStatus.Published)
                {
                    TempData["ErrorMessage"] = "Only published tenders can be closed.";
                    return RedirectToAction("Details", new { id });
                }

                var success = await _tenderRepository.CloseTenderAsync(id);
                if (success)
                {
                    _logger.LogInformation($"Tender {tender.TenderId} closed successfully by user {GetCurrentUserId()}");
                    TempData["SuccessMessage"] = $"Tender {tender.TenderId} has been closed successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to close the tender. Please try again.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing tender");
                TempData["ErrorMessage"] = "An error occurred while closing the tender.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(id);
                if (tender == null)
                {
                    TempData["ErrorMessage"] = "Tender not found.";
                    return RedirectToAction("Index");
                }

                if (tender.Status == TenderStatus.Closed)
                {
                    TempData["ErrorMessage"] = "Closed tenders cannot be cancelled.";
                    return RedirectToAction("Details", new { id });
                }

                var success = await _tenderRepository.CancelTenderAsync(id);
                if (success)
                {
                    _logger.LogInformation($"Tender {tender.TenderId} cancelled successfully by user {GetCurrentUserId()}");
                    TempData["SuccessMessage"] = $"Tender {tender.TenderId} has been cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel the tender. Please try again.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling tender");
                TempData["ErrorMessage"] = "An error occurred while cancelling the tender.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Tender Documents

        [HttpGet]
        public async Task<IActionResult> Documents(int id)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(id);
                if (tender == null)
                {
                    TempData["ErrorMessage"] = "Tender not found.";
                    return RedirectToAction("Index");
                }

                var documents = await _tenderRepository.GetTenderDocumentsAsync(id);
                var viewModel = new TenderDocumentViewModel
                {
                    TenderId = id
                };

                ViewBag.Tender = tender;
                ViewBag.Documents = documents;
                ViewBag.DocumentTypes = GetDocumentTypeOptions();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tender documents");
                TempData["ErrorMessage"] = "An error occurred while loading tender documents.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(TenderDocumentViewModel model)
        {
            try
            {
                if (model.File == null || model.File.Length == 0)
                {
                    ModelState.AddModelError("File", "Please select a file to upload.");
                    return RedirectToAction("Documents", new { id = model.TenderId });
                }

                // Validate file
                if (!ValidateFile(model.File))
                {
                    return RedirectToAction("Documents", new { id = model.TenderId });
                }

                // Save file
                var fileName = await SaveFileAsync(model.File);
                var filePath = Path.Combine("uploads", "tenders", model.TenderId.ToString(), fileName);

                var document = new TenderDocument
                {
                    TenderId = model.TenderId,
                    DocumentName = model.DocumentName,
                    FileName = model.File.FileName,
                    FilePath = filePath,
                    FileSize = model.File.Length,
                    ContentType = model.File.ContentType,
                    DocumentType = model.DocumentType,
                    IsRequired = model.IsRequired,
                    UploadedBy = GetCurrentUserId(),
                    UploadedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var documentId = await _tenderRepository.AddTenderDocumentAsync(document);
                _logger.LogInformation($"Document {model.DocumentName} uploaded for tender {model.TenderId} by user {GetCurrentUserId()}");

                TempData["SuccessMessage"] = "Document uploaded successfully.";
                return RedirectToAction("Documents", new { id = model.TenderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                TempData["ErrorMessage"] = "An error occurred while uploading the document.";
                return RedirectToAction("Documents", new { id = model.TenderId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int id, int tenderId)
        {
            try
            {
                var success = await _tenderRepository.DeleteTenderDocumentAsync(id);
                if (success)
                {
                    _logger.LogInformation($"Document {id} deleted for tender {tenderId} by user {GetCurrentUserId()}");
                    TempData["SuccessMessage"] = "Document deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete the document.";
                }

                return RedirectToAction("Documents", new { id = tenderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document");
                TempData["ErrorMessage"] = "An error occurred while deleting the document.";
                return RedirectToAction("Documents", new { id = tenderId });
            }
        }

        #endregion

        #region Helper Methods

        private TenderViewModel MapToViewModel(Tender tender)
        {
            return new TenderViewModel
            {
                Id = tender.Id,
                TenderId = tender.TenderId,
                TenderTitle = tender.TenderTitle,
                Description = tender.Description,
                Department = tender.Department,
                PublishDate = tender.PublishDate,
                EmdAmount = tender.EmdAmount,
                SdAmount = tender.SdAmount,
                ProcessingFee = tender.ProcessingFee,
                EstimatedValue = tender.EstimatedValue,
                LastDateEmd = tender.LastDateEmd,
                TenderClosingDate = tender.TenderClosingDate,
                TenderOpeningDate = tender.TenderOpeningDate,
                Status = tender.Status,
                CreatedByUserName = tender.CreatedByUser?.FullName ?? "Unknown",
                CreatedAt = tender.CreatedAt,
                UpdatedAt = tender.UpdatedAt,
                PublishedAt = tender.PublishedAt
            };
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private async Task<List<string>> GetDepartmentsAsync()
        {
            // This could be made dynamic by reading from a database table
            return new List<string>
            {
                "Procurement",
                "Finance",
                "Operations",
                "IT",
                "HR",
                "Legal",
                "Marketing",
                "Sales"
            };
        }

        private List<SelectListItem> GetDepartmentOptions()
        {
            var departments = new List<string>
            {
                "Procurement",
                "Finance",
                "Operations",
                "IT",
                "HR",
                "Legal",
                "Marketing",
                "Sales"
            };

            return departments.Select(d => new SelectListItem
            {
                Value = d,
                Text = d
            }).ToList();
        }

        private List<SelectListItem> GetDocumentTypeOptions()
        {
            return Enum.GetValues<TenderDocumentType>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.GetDisplayName()
                }).ToList();
        }

        private bool ValidateFile(IFormFile file)
        {
            // Check file size (10MB limit)
            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "File size must be less than 10MB.";
                return false;
            }

            // Check file type
            var allowedTypes = new[]
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            if (!allowedTypes.Contains(file.ContentType))
            {
                TempData["ErrorMessage"] = "Only PDF, DOC, DOCX, XLS, and XLSX files are allowed.";
                return false;
            }

            return true;
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tenders");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        #endregion
    }
}
