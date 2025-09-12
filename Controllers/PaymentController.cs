using BiddingSystem.Data;
using BiddingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace BiddingSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentLinkRepository _paymentLinkRepository;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IRazorpayPaymentService _razorpayService;
        private readonly IWebhookEventRepository _webhookRepository;
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentLinkRepository paymentLinkRepository,
            ITenderBidRepository tenderBidRepository,
            IRazorpayPaymentService razorpayService,
            IWebhookEventRepository webhookRepository,
            IPaymentTransactionRepository transactionRepository,
            ILogger<PaymentController> logger)
        {
            _paymentLinkRepository = paymentLinkRepository;
            _tenderBidRepository = tenderBidRepository;
            _razorpayService = razorpayService;
            _webhookRepository = webhookRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        // GET: /Payment/Pay/{linkId}?token={securityToken}
        [HttpGet]
        [Route("Payment/Pay/{linkId}")]
        public async Task<IActionResult> Pay(string linkId, string token)
        {
            try
            {
                // Validate link ID
                if (string.IsNullOrEmpty(linkId) || string.IsNullOrEmpty(token))
                {
                    return View("InvalidLink");
                }

                var paymentLink = await _paymentLinkRepository.GetBySecurityTokenAsync(token);

                // Get payment link
                //var paymentLink = await _paymentLinkRepository.GetByLinkIdAsync(linkId);

                if (paymentLink == null)
                {
                    return View("InvalidLink");
                }

                // Validate security token
                if (!paymentLink.ValidateSecurityToken(token))
                {
                    return View("InvalidLink");
                }

                // Check if link is usable
                if (!paymentLink.IsUsable)
                {
                    var message = paymentLink.Status switch
                    {
                        PaymentLinkStatus.Used => "This payment link has already been used.",
                        PaymentLinkStatus.Expired => "This payment link has expired.",
                        PaymentLinkStatus.Cancelled => "This payment link has been cancelled.",
                        _ => "This payment link is no longer valid."
                    };

                    ViewBag.ErrorMessage = message;
                    return View("LinkNotAvailable");
                }

                // Get tender bid details
                TenderBid tenderBid = null;
                if (paymentLink.TenderBidId.HasValue)
                {
                    tenderBid = await _tenderBidRepository.GetBidByIdAsync(paymentLink.TenderBidId.Value);
                }

                // Create Razorpay order
                var razorpayOrder = await _razorpayService.CreateOrderAsync(paymentLink, tenderBid);

                // Create view model
                var viewModel = new PaymentViewModel
                {
                    PaymentLinkId = paymentLink.Id,
                    LinkId = paymentLink.LinkId,
                    TenderId = paymentLink.TenderId,
                    TenderBidId = paymentLink.TenderBidId,
                    Amount = paymentLink.Amount,
                    PaymentType = paymentLink.PaymentType,

                    // Bidder details from TenderBid
                    BidderName = tenderBid?.BidderName ?? "",
                    BidderEmail = tenderBid?.BidderEmail ?? "",
                    BidderPhone = tenderBid?.BidderPhone ?? "",
                    CompanyName = tenderBid?.CompanyName ?? "",

                    // Razorpay order details
                    RazorpayOrderId = razorpayOrder.OrderId,
                    RazorpayKey = razorpayOrder.RazorpayKey,
                    RazorpayAmount = razorpayOrder.Amount,

                    // Additional info
                    ExpiryDate = paymentLink.ExpiryDate,
                    TenderTitle = paymentLink.Tender?.TenderTitle ?? ""
                };

                return View("RazorpayCheckout", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment link: {LinkId}", linkId);
                return View("Error");
            }
        }

        // POST: /Payment/VerifyPayment
        [HttpPost]
        [Route("Payment/VerifyPayment")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationRequest request)
        {
            try
            {
                // Verify payment with Razorpay
                var verificationResult = await _razorpayService.VerifyPaymentAsync(
                    request.RazorpayPaymentId,
                    request.RazorpayOrderId,
                    request.RazorpaySignature
                );

                // Get payment link details
                var paymentLink = await _paymentLinkRepository.GetByIdAsync(request.PaymentLinkId);

                // Create payment transaction record
                var transaction = new PaymentTransaction
                {
                    PaymentLinkId = paymentLink.LinkId,
                    TenderId = paymentLink.TenderId,
                    TenderBidId = paymentLink.TenderBidId,
                    RazorpayPaymentId = request.RazorpayPaymentId,
                    RazorpayOrderId = request.RazorpayOrderId,
                    Amount = paymentLink.Amount,
                    Status = verificationResult.IsValid ? "Success" : "Failed",
                    PaymentMethod = request.PaymentMethod, // Get from frontend
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    TransactionDate = DateTime.UtcNow,
                    ErrorDescription = verificationResult.IsValid ? null : verificationResult.Message
                };

                // Store transaction
                await _transactionRepository.CreateAsync(transaction);

                if (verificationResult.IsValid && paymentLink != null && paymentLink.IsUsable)
                {
                    // Mark payment link as used
                    await _paymentLinkRepository.MarkAsUsedAsync(
                        paymentLink.Id,
                        null,
                        request.RazorpayPaymentId
                    );

                    // Update TenderBid if applicable
                    if (paymentLink.TenderBidId.HasValue)
                    {
                        var tenderBid = await _tenderBidRepository.GetBidByIdAsync(paymentLink.TenderBidId.Value);
                        if (tenderBid != null)
                        {
                            tenderBid.PaymentStatus = "Paid";
                            tenderBid.PaymentReference = request.RazorpayPaymentId;
                            tenderBid.PaymentDate = DateTime.UtcNow;
                            await _tenderBidRepository.UpdateBidAsync(tenderBid);
                        }
                    }

                    _logger.LogInformation("Payment verified successfully: {PaymentId}", request.RazorpayPaymentId);
                    return Json(new { success = true, message = "Payment verified successfully" });
                }

                return Json(new { success = false, message = verificationResult.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return Json(new { success = false, message = "Payment verification failed" });
            }
        }

        // POST: /Payment/Webhook
        [HttpPost]
        [Route("Payment/Webhook")]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                string json;
                using (var reader = new StreamReader(Request.Body))
                {
                    json = await reader.ReadToEndAsync();
                }

                string signature = Request.Headers["X-Razorpay-Signature"];

                if (!_razorpayService.VerifyWebhookSignature(json, signature))
                {
                    _logger.LogWarning("Invalid webhook signature");
                    return BadRequest("Invalid signature");
                }

                dynamic webhookData = JsonConvert.DeserializeObject(json);
                string eventType = webhookData.@event;

                // Extract payment details
                var payment = webhookData.payload.payment?.entity;
                string paymentId = payment?.id;
                string orderId = payment?.order_id;
                decimal amount = payment?.amount != null ? (decimal)payment.amount / 100 : 0;
                var notes = payment?.notes;
                string linkId = notes?.payment_link_id;

                // Store webhook event
                var webhookEvent = new WebhookEvent
                {
                    EventType = eventType,
                    RazorpayPaymentId = paymentId,
                    RazorpayOrderId = orderId,
                    PaymentLinkId = linkId,
                    Amount = amount,
                    Status = payment?.status,
                    ReceivedAt = DateTime.UtcNow
                };

                try
                {
                    // Process based on event type
                    switch (eventType)
                    {
                        case "payment.captured":
                            await HandlePaymentCaptured(webhookData, webhookEvent);
                            break;
                        case "payment.failed":
                            await HandlePaymentFailed(webhookData, webhookEvent);
                            break;
                        default:
                            _logger.LogInformation("Unhandled webhook event: {EventType}", eventType);
                            break;
                    }

                    webhookEvent.ProcessedSuccessfully = true;
                    webhookEvent.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception processEx)
                {
                    webhookEvent.ProcessedSuccessfully = false;
                    webhookEvent.ErrorMessage = processEx.Message;
                    _logger.LogError(processEx, "Error processing webhook event");
                }

                // Save webhook event
                await _webhookRepository.CreateAsync(webhookEvent);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500);
            }
        }

        private async Task HandlePaymentCaptured(dynamic webhookData, WebhookEvent webhookEvent)
        {
            var payment = webhookData.payload.payment.entity;
            string paymentId = payment.id;
            string linkId = payment.notes?.payment_link_id;

            // Check if transaction already exists
            var existingTransaction = await _transactionRepository.GetByPaymentIdAsync(paymentId);
            if (existingTransaction == null && !string.IsNullOrEmpty(linkId))
            {
                var paymentLink = await _paymentLinkRepository.GetByLinkIdAsync(linkId);
                if (paymentLink != null)
                {
                    // Create transaction from webhook
                    var transaction = new PaymentTransaction
                    {
                        PaymentLinkId = linkId,
                        TenderId = paymentLink.TenderId,
                        TenderBidId = paymentLink.TenderBidId,
                        RazorpayPaymentId = paymentId,
                        RazorpayOrderId = payment.order_id,
                        Amount = (decimal)payment.amount / 100,
                        Status = "Success",
                        PaymentMethod = payment.method,
                        BankName = payment.bank,
                        CardLast4 = payment.card?.last4,
                        UPIId = payment.vpa,
                        WalletName = payment.wallet,
                        CustomerEmail = payment.email,
                        CustomerPhone = payment.contact,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _transactionRepository.CreateAsync(transaction);

                    // Update payment link status
                    if (paymentLink.Status == PaymentLinkStatus.Active)
                    {
                        await _paymentLinkRepository.MarkAsUsedAsync(paymentLink.Id, null, paymentId);
                    }
                }
            }

            _logger.LogInformation("Payment captured webhook processed: {PaymentId}", paymentId);
        }

        private async Task HandlePaymentFailed(dynamic webhookData, WebhookEvent webhookEvent)
        {
            var payment = webhookData.payload.payment.entity;
            string paymentId = payment.id;
            string linkId = payment.notes?.payment_link_id;

            // Check if transaction already exists
            var existingTransaction = await _transactionRepository.GetByPaymentIdAsync(paymentId);
            if (existingTransaction == null && !string.IsNullOrEmpty(linkId))
            {
                var paymentLink = await _paymentLinkRepository.GetByLinkIdAsync(linkId);
                if (paymentLink != null)
                {
                    // Create failed transaction record
                    var transaction = new PaymentTransaction
                    {
                        PaymentLinkId = linkId,
                        TenderId = paymentLink.TenderId,
                        TenderBidId = paymentLink.TenderBidId,
                        RazorpayPaymentId = paymentId,
                        RazorpayOrderId = payment.order_id,
                        Amount = (decimal)payment.amount / 100,
                        Status = "Failed",
                        PaymentMethod = payment.method,
                        ErrorCode = payment.error_code,
                        ErrorDescription = payment.error_description,
                        CustomerEmail = payment.email,
                        CustomerPhone = payment.contact,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _transactionRepository.CreateAsync(transaction);
                }
            }

            //_logger.LogWarning("Payment failed webhook processed: {PaymentId}, Error: {Error}",
            //    paymentId, payment.error_description);
        }


        // GET: /Payment/Success
        [HttpGet]
        [Route("Payment/Success")]
        public IActionResult Success(string paymentId, string linkId)
        {
            ViewBag.PaymentId = paymentId;
            ViewBag.LinkId = linkId;
            return View();
        }

        // GET: /Payment/Failed
        [HttpGet]
        [Route("Payment/Failed")]
        public IActionResult Failed()
        {
            return View();
        }

        [HttpPost]
        [Route("Payment/LogFailedTransaction")]
        public async Task<IActionResult> LogFailedTransaction([FromBody] FailedTransactionRequest request)
        {
            try
            {
                var paymentLink = await _paymentLinkRepository.GetByIdAsync(request.PaymentLinkId);

                if (paymentLink != null)
                {
                    // Create failed transaction record
                    var transaction = new PaymentTransaction
                    {
                        PaymentLinkId = paymentLink.LinkId,
                        TenderId = paymentLink.TenderId,
                        TenderBidId = paymentLink.TenderBidId,
                        Amount = paymentLink.Amount,
                        Status = "Failed",
                        ErrorCode = request.ErrorCode,
                        ErrorDescription = request.ErrorDescription,
                        CustomerEmail = request.CustomerEmail,
                        CustomerPhone = request.CustomerPhone,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _transactionRepository.CreateAsync(transaction);

                    _logger.LogWarning("Failed transaction logged for PaymentLinkId: {LinkId}, Error: {Error}",
                        paymentLink.LinkId, request.ErrorDescription);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging failed transaction");
                return StatusCode(500);
            }
        }

    }

    // View Models
    public class PaymentViewModel
    {
        public int PaymentLinkId { get; set; }
        public string LinkId { get; set; }
        public int TenderId { get; set; }
        public int? TenderBidId { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }

        // Bidder Details
        public string BidderName { get; set; }
        public string BidderEmail { get; set; }
        public string BidderPhone { get; set; }
        public string CompanyName { get; set; }

        // Razorpay Details
        public string RazorpayOrderId { get; set; }
        public string RazorpayKey { get; set; }
        public int RazorpayAmount { get; set; }

        // Additional Info
        public DateTime ExpiryDate { get; set; }
        public string TenderTitle { get; set; }

        // Display helpers
        public string AmountDisplay => Amount.ToString("C", new System.Globalization.CultureInfo("en-IN"));
        public string PaymentTypeDisplay => PaymentType.GetDisplayName();
    }

    public class PaymentVerificationRequest
    {
        public int PaymentLinkId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
        public string PaymentMethod { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
    }


   
    public class FailedTransactionRequest
    {
        public int PaymentLinkId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorSource { get; set; }
        public string ErrorStep { get; set; }
        public string ErrorReason { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
    }
}
