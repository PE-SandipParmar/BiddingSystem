using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;

namespace BiddingSystem.Controllers
{
    public class BidderPaymentController : Controller
    {
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IPaymentLinkRepository _paymentLinkRepository;
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly ILogger<BidderPaymentController> _logger;

        public BidderPaymentController(
            ITenderBidRepository tenderBidRepository,
            IPaymentLinkRepository paymentLinkRepository,
            IPaymentTransactionRepository transactionRepository,
            ITenderRepository tenderRepository,
            ILogger<BidderPaymentController> logger)
        {
            _tenderBidRepository = tenderBidRepository;
            _paymentLinkRepository = paymentLinkRepository;
            _transactionRepository = transactionRepository;
            _tenderRepository = tenderRepository;
            _logger = logger;
        }

        // GET: BidderPayment/Dashboard
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Dashboard(string email = null, string phone = null, int? bidId = null)
        {
            try
            {
                // Validate input - require at least one identifier
                if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone) && !bidId.HasValue)
                {
                    return View("BidderLogin");
                }

                // Find bids based on provided identifiers
                List<TenderBid> bidderBids = new List<TenderBid>();

                if (bidId.HasValue)
                {
                    var bid = await _tenderBidRepository.GetBidByIdAsync(bidId.Value);
                    if (bid != null)
                    {
                        // Verify email or phone matches if provided
                        if (!string.IsNullOrEmpty(email) && bid.BidderEmail != email)
                        {
                            TempData["ErrorMessage"] = "Invalid credentials.";
                            return View("BidderLogin");
                        }
                        if (!string.IsNullOrEmpty(phone) && bid.BidderPhone != phone)
                        {
                            TempData["ErrorMessage"] = "Invalid credentials.";
                            return View("BidderLogin");
                        }
                        bidderBids.Add(bid);
                    }
                }
                else
                {
                    // Search by email or phone
                    bidderBids = await _tenderBidRepository.GetBidsByEmailOrPhoneAsync(email, phone);
                }

                if (!bidderBids.Any())
                {
                    TempData["ErrorMessage"] = "No bids found for the provided information.";
                    return View("BidderLogin");
                }

                // Create view model
                var viewModel = new BidderPaymentDashboardViewModel
                {
                    BidderName = bidderBids.First().BidderName,
                    BidderEmail = bidderBids.First().BidderEmail,
                    BidderPhone = bidderBids.First().BidderPhone,
                    CompanyName = bidderBids.First().CompanyName,
                    TenderBids = new List<BidderPaymentTenderViewModel>()
                };

