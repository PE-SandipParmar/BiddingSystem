using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiddingSystem.Data;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using System.Linq;

namespace BiddingSystem.Controllers
{
    public class BidderPaymentController : Controller
    {
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IPaymentLinkRepository _paymentLinkRepository;
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly IRefundRepository _refundRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly ILogger<BidderPaymentController> _logger;

        public BidderPaymentController(
            ITenderBidRepository tenderBidRepository,
            IPaymentLinkRepository paymentLinkRepository,
            IPaymentTransactionRepository transactionRepository,
            IRefundRepository refundRepository,
            ITenderRepository tenderRepository,
            ILogger<BidderPaymentController> logger)
        {
            _tenderBidRepository = tenderBidRepository;
            _paymentLinkRepository = paymentLinkRepository;
            _transactionRepository = transactionRepository;
            _refundRepository = refundRepository;
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
                    // Search by email or phone using the repository extension method
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

                decimal totalRefunded = 0;

                // Process each bid and get payment information
                foreach (var bid in bidderBids)
                {
                    var tender = await _tenderRepository.GetByIdAsync(bid.TenderId);

                    // Get payment links for this tender bid
                    var paymentLinks = await _paymentLinkRepository.GetByTenderIdAsync(bid.TenderId);
                    var bidPaymentLinks = paymentLinks.Where(pl => pl.TenderBidId == bid.Id).ToList();

                    // Get refund summary for this bid using the RefundRepository
                    var refundSummaries = await _refundRepository.GetPaymentSummaryForBidAsync(bid.Id);

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

                    decimal tenderTotalRefunded = 0;

                    // Add payment component details
                    var paymentTypes = new[]
                    {
                        new { Type = PaymentType.EMD, Amount = bid.BidAmount, Label = "Bid Amount (EMD)" },
                        new { Type = PaymentType.SD, Amount = bid.EmdAmount, Label = "Security Deposit" },
                        new { Type = PaymentType.ProcessingFee, Amount = bid.ProcessingFee, Label = "Processing Fee" }
                    };

                    foreach (var paymentType in paymentTypes.Where(pt => pt.Amount > 0))
                    {
                        var link = bidPaymentLinks.FirstOrDefault(l => l.PaymentType == paymentType.Type);

                        // Get transactions for this payment link
                        List<PaymentTransaction> transactions = new List<PaymentTransaction>();
                        if (link != null)
                        {
                            transactions = await _transactionRepository.GetByPaymentLinkIdAsync(link.LinkId);
                        }

                        var successfulTransaction = transactions.FirstOrDefault(t => t.Status == "Success");
                        var latestTransaction = transactions.OrderByDescending(t => t.TransactionDate).FirstOrDefault();

                        // Get refund information from refund summaries
                        PaymentRefundSummary refundSummary = null;
                        if (link != null)
                        {
                            refundSummary = refundSummaries.FirstOrDefault(rs => rs.PaymentLinkId == link.Id);
                        }

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

                            // Add refund information from refund summary
                            HasRefund = refundSummary?.RefundPaymentId != null,
                            RefundStatus = refundSummary?.RefundStatus,
                            RefundAmount = refundSummary?.RefundAmount ?? 0,
                            RefundTransactionId = refundSummary?.RazorpayRefundId,
                            RefundDate = refundSummary?.RefundApprovedDate,
                            RefundInitiatedDate = refundSummary?.RefundInitiatedDate,
                            RefundReason = "Tender awarded to another bidder", // Default reason
                            RefundRemarks = refundSummary?.RefundErrorMessage,

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

                        // Calculate refunded amount for this tender
                        if (refundSummary != null && refundSummary.RefundStatus == "Approved" && refundSummary.RefundAmount > 0)
                        {
                            tenderTotalRefunded += refundSummary.RefundAmount;
                        }

                        tenderViewModel.PaymentComponents.Add(component);
                    }

                    // Calculate payment summary
                    tenderViewModel.TotalPaid = tenderViewModel.PaymentComponents
                        .Where(pc => pc.IsPaid)
                        .Sum(pc => pc.Amount);

                    tenderViewModel.TotalPending = tenderViewModel.TotalAmount - tenderViewModel.TotalPaid;
                    tenderViewModel.TotalRefunded = tenderTotalRefunded;
                    tenderViewModel.IsFullyPaid = tenderViewModel.TotalPending <= 0;
                    tenderViewModel.HasRefunds = tenderViewModel.PaymentComponents.Any(pc => pc.HasRefund);

                    totalRefunded += tenderTotalRefunded;
                    viewModel.TenderBids.Add(tenderViewModel);
                }

                // Calculate overall summary
                viewModel.TotalBids = viewModel.TenderBids.Count;
                viewModel.TotalAmountDue = viewModel.TenderBids.Sum(tb => tb.TotalAmount);
                viewModel.TotalAmountPaid = viewModel.TenderBids.Sum(tb => tb.TotalPaid);
                viewModel.TotalAmountPending = viewModel.TotalAmountDue - viewModel.TotalAmountPaid;
                viewModel.TotalRefunded = totalRefunded;

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

        // GET: BidderPayment/RefundStatusDetailed
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RefundStatusDetailed(int bidId)
        {
            try
            {
                var bid = await _tenderBidRepository.GetBidByIdAsync(bidId);
                if (bid == null)
                {
                    return Json(new { success = false, message = "Bid not found." });
                }

                var tender = await _tenderRepository.GetByIdAsync(bid.TenderId);

                // Use RefundRepository to get payment summary with refund details
                var paymentSummaries = await _refundRepository.GetPaymentSummaryForBidAsync(bidId);

                var refundDetails = new RefundDetailsViewModel
                {
                    BidId = bidId,
                    TenderTitle = tender?.TenderTitle,
                    BidderName = bid.BidderName,
                    RefundComponents = new List<RefundComponentViewModel>()
                };

                decimal totalPaid = 0;
                decimal totalRefunded = 0;

                foreach (var summary in paymentSummaries.Where(ps => ps.RazorpayPaymentId != null))
                {
                    totalPaid += summary.Amount;

                    var refundComponent = new RefundComponentViewModel
                    {
                        PaymentType = summary.PaymentTypeDisplay,
                        Amount = summary.Amount,
                        Status = summary.RefundStatus ?? "Not Initiated",
                        RefundId = summary.RazorpayRefundId,
                        RefundDate = summary.RefundApprovedDate,
                        RefundMethod = "Razorpay",
                        Reason = "Tender awarded to another bidder"
                    };

                    if (summary.RefundStatus == "Approved" && summary.RefundAmount > 0)
                    {
                        totalRefunded += summary.RefundAmount;
                    }

                    refundDetails.RefundComponents.Add(refundComponent);
                }

                refundDetails.TotalPaid = totalPaid;
                refundDetails.TotalRefunded = totalRefunded;
                refundDetails.PendingRefund = totalPaid - totalRefunded;

                return Json(new
                {
                    success = true,
                    totalPaid = totalPaid.ToString("N2"),
                    totalRefunded = totalRefunded.ToString("N2"),
                    refundComponents = refundDetails.RefundComponents,
                    hasUpdate = totalRefunded > 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking refund status for bid: {BidId}", bidId);
                return Json(new { success = false, message = "An error occurred while checking refund status." });
            }
        }

        // GET: BidderPayment/GetPaymentHistory
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentHistory(string linkId)
        {
            try
            {
                var transactions = await _transactionRepository.GetByPaymentLinkIdAsync(linkId);

                var history = transactions.Select(t => new
                {
                    status = t.Status,
                    transactionDate = t.TransactionDate.ToString("dd MMM yyyy HH:mm"),
                    amount = t.Amount.ToString("N2"),
                    paymentMethod = t.PaymentMethod,
                    transactionId = t.RazorpayPaymentId,
                    errorDescription = t.ErrorDescription
                }).ToList();

                return Json(new { success = true, history });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history");
                return Json(new { success = false, message = "Error loading payment history" });
            }
        }

        // GET: BidderPayment/RefundReceipt/{refundPaymentId}
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RefundReceipt(int refundPaymentId)
        {
            try
            {
                // Get refund details using RefundRepository
                var refundDetails = await _refundRepository.GetRefundDetailsAsync(refundPaymentId);
                if (refundDetails == null)
                {
                    TempData["ErrorMessage"] = "Refund receipt not found.";
                    return RedirectToAction("Dashboard");
                }

                // Get tender information
                var tender = await _tenderRepository.GetByIdAsync(refundDetails.TenderId);

                var viewModel = new RefundReceiptViewModel
                {
                    RefundId = refundDetails.RazorpayRefundId,
                    PaymentId = refundDetails.OriginalPaymentId,
                    RefundAmount = refundDetails.RefundAmount,
                    RefundDate = refundDetails.ApprovedAt ?? refundDetails.InitiatedAt,
                    RefundStatus = refundDetails.RefundStatus,
                    RefundReason = refundDetails.ReasonForRefund,
                    PaymentType = refundDetails.PaymentTypeDisplay,

                    // Bidder Information
                    BidderName = refundDetails.BidderName ?? "N/A",
                    BidderEmail = refundDetails.BidderEmail ?? "N/A",
                    CompanyName = refundDetails.CompanyName ?? "N/A",

                    // Tender Information
                    TenderTitle = refundDetails.TenderTitle ?? "N/A",
                    TenderRefNo = refundDetails.TenderIdString ?? "N/A",

                    // Processing Information
                    ProcessedBy = refundDetails.ApprovedByName,
                    ProcessedDate = refundDetails.ApprovedAt,
                    RefundMethod = "Razorpay"
                };

                return View("RefundReceipt", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refund receipt: {RefundPaymentId}", refundPaymentId);
                TempData["ErrorMessage"] = "An error occurred while loading the refund receipt.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: BidderPayment/RefundStatus - Simple refund status check
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

                // Get refund information using RefundRepository
                var paymentSummaries = await _refundRepository.GetPaymentSummaryForBidAsync(bidId);
                var refundedSummaries = paymentSummaries.Where(ps => ps.RefundStatus == "Approved").ToList();

                if (!refundedSummaries.Any())
                {
                    return Json(new { success = false, message = "No refund information available." });
                }

                var totalRefunded = refundedSummaries.Sum(rs => rs.RefundAmount);
                var latestRefund = refundedSummaries.OrderByDescending(rs => rs.RefundApprovedDate).FirstOrDefault();

                return Json(new
                {
                    success = true,
                    refundStatus = "Approved",
                    refundAmount = totalRefunded,
                    refundId = latestRefund?.RazorpayRefundId,
                    refundDate = latestRefund?.RefundApprovedDate,
                    reason = "Tender awarded to another bidder"
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


public class PaymentRefundSummary
{
    public int PaymentLinkId { get; set; }
    public int TenderBidId { get; set; }
    public PaymentType PaymentType { get; set; }
    public string PaymentTypeDisplay { get; set; }
    public decimal Amount { get; set; }
    public string LinkId { get; set; }
    public int PaymentStatus { get; set; }
    public string RazorpayPaymentId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public int? RefundPaymentId { get; set; }
    public string RefundStatus { get; set; }
    public string RazorpayRefundId { get; set; }
    public DateTime? RefundInitiatedDate { get; set; }
    public DateTime? RefundApprovedDate { get; set; }
    public string RefundErrorMessage { get; set; }
    public decimal RefundAmount { get; set; }
}

public class RefundReceiptViewModel
{
    public string RefundId { get; set; }
    public string PaymentId { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime? RefundDate { get; set; }
    public string RefundStatus { get; set; }
    public string RefundReason { get; set; }
    public string PaymentType { get; set; }
    public string BidderName { get; set; }
    public string BidderEmail { get; set; }
    public string CompanyName { get; set; }
    public string TenderTitle { get; set; }
    public string TenderRefNo { get; set; }
    public string ProcessedBy { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string RefundMethod { get; set; }
}

// Payment Receipt ViewModel
public class PaymentReceiptViewModel
{
    public string TransactionId { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; }
    public string Status { get; set; }
    public PaymentType PaymentType { get; set; }

    // Bidder Information
    public string BidderName { get; set; }
    public string BidderEmail { get; set; }
    public string BidderPhone { get; set; }
    public string CompanyName { get; set; }

    // Tender Information
    public string TenderTitle { get; set; }
    public string TenderRefNo { get; set; }

    // Additional Payment Details
    public string BankName { get; set; }
    public string CardLast4 { get; set; }
    public string UPIId { get; set; }
    public string WalletName { get; set; }
}