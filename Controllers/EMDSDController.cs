using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiddingSystem.Controllers
{
    [Authorize]
    public class EMDSDController : Controller
    {
        private readonly IEMDSDService _emdsdService;
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly ILogger<EMDSDController> _logger;

        public EMDSDController(IEMDSDService emdsdService, ITenderRepository tenderRepository, ITenderBidRepository tenderBidRepository, ILogger<EMDSDController> logger)
        {
            _emdsdService = emdsdService;
            _tenderRepository = tenderRepository;
            _tenderBidRepository = tenderBidRepository;
            _logger = logger;
        }

        // GET: EMDSD
        public async Task<IActionResult> Index(int page = 1, int pageSize = 25, string? searchTerm = null, string? status = null, string? type = null)
        {
            try
            {
                var (deposits, totalCount) = await _emdsdService.GetDepositsPagedAsync(page, pageSize, searchTerm, status, type);
                
                var viewModel = new EMDSDDepositListViewModel
                {
                    Deposits = deposits.Select(d => new EMDSDDepositViewModel
                    {
                        Id = d.Id,
                        DepositId = d.DepositId,
                        TenderId = d.TenderId,
                        TenderBidId = d.TenderBidId,
                        TenderName = d.Tender?.TenderTitle ?? "N/A",
                        Amount = d.Amount,
                        BidderName = d.BidderName,
                        CompanyName = d.CompanyName,
                        TransactionDate = d.TransactionDate,
                        BankName = d.BankName,
                        FSSAIBranchName = d.FSSAIBranchName,
                        TransactionId = d.TransactionId,
                        Status = d.Status,
                        Type = d.Type,
                        Remarks = d.Remarks,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt,
                        Tender = d.Tender,
                        Transactions = d.Transactions.ToList()
                    }).ToList(),
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    SearchTerm = searchTerm,
                    Status = status,
                    Type = type
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading deposits: " + ex.Message;
                return View(new EMDSDDepositListViewModel());
            }
        }

        // GET: EMDSD/GetTenderBids
        [HttpGet]
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

        // GET: EMDSD/GetTenderDetails
        [HttpGet]
        public async Task<IActionResult> GetTenderDetails(int tenderId)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(tenderId);
                if (tender == null)
                {
                    return Json(new { error = "Tender not found" });
                }

                var result = new
                {
                    id = tender.Id,
                    tenderId = tender.TenderId,
                    tenderTitle = tender.TenderTitle,
                    emdAmount = tender.EmdAmount,
                    sdAmount = tender.SdAmount,
                    processingFee = tender.ProcessingFee,
                    estimatedValue = tender.EstimatedValue
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender details for tender: {TenderId}", tenderId);
                return Json(new { error = "Error loading tender details" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestCreateDeposit()
        {
            try
            {
                // Create a test deposit with TenderBidId
                var testDeposit = new EMDSDDeposit
                {
                    TenderId = 1, // Assuming tender with ID 1 exists
                    TenderBidId = 1, // Assuming tender bid with ID 1 exists
                    Amount = 1000.00m,
                    BidderName = "Test Bidder",
                    CompanyName = "Test Company",
                    TransactionDate = DateTime.Today,
                    BankName = "Test Bank",
                    FSSAIBranchName = "Test Branch",
                    TransactionId = "TEST-" + DateTime.Now.Ticks,
                    Type = "EMD",
                    Remarks = "Test deposit for TenderBidId validation",
                    Status = "Pending"
                };

                var createdDeposit = await _emdsdService.CreateDepositAsync(testDeposit);
                
                return Json(new { 
                    success = true, 
                    message = "Test deposit created successfully",
                    depositId = createdDeposit.Id,
                    tenderBidId = createdDeposit.TenderBidId,
                    tenderId = createdDeposit.TenderId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test deposit");
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: EMDSD/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var deposit = await _emdsdService.GetDepositByIdAsync(id);
                if (deposit == null)
                {
                    return NotFound();
                }

                var viewModel = new EMDSDDepositViewModel
                {
                    Id = deposit.Id,
                    DepositId = deposit.DepositId,
                    TenderId = deposit.TenderId,
                    TenderBidId = deposit.TenderBidId,
                    TenderName = deposit.Tender?.TenderTitle ?? "N/A",
                    Amount = deposit.Amount,
                    BidderName = deposit.BidderName,
                    CompanyName = deposit.CompanyName,
                    TransactionDate = deposit.TransactionDate,
                    BankName = deposit.BankName,
                    FSSAIBranchName = deposit.FSSAIBranchName,
                    TransactionId = deposit.TransactionId,
                    Status = deposit.Status,
                    Type = deposit.Type,
                    Remarks = deposit.Remarks,
                    CreatedAt = deposit.CreatedAt,
                    UpdatedAt = deposit.UpdatedAt,
                    Tender = deposit.Tender,
                    Transactions = deposit.Transactions.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading deposit details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: EMDSD/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var tenders = await _tenderRepository.GetAllAsync();
                var viewModel = new EMDSDDepositCreateViewModel
                {
                    AvailableTenders = tenders.ToList(),
                    TransactionDate = DateTime.Today
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading create form: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: EMDSD/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EMDSDDepositCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var tenders = await _tenderRepository.GetAllAsync();
                    model.AvailableTenders = tenders.ToList();
                    return View(model);
                }

                var deposit = new EMDSDDeposit
                {
                    TenderId = model.TenderId,
                    TenderBidId = model.TenderBidId,
                    Amount = model.Amount,
                    BidderName = model.BidderName,
                    CompanyName = model.CompanyName,
                    TransactionDate = model.TransactionDate,
                    BankName = model.BankName,
                    FSSAIBranchName = model.FSSAIBranchName,
                    TransactionId = model.TransactionId,
                    Type = model.Type,
                    Remarks = model.Remarks,
                    Status = "Pending"
                };

                await _emdsdService.CreateDepositAsync(deposit);
                TempData["SuccessMessage"] = "EMD/SD deposit created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating deposit: " + ex.Message;
                var tenders = await _tenderRepository.GetAllAsync();
                model.AvailableTenders = tenders.ToList();
                return View(model);
            }
        }

        // GET: EMDSD/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var deposit = await _emdsdService.GetDepositByIdAsync(id);
                if (deposit == null)
                {
                    return NotFound();
                }

                var tenders = await _tenderRepository.GetAllAsync();
                var viewModel = new EMDSDDepositEditViewModel
                {
                    Id = deposit.Id,
                    DepositId = deposit.DepositId,
                    TenderId = deposit.TenderId,
                    TenderBidId = deposit.TenderBidId,
                    Amount = deposit.Amount,
                    BidderName = deposit.BidderName,
                    CompanyName = deposit.CompanyName,
                    TransactionDate = deposit.TransactionDate,
                    BankName = deposit.BankName,
                    FSSAIBranchName = deposit.FSSAIBranchName,
                    TransactionId = deposit.TransactionId,
                    Status = deposit.Status,
                    Type = deposit.Type,
                    Remarks = deposit.Remarks,
                    AvailableTenders = tenders.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading edit form: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: EMDSD/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EMDSDDepositEditViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    var tenders = await _tenderRepository.GetAllAsync();
                    model.AvailableTenders = tenders.ToList();
                    return View(model);
                }

                var deposit = await _emdsdService.GetDepositByIdAsync(id);
                if (deposit == null)
                {
                    return NotFound();
                }

                deposit.TenderId = model.TenderId;
                deposit.TenderBidId = model.TenderBidId;
                deposit.Amount = model.Amount;
                deposit.BidderName = model.BidderName;
                deposit.CompanyName = model.CompanyName;
                deposit.TransactionDate = model.TransactionDate;
                deposit.BankName = model.BankName;
                deposit.FSSAIBranchName = model.FSSAIBranchName;
                deposit.TransactionId = model.TransactionId;
                deposit.Status = model.Status;
                deposit.Type = model.Type;
                deposit.Remarks = model.Remarks;

                await _emdsdService.UpdateDepositAsync(deposit);
                TempData["SuccessMessage"] = "EMD/SD deposit updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating deposit: " + ex.Message;
                var tenders = await _tenderRepository.GetAllAsync();
                model.AvailableTenders = tenders.ToList();
                return View(model);
            }
        }

        // GET: EMDSD/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deposit = await _emdsdService.GetDepositByIdAsync(id);
                if (deposit == null)
                {
                    return NotFound();
                }

                var viewModel = new EMDSDDepositViewModel
                {
                    Id = deposit.Id,
                    DepositId = deposit.DepositId,
                    TenderId = deposit.TenderId,
                    TenderBidId = deposit.TenderBidId,
                    TenderName = deposit.Tender?.TenderTitle ?? "N/A",
                    Amount = deposit.Amount,
                    BidderName = deposit.BidderName,
                    CompanyName = deposit.CompanyName,
                    TransactionDate = deposit.TransactionDate,
                    BankName = deposit.BankName,
                    FSSAIBranchName = deposit.FSSAIBranchName,
                    TransactionId = deposit.TransactionId,
                    Status = deposit.Status,
                    Type = deposit.Type,
                    Remarks = deposit.Remarks,
                    CreatedAt = deposit.CreatedAt,
                    UpdatedAt = deposit.UpdatedAt,
                    Tender = deposit.Tender,
                    Transactions = deposit.Transactions.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading delete confirmation: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: EMDSD/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _emdsdService.DeleteDepositAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "EMD/SD deposit deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Deposit not found or could not be deleted.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting deposit: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: EMDSD/ProcessPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int id, string transactionId, string bankReference)
        {
            try
            {
                await _emdsdService.ProcessPaymentAsync(id, transactionId, bankReference);
                TempData["SuccessMessage"] = "Payment processed successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing payment: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: EMDSD/ProcessRefund/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessRefund(int id, string reason)
        {
            try
            {
                await _emdsdService.ProcessRefundAsync(id, reason);
                TempData["SuccessMessage"] = "Refund processed successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing refund: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: EMDSD/Search
        public async Task<IActionResult> Search(string? depositId, string? tenderName, string? status, string? type)
        {
            try
            {
                var deposits = await _emdsdService.SearchDepositsAsync(depositId, tenderName, status, type);
                
                var viewModel = new EMDSDDepositListViewModel
                {
                    Deposits = deposits.Select(d => new EMDSDDepositViewModel
                    {
                        Id = d.Id,
                        DepositId = d.DepositId,
                        TenderId = d.TenderId,
                        TenderBidId = d.TenderBidId,
                        TenderName = d.Tender?.TenderTitle ?? "N/A",
                        Amount = d.Amount,
                        BidderName = d.BidderName,
                        CompanyName = d.CompanyName,
                        TransactionDate = d.TransactionDate,
                        BankName = d.BankName,
                        FSSAIBranchName = d.FSSAIBranchName,
                        TransactionId = d.TransactionId,
                        Status = d.Status,
                        Type = d.Type,
                        Remarks = d.Remarks,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt,
                        Tender = d.Tender,
                        Transactions = d.Transactions.ToList()
                    }).ToList(),
                    SearchTerm = depositId,
                    Status = status,
                    Type = type
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while searching deposits: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