                // Process each bid and get payment information
                foreach (var bid in bidderBids)
                {
                    var tender = await _tenderRepository.GetByIdAsync(bid.TenderId);
                    var paymentLinks = await _paymentLinkRepository.GetByTenderBidIdAsync(bid.Id);

                    var tenderViewModel = new BidderPaymentTenderViewModel
                    {
                        BidId = bid.Id,
                        TenderId = tender.Id,
                        TenderTitle = tender.TenderTitle,
                        TenderRefNo = tender.TenderId,
                        BidAmount = Convert.ToDecimal(bid.BidAmount),
                        EmdAmount = Convert.ToDecimal(bid.EmdAmount),
                        ProcessingFee = Convert.ToDecimal(bid.ProcessingFee),
                        TotalAmount = bid.TotalAmount,
                        BidStatus = bid.Status,
                        OverallPaymentStatus = bid.PaymentStatus,
                        SubmittedDate = bid.SubmittedAt,
                        PaymentComponents = new List<PaymentComponentViewModel>()
                    };

                    // Add payment component details
                    var paymentTypes = new[]
                    {
                        new { Type = PaymentType.EMD, Amount = bid.BidAmount, Label = "Bid Amount" },
                        new { Type = PaymentType.SD, Amount = bid.EmdAmount, Label = "EMD Amount" },
                        new { Type = PaymentType.ProcessingFee, Amount = bid.ProcessingFee, Label = "Processing Fee" }
                    };

                    foreach (var paymentType in paymentTypes.Where(pt => pt.Amount > 0))
                    {
                        var link = paymentLinks.FirstOrDefault(l => l.PaymentType == paymentType.Type);
                        List<PaymentTransaction> transactions;
                        if (link != null)
                        {
                            transactions = await _transactionRepository.GetByPaymentLinkIdAsync(link.LinkId);
                        }
                        else
                        {
                            transactions = new List<PaymentTransaction>();
                        }

                        var successfulTransaction = transactions.FirstOrDefault(t => t.Status == "Success");
                        var latestTransaction = transactions.OrderByDescending(t => t.TransactionDate).FirstOrDefault();

                        var component = new PaymentComponentViewModel
                        {
                            PaymentType = paymentType.Type,
                            PaymentTypeLabel = paymentType.Label,
                            Amount = Convert.ToDecimal(paymentType.Amount),
                            PaymentLinkId = link?.Id,
                            PaymentLinkUrl = link?.PaymentUrl,
                            SecurityToken = link?.SecurityToken,
                            LinkStatus = link?.Status ?? PaymentLinkStatus.NotGenerated,
                            IsPaid = successfulTransaction != null,
                            PaymentDate = successfulTransaction?.TransactionDate,
                            TransactionId = successfulTransaction?.RazorpayPaymentId,
                            PaymentMethod = successfulTransaction?.PaymentMethod,
                            IsExpired = link?.IsExpired ?? false,
                            ExpiryDate = link?.ExpiryDate,
                            FailedAttempts = transactions.Count(t => t.Status == "Failed"),
                            LastAttemptDate = latestTransaction?.TransactionDate,
                            PaymentHistory = transactions.Select(t => new PaymentHistoryViewModel
                            {
                                TransactionId = t.RazorpayPaymentId,
                                Amount = t.Amount,
                                Status = t.Status,
                                PaymentMethod = t.PaymentMethod,
                                TransactionDate = t.TransactionDate,
                                ErrorDescription = t.ErrorDescription
                            }).ToList()
                        };

                        tenderViewModel.PaymentComponents.Add(component);
                    }

                    // Calculate payment summary
                    tenderViewModel.TotalPaid = tenderViewModel.PaymentComponents
                        .Where(pc => pc.IsPaid)
                        .Sum(pc => pc.Amount);

                    tenderViewModel.TotalPending = tenderViewModel.TotalAmount - tenderViewModel.TotalPaid;

                    tenderViewModel.IsFullyPaid = tenderViewModel.TotalPending <= 0;

                    viewModel.TenderBids.Add(tenderViewModel);
                }

                // Calculate overall summary
                viewModel.TotalBids = viewModel.TenderBids.Count;
                viewModel.TotalAmountDue = viewModel.TenderBids.Sum(tb => tb.TotalAmount);
                viewModel.TotalAmountPaid = viewModel.TenderBids.Sum(tb => tb.TotalPaid);
                viewModel.TotalAmountPending = viewModel.TotalAmountDue - viewModel.TotalAmountPaid;

                return View("Dashboard", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bidder payment dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the payment dashboard.";
                return View("BidderLogin");
            }
        }

        // GET: BidderPayment/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("BidderLogin");
        }

        // POST: BidderPayment/Login
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(string identifier, string verificationType)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                TempData["ErrorMessage"] = "Please enter email or phone number.";
                return View("BidderLogin");
            }

            // Redirect to dashboard with credentials
            if (verificationType == "email")
            {
                return RedirectToAction("Dashboard", new { email = identifier });
            }
            else
            {
                return RedirectToAction("Dashboard", new { phone = identifier });
            }
        }

        // GET: BidderPayment/PaymentReceipt/{transactionId}
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentReceipt(string transactionId)
        {
            try
            {
                var transaction = await _transactionRepository.GetByPaymentIdAsync(transactionId);
                if (transaction == null)
                {
                    TempData["ErrorMessage"] = "Payment receipt not found.";
                    return RedirectToAction("Dashboard");
                }

                // Get related information
                var paymentLink = await _paymentLinkRepository.GetByLinkIdAsync(transaction.PaymentLinkId);
                var tenderBid = paymentLink?.TenderBidId != null
                    ? await _tenderBidRepository.GetBidByIdAsync(paymentLink.TenderBidId.Value)
                    : null;
                var tender = paymentLink != null
                    ? await _tenderRepository.GetByIdAsync(paymentLink.TenderId)
                    : null;

                var viewModel = new PaymentReceiptViewModel
                {
                    TransactionId = transaction.RazorpayPaymentId,
                    OrderId = transaction.RazorpayOrderId,
                    Amount = transaction.Amount,
                    PaymentDate = transaction.TransactionDate,
                    PaymentMethod = transaction.PaymentMethod,
                    Status = transaction.Status,
                    PaymentType = paymentLink?.PaymentType ?? PaymentType.ProcessingFee,

                    // Bidder Information
                    BidderName = tenderBid?.BidderName ?? "N/A",
                    BidderEmail = tenderBid?.BidderEmail ?? "N/A",
                    BidderPhone = tenderBid?.BidderPhone ?? "N/A",
                    CompanyName = tenderBid?.CompanyName ?? "N/A",

                    // Tender Information
                    TenderTitle = tender?.TenderTitle ?? "N/A",
                    TenderRefNo = tender?.TenderId ?? "N/A",

                    // Additional Details
                    BankName = transaction.BankName,
                    CardLast4 = transaction.CardLast4,
                    UPIId = transaction.UPIId,
                    WalletName = transaction.WalletName
                };

                return View("PaymentReceipt", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment receipt: {TransactionId}", transactionId);
                TempData["ErrorMessage"] = "An error occurred while loading the payment receipt.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: BidderPayment/RefundStatus
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RefundStatus(int bidId)
        {
            try
            {
                var bid = await _tenderBidRepository.GetBidByIdAsync(bidId);
                if (bid == null)
                {
                    return Json(new { success = false, message = "Bid not found." });
                }

                // Check for refund information
                var refundInfo = await _tenderBidRepository.GetRefundInfoForBidAsync(bidId);

                if (refundInfo == null)
                {
                    return Json(new { success = false, message = "No refund information available." });
                }

                return Json(new
                {
                    success = true,
                    refundStatus = refundInfo.RefundStatus,
                    refundAmount = refundInfo.RefundAmount,
                    refundId = refundInfo.RazorpayRefundId,
                    refundDate = refundInfo.ApprovedAt,
                    reason = refundInfo.ReasonForRefund
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking refund status for bid: {BidId}", bidId);
                return Json(new { success = false, message = "An error occurred while checking refund status." });
            }
        }
    }
}

// Add extension method to ITenderBidRepository
public static class TenderBidRepositoryExtensions
{
    public static async Task<List<TenderBid>> GetBidsByEmailOrPhoneAsync(
        this ITenderBidRepository repository,
        string email,
        string phone)
    {
        // This would need to be implemented in the repository
        // For now, using a workaround
        var allBids = await repository.GetAllBidsAsync();

        return allBids.Where(b =>
            (!string.IsNullOrEmpty(email) && b.BidderEmail == email) ||
            (!string.IsNullOrEmpty(phone) && b.BidderPhone == phone)
        ).ToList();
    }

    public static async Task<List<PaymentLink>> GetByTenderBidIdAsync(
        this IPaymentLinkRepository repository,
        int tenderBidId)
    {
        // This would need to be implemented in the repository
        var allLinks = await repository.GetAllAsync();
        return allLinks.Where(l => l.TenderBidId == tenderBidId).ToList();
    }
}