using BiddingSystem.Models;
using BiddingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiddingSystem.Data
{
    public class RefundService : IRefundService
    {
        private readonly IRefundRepository _refundRepository;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IPaymentLinkRepository _paymentLinkRepository;
        private readonly IEMDSDRepository _emdSdRepository;
        private readonly IEmailService _emailService;
        private readonly ISecurityAuditService _securityAudit;
        private readonly ILogger<RefundService> _logger;

        public RefundService(
            IRefundRepository refundRepository,
            ITenderBidRepository tenderBidRepository,
            IPaymentLinkRepository paymentLinkRepository,
            IEMDSDRepository emdSdRepository,
            IEmailService emailService,
            ISecurityAuditService securityAudit,
            ILogger<RefundService> logger)
        {
            _refundRepository = refundRepository;
            _tenderBidRepository = tenderBidRepository;
            _paymentLinkRepository = paymentLinkRepository;
            _emdSdRepository = emdSdRepository;
            _emailService = emailService;
            _securityAudit = securityAudit;
            _logger = logger;
        }

        #region RefundRequest Operations

        public async Task<RefundRequestViewModel?> GetRefundRequestByIdAsync(int id)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(id);
                if (refundRequest == null) return null;

                var transactions = await _refundRepository.GetRefundTransactionsByRequestIdAsync(id);
                refundRequest.Transactions = transactions.ToList();

                return MapToViewModel(refundRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request by ID: {Id}", id);
                throw;
            }
        }

        public async Task<RefundRequestViewModel?> GetRefundRequestByRefundIdAsync(string refundId)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByRefundIdAsync(refundId);
                if (refundRequest == null) return null;

                var transactions = await _refundRepository.GetRefundTransactionsByRequestIdAsync(refundRequest.Id);
                refundRequest.Transactions = transactions.ToList();

                return MapToViewModel(refundRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request by RefundId: {RefundId}", refundId);
                throw;
            }
        }

        public async Task<RefundRequestListViewModel> GetRefundRequestsAsync(
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
                var refundRequests = await _refundRepository.SearchRefundRequestsAsync(
                    searchTerm, status, type, reason, fromDate, toDate, page, pageSize);
                
                var totalCount = await _refundRepository.GetSearchCountAsync(
                    searchTerm, status, type, reason, fromDate, toDate);

                var statusCounts = await _refundRepository.GetStatusCountsAsync();
                var typeCounts = await _refundRepository.GetTypeCountsAsync();

                return new RefundRequestListViewModel
                {
                    RefundRequests = refundRequests.Select(MapToViewModel).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    Status = status,
                    Type = type,
                    Reason = reason,
                    FromDate = fromDate,
                    ToDate = toDate,
                    StatusCounts = statusCounts,
                    TypeCounts = typeCounts,
                    AvailableStatuses = Enum.GetValues<RefundStatus>().Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.GetDisplayName()
                    }).ToList(),
                    AvailableTypes = Enum.GetValues<RefundType>().Select(t => new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = t.GetDisplayName()
                    }).ToList(),
                    AvailableReasons = Enum.GetValues<RefundReason>().Select(r => new SelectListItem
                    {
                        Value = ((int)r).ToString(),
                        Text = r.GetDisplayName()
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund requests");
                throw;
            }
        }

        public async Task<RefundRequestListViewModel> GetRefundRequestsByUserAsync(string requestedBy, int page = 1, int pageSize = 25)
        {
            try
            {
                var refundRequests = await _refundRepository.GetRefundRequestsByUserAsync(requestedBy);
                var totalCount = refundRequests.Count();

                return new RefundRequestListViewModel
                {
                    RefundRequests = refundRequests.Select(MapToViewModel).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    AvailableStatuses = Enum.GetValues<RefundStatus>().Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.GetDisplayName()
                    }).ToList(),
                    AvailableTypes = Enum.GetValues<RefundType>().Select(t => new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = t.GetDisplayName()
                    }).ToList(),
                    AvailableReasons = Enum.GetValues<RefundReason>().Select(r => new SelectListItem
                    {
                        Value = ((int)r).ToString(),
                        Text = r.GetDisplayName()
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund requests by user: {RequestedBy}", requestedBy);
                throw;
            }
        }

        public async Task<RefundRequestListViewModel> GetRefundRequestsByTenderBidAsync(int tenderBidId)
        {
            try
            {
                var refundRequests = await _refundRepository.GetRefundRequestsByTenderBidIdAsync(tenderBidId);
                var totalCount = refundRequests.Count();

                return new RefundRequestListViewModel
                {
                    RefundRequests = refundRequests.Select(MapToViewModel).ToList(),
                    TotalCount = totalCount,
                    Page = 1,
                    PageSize = totalCount,
                    AvailableStatuses = Enum.GetValues<RefundStatus>().Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.GetDisplayName()
                    }).ToList(),
                    AvailableTypes = Enum.GetValues<RefundType>().Select(t => new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = t.GetDisplayName()
                    }).ToList(),
                    AvailableReasons = Enum.GetValues<RefundReason>().Select(r => new SelectListItem
                    {
                        Value = ((int)r).ToString(),
                        Text = r.GetDisplayName()
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund requests by tender bid: {TenderBidId}", tenderBidId);
                throw;
            }
        }

        #endregion

        #region CRUD Operations

        public async Task<RefundRequestViewModel> CreateRefundRequestAsync(RefundRequestCreateViewModel model, string requestedBy)
        {
            try
            {
                // Validate the request
                var validation = await ValidateRefundRequestAsync(model);
                if (!validation.IsValid)
                {
                    throw new InvalidOperationException($"Validation failed: {string.Join(", ", validation.Errors)}");
                }

                // Check if refund request already exists
                if (!await _refundRepository.CanCreateRefundRequestAsync(model.TenderBidId, model.PaymentLinkId))
                {
                    throw new InvalidOperationException("A refund request already exists for this tender bid and payment link.");
                }

                // Create the refund request
                var refundRequest = new RefundRequest
                {
                    RefundId = RefundRequest.GenerateRefundId(),
                    TenderBidId = model.TenderBidId,
                    PaymentLinkId = model.PaymentLinkId,
                    Type = model.Type,
                    RequestedAmount = model.RequestedAmount,
                    Reason = model.Reason,
                    Status = RefundStatus.Pending,
                    RequestedBy = requestedBy,
                    Remarks = model.Remarks,
                    BankDetails = model.BankDetails
                };

                var createdRefund = await _refundRepository.CreateRefundRequestAsync(refundRequest);

                // Log the action
                await LogRefundActionAsync(createdRefund.Id, "Created", 0, $"Refund request created by {requestedBy}");

                // Send notification
                await SendRefundRequestNotificationAsync(createdRefund.Id);

                return MapToViewModel(createdRefund);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund request for tender bid: {TenderBidId}", model.TenderBidId);
                throw;
            }
        }

        public async Task<RefundRequestViewModel> UpdateRefundRequestAsync(RefundRequestEditViewModel model, int updatedBy)
        {
            try
            {
                var existingRefund = await _refundRepository.GetRefundRequestByIdAsync(model.Id);
                if (existingRefund == null)
                {
                    throw new InvalidOperationException("Refund request not found.");
                }

                // Update the refund request
                existingRefund.Type = model.Type;
                existingRefund.RequestedAmount = model.RequestedAmount;
                existingRefund.ApprovedAmount = model.ApprovedAmount;
                existingRefund.Reason = model.Reason;
                existingRefund.Status = model.Status;
                existingRefund.Remarks = model.Remarks;
                existingRefund.BankDetails = model.BankDetails;
                existingRefund.RefundReference = model.RefundReference;

                var updated = await _refundRepository.UpdateRefundRequestAsync(existingRefund);

                if (updated)
                {
                    // Log the action
                    await LogRefundActionAsync(model.Id, "Updated", updatedBy, "Refund request updated");
                }

                return MapToViewModel(existingRefund);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund request: {Id}", model.Id);
                throw;
            }
        }

        public async Task<bool> DeleteRefundRequestAsync(int id)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    return false;
                }

                // Only allow deletion of pending refund requests
                if (refundRequest.Status != RefundStatus.Pending)
                {
                    throw new InvalidOperationException("Only pending refund requests can be deleted.");
                }

                var deleted = await _refundRepository.DeleteRefundRequestAsync(id);

                if (deleted)
                {
                    // Log the action
                    await LogRefundActionAsync(id, "Deleted", 0, "Refund request deleted");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting refund request: {Id}", id);
                throw;
            }
        }

        #endregion

        #region Status Management

        public async Task<bool> ApproveRefundRequestAsync(int id, decimal approvedAmount, int processedBy, string? remarks = null)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    return false;
                }

                if (refundRequest.Status != RefundStatus.Pending)
                {
                    throw new InvalidOperationException("Only pending refund requests can be approved.");
                }

                var approved = await _refundRepository.ApproveRefundAsync(id, approvedAmount, processedBy, remarks);

                if (approved)
                {
                    // Log the action
                    await LogRefundActionAsync(id, "Approved", processedBy, $"Approved amount: {approvedAmount:C}");

                    // Send notification
                    await SendRefundApprovalNotificationAsync(id);
                }

                return approved;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving refund request: {Id}", id);
                throw;
            }
        }

        public async Task<bool> RejectRefundRequestAsync(int id, int processedBy, string? remarks = null)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    return false;
                }

                if (refundRequest.Status != RefundStatus.Pending)
                {
                    throw new InvalidOperationException("Only pending refund requests can be rejected.");
                }

                var rejected = await _refundRepository.RejectRefundAsync(id, processedBy, remarks);

                if (rejected)
                {
                    // Log the action
                    await LogRefundActionAsync(id, "Rejected", processedBy, remarks ?? "No reason provided");

                    // Send notification
                    await SendRefundRejectionNotificationAsync(id, remarks);
                }

                return rejected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting refund request: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ProcessRefundRequestAsync(int id, int processedBy, string? refundReference = null)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    return false;
                }

                if (refundRequest.Status != RefundStatus.Approved)
                {
                    throw new InvalidOperationException("Only approved refund requests can be processed.");
                }

                var processed = await _refundRepository.ProcessRefundAsync(id, processedBy, refundReference);

                if (processed)
                {
                    // Create refund transaction
                    await CreateRefundTransactionAsync(id, refundRequest.ApprovedAmount ?? refundRequest.RequestedAmount, processedBy.ToString());

                    // Log the action
                    await LogRefundActionAsync(id, "Processing", processedBy, $"Refund processing initiated");

                    // Send notification
                    await SendRefundStatusUpdateNotificationAsync(id, RefundStatus.Processing);
                }

                return processed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund request: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CompleteRefundRequestAsync(int id, int processedBy, string? remarks = null)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    return false;
                }

                if (refundRequest.Status != RefundStatus.Processing)
                {
                    throw new InvalidOperationException("Only processing refund requests can be completed.");
                }

                var completed = await _refundRepository.UpdateRefundStatusAsync(id, RefundStatus.Completed, processedBy, remarks);

                if (completed)
                {
                    // Update tender bid payment status
                    await UpdateTenderBidPaymentStatusAsync(refundRequest.TenderBidId, "Refunded");

                    // Update payment link status
                    await UpdatePaymentLinkStatusAsync(refundRequest.PaymentLinkId, PaymentLinkStatus.Cancelled);

                    // Create EMDSD transaction if applicable
                    if (refundRequest.Type == RefundType.EMD || refundRequest.Type == RefundType.SD)
                    {
                        await CreateEMDSDTransactionAsync(id, refundRequest.ApprovedAmount ?? refundRequest.RequestedAmount, "Refund");
                    }

                    // Log the action
                    await LogRefundActionAsync(id, "Completed", processedBy, remarks ?? "Refund completed successfully");

                    // Send notification
                    await SendRefundCompletionNotificationAsync(id);
                }

                return completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing refund request: {Id}", id);
                throw;
            }
        }

        public async Task<bool> FailRefundRequestAsync(int id, int processedBy, string? failureReason = null)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(id);
                if (refundRequest == null)
                {
                    return false;
                }

                if (refundRequest.Status != RefundStatus.Processing)
                {
                    throw new InvalidOperationException("Only processing refund requests can be marked as failed.");
                }

                var failed = await _refundRepository.UpdateRefundStatusAsync(id, RefundStatus.Failed, processedBy, failureReason);

                if (failed)
                {
                    // Log the action
                    await LogRefundActionAsync(id, "Failed", processedBy, failureReason ?? "Refund processing failed");

                    // Send notification
                    await SendRefundStatusUpdateNotificationAsync(id, RefundStatus.Failed);
                }

                return failed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error failing refund request: {Id}", id);
                throw;
            }
        }

        #endregion

        #region Validation

        public async Task<bool> CanCreateRefundRequestAsync(int tenderBidId, int paymentLinkId)
        {
            return await _refundRepository.CanCreateRefundRequestAsync(tenderBidId, paymentLinkId);
        }

        public async Task<bool> IsRefundRequestEligibleAsync(int tenderBidId)
        {
            return await _refundRepository.IsRefundRequestEligibleAsync(tenderBidId);
        }

        public async Task<RefundValidationResult> ValidateRefundRequestAsync(RefundRequestCreateViewModel model)
        {
            var result = new RefundValidationResult { IsValid = true };

            // Validate tender bid exists and is eligible
            var tenderBid = await _tenderBidRepository.GetBidByIdAsync(model.TenderBidId);
            if (tenderBid == null)
            {
                result.Errors.Add("Tender bid not found.");
                result.IsValid = false;
            }
            else if (tenderBid.PaymentStatus != "Paid")
            {
                result.Errors.Add("Refund can only be requested for paid bids.");
                result.IsValid = false;
            }

            // Validate payment link exists
            var paymentLink = await _paymentLinkRepository.GetByIdAsync(model.PaymentLinkId);
            if (paymentLink == null)
            {
                result.Errors.Add("Payment link not found.");
                result.IsValid = false;
            }

            // Validate amount
            if (model.RequestedAmount <= 0)
            {
                result.Errors.Add("Requested amount must be greater than 0.");
                result.IsValid = false;
            }

            if (tenderBid != null && model.RequestedAmount > tenderBid.TotalAmount)
            {
                result.Errors.Add("Requested amount cannot exceed the total bid amount.");
                result.IsValid = false;
            }

            // Validate refund type and amount combination
            if (model.Type == RefundType.ProcessingFee && tenderBid != null)
            {
                if (model.RequestedAmount > tenderBid.ProcessingFee)
                {
                    result.Errors.Add("Processing fee refund amount cannot exceed the original processing fee.");
                    result.IsValid = false;
                }
            }

            return result;
        }

        #endregion

        #region Statistics and Reporting

        public async Task<object> GetRefundStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _refundRepository.GetRefundStatisticsAsync(fromDate, toDate);
        }

        public async Task<Dictionary<RefundStatus, int>> GetStatusCountsAsync()
        {
            return await _refundRepository.GetStatusCountsAsync();
        }

        public async Task<Dictionary<RefundType, int>> GetTypeCountsAsync()
        {
            return await _refundRepository.GetTypeCountsAsync();
        }

        public async Task<Dictionary<RefundReason, int>> GetReasonCountsAsync()
        {
            return await _refundRepository.GetReasonCountsAsync();
        }

        public async Task<decimal> GetTotalRefundedAmountAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _refundRepository.GetTotalRefundedAmountAsync(fromDate, toDate);
        }

        public async Task<decimal> GetPendingRefundAmountAsync()
        {
            return await _refundRepository.GetPendingRefundAmountAsync();
        }

        #endregion

        #region Dashboard Data

        public async Task<IEnumerable<RefundRequestViewModel>> GetRecentRefundRequestsAsync(int count = 10)
        {
            var refundRequests = await _refundRepository.GetRecentRefundRequestsAsync(count);
            return refundRequests.Select(MapToViewModel);
        }

        public async Task<IEnumerable<RefundRequestViewModel>> GetRefundRequestsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var refundRequests = await _refundRepository.GetRefundRequestsByDateRangeAsync(fromDate, toDate);
            return refundRequests.Select(MapToViewModel);
        }

        #endregion

        #region RefundTransaction Operations

        public async Task<RefundTransaction> CreateRefundTransactionAsync(int refundRequestId, decimal amount, string? createdBy = null)
        {
            var transaction = new RefundTransaction
            {
                RefundRequestId = refundRequestId,
                TransactionReference = RefundTransaction.GenerateTransactionReference(),
                Amount = amount,
                Status = RefundTransactionStatus.Pending,
                ProcessedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            return await _refundRepository.CreateRefundTransactionAsync(transaction);
        }

        public async Task<bool> UpdateTransactionStatusAsync(int transactionId, RefundTransactionStatus status, string? bankResponse = null, string? failureReason = null)
        {
            return await _refundRepository.UpdateTransactionStatusAsync(transactionId, status, bankResponse, failureReason);
        }

        public async Task<IEnumerable<RefundTransaction>> GetRefundTransactionsByRequestIdAsync(int refundRequestId)
        {
            return await _refundRepository.GetRefundTransactionsByRequestIdAsync(refundRequestId);
        }

        #endregion

        #region Email Notifications

        public async Task<bool> SendRefundRequestNotificationAsync(int refundRequestId)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(refundRequestId);
                if (refundRequest == null) return false;

                var subject = $"Refund Request Submitted - {refundRequest.RefundId}";
                var body = $@"
                    <h2>Refund Request Submitted</h2>
                    <p>A new refund request has been submitted with the following details:</p>
                    <ul>
                        <li><strong>Refund ID:</strong> {refundRequest.RefundId}</li>
                        <li><strong>Requested By:</strong> {refundRequest.RequestedBy}</li>
                        <li><strong>Amount:</strong> {refundRequest.RequestedAmountDisplay}</li>
                        <li><strong>Type:</strong> {refundRequest.TypeDisplayName}</li>
                        <li><strong>Reason:</strong> {refundRequest.ReasonDisplayName}</li>
                        <li><strong>Requested At:</strong> {refundRequest.RequestedAt:dd MMM yyyy HH:mm}</li>
                    </ul>
                    <p>Please review and process this refund request.</p>";

                return await _emailService.SendCustomEmailAsync("admin@biddingsystem.com", subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund request notification for ID: {RefundRequestId}", refundRequestId);
                return false;
            }
        }

        public async Task<bool> SendRefundStatusUpdateNotificationAsync(int refundRequestId, RefundStatus newStatus)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(refundRequestId);
                if (refundRequest == null) return false;

                var subject = $"Refund Status Update - {refundRequest.RefundId}";
                var body = $@"
                    <h2>Refund Status Update</h2>
                    <p>Your refund request status has been updated:</p>
                    <ul>
                        <li><strong>Refund ID:</strong> {refundRequest.RefundId}</li>
                        <li><strong>New Status:</strong> {newStatus.GetDisplayName()}</li>
                        <li><strong>Amount:</strong> {refundRequest.RequestedAmountDisplay}</li>
                        <li><strong>Updated At:</strong> {DateTime.UtcNow:dd MMM yyyy HH:mm}</li>
                    </ul>";

                return await _emailService.SendCustomEmailAsync(refundRequest.RequestedBy, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund status update notification for ID: {RefundRequestId}", refundRequestId);
                return false;
            }
        }

        public async Task<bool> SendRefundApprovalNotificationAsync(int refundRequestId)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(refundRequestId);
                if (refundRequest == null) return false;

                var subject = $"Refund Approved - {refundRequest.RefundId}";
                var body = $@"
                    <h2>Refund Request Approved</h2>
                    <p>Your refund request has been approved:</p>
                    <ul>
                        <li><strong>Refund ID:</strong> {refundRequest.RefundId}</li>
                        <li><strong>Approved Amount:</strong> {refundRequest.ApprovedAmountDisplay}</li>
                        <li><strong>Status:</strong> {refundRequest.StatusDisplayName}</li>
                        <li><strong>Approved At:</strong> {refundRequest.ProcessedAt:dd MMM yyyy HH:mm}</li>
                    </ul>
                    <p>The refund will be processed shortly.</p>";

                return await _emailService.SendCustomEmailAsync(refundRequest.RequestedBy, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund approval notification for ID: {RefundRequestId}", refundRequestId);
                return false;
            }
        }

        public async Task<bool> SendRefundRejectionNotificationAsync(int refundRequestId, string? reason = null)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(refundRequestId);
                if (refundRequest == null) return false;

                var subject = $"Refund Request Rejected - {refundRequest.RefundId}";
                var body = $@"
                    <h2>Refund Request Rejected</h2>
                    <p>Your refund request has been rejected:</p>
                    <ul>
                        <li><strong>Refund ID:</strong> {refundRequest.RefundId}</li>
                        <li><strong>Requested Amount:</strong> {refundRequest.RequestedAmountDisplay}</li>
                        <li><strong>Status:</strong> {refundRequest.StatusDisplayName}</li>
                        <li><strong>Rejected At:</strong> {refundRequest.ProcessedAt:dd MMM yyyy HH:mm}</li>
                    </ul>
                    {(string.IsNullOrEmpty(reason) ? "" : $"<p><strong>Reason:</strong> {reason}</p>")}
                    <p>If you have any questions, please contact our support team.</p>";

                return await _emailService.SendCustomEmailAsync(refundRequest.RequestedBy, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund rejection notification for ID: {RefundRequestId}", refundRequestId);
                return false;
            }
        }

        public async Task<bool> SendRefundCompletionNotificationAsync(int refundRequestId)
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(refundRequestId);
                if (refundRequest == null) return false;

                var subject = $"Refund Completed - {refundRequest.RefundId}";
                var body = $@"
                    <h2>Refund Completed</h2>
                    <p>Your refund has been successfully processed:</p>
                    <ul>
                        <li><strong>Refund ID:</strong> {refundRequest.RefundId}</li>
                        <li><strong>Refunded Amount:</strong> {refundRequest.ApprovedAmountDisplay}</li>
                        <li><strong>Status:</strong> {refundRequest.StatusDisplayName}</li>
                        <li><strong>Completed At:</strong> {refundRequest.ProcessedAt:dd MMM yyyy HH:mm}</li>
                    </ul>
                    <p>The refund amount has been credited to your account. Please allow 2-3 business days for the amount to reflect in your bank account.</p>";

                return await _emailService.SendCustomEmailAsync(refundRequest.RequestedBy, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund completion notification for ID: {RefundRequestId}", refundRequestId);
                return false;
            }
        }

        #endregion

        #region Integration with Existing Systems

        public async Task<bool> UpdateTenderBidPaymentStatusAsync(int tenderBidId, string newStatus)
        {
            try
            {
                var tenderBid = await _tenderBidRepository.GetBidByIdAsync(tenderBidId);
                if (tenderBid == null) return false;

                tenderBid.PaymentStatus = newStatus;
                tenderBid.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _tenderBidRepository.UpdateBidAsync(tenderBid);
                return updateResult != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tender bid payment status: {TenderBidId}", tenderBidId);
                return false;
            }
        }

        public async Task<bool> UpdatePaymentLinkStatusAsync(int paymentLinkId, PaymentLinkStatus newStatus)
        {
            try
            {
                var paymentLink = await _paymentLinkRepository.GetByIdAsync(paymentLinkId);
                if (paymentLink == null) return false;

                paymentLink.Status = newStatus;

                var updateResult = await _paymentLinkRepository.UpdateAsync(paymentLink);
                return updateResult != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment link status: {PaymentLinkId}", paymentLinkId);
                return false;
            }
        }

        public async Task<bool> CreateEMDSDTransactionAsync(int refundRequestId, decimal amount, string transactionType = "Refund")
        {
            try
            {
                var refundRequest = await _refundRepository.GetRefundRequestByIdAsync(refundRequestId);
                if (refundRequest == null) return false;

                // This would integrate with the EMDSD system
                // Implementation depends on the EMDSD transaction structure
                _logger.LogInformation("Creating EMDSD transaction for refund: {RefundRequestId}, Amount: {Amount}", refundRequestId, amount);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating EMDSD transaction for refund: {RefundRequestId}", refundRequestId);
                return false;
            }
        }

        #endregion

        #region Audit and Logging

        public async Task LogRefundActionAsync(int refundRequestId, string action, int userId, string? details = null)
        {
            try
            {
                await _securityAudit.LogUserActionAsync(new UserAction
                {
                    UserId = userId,
                    Action = $"Refund_{action}",
                    EntityType = "RefundRequest",
                    EntityId = refundRequestId,
                    NewValues = details,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging refund action: {Action} for refund: {RefundRequestId}", action, refundRequestId);
            }
        }

        public async Task<bool> UpdateRefundRequestAsync(int id, RefundRequest refundRequest, int userId)
        {
            try
            {
                var existingRequest = await _refundRepository.GetRefundRequestByIdAsync(id);
                if (existingRequest == null)
                    return false;

                // Update the refund request
                refundRequest.Id = id;
                refundRequest.UpdatedAt = DateTime.UtcNow;
                
                var result = await _refundRepository.UpdateRefundRequestAsync(refundRequest);
                
                if (result)
                {
                    await LogRefundActionAsync(id, "Updated", userId, $"Updated refund request details");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund request: {Id}", id);
                throw;
            }
        }


        public async Task<IEnumerable<object>> GetRefundAuditLogAsync(int refundRequestId)
        {
            try
            {
                // This would retrieve audit logs for the refund request
                // Implementation depends on the audit system structure
                return new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund audit log: {RefundRequestId}", refundRequestId);
                return new List<object>();
            }
        }

        #endregion

        #region Helper Methods

        private RefundRequestViewModel MapToViewModel(RefundRequest refundRequest)
        {
            return new RefundRequestViewModel
            {
                Id = refundRequest.Id,
                RefundId = refundRequest.RefundId,
                TenderBidId = refundRequest.TenderBidId,
                PaymentLinkId = refundRequest.PaymentLinkId,
                Type = refundRequest.Type,
                RequestedAmount = refundRequest.RequestedAmount,
                ApprovedAmount = refundRequest.ApprovedAmount,
                Reason = refundRequest.Reason,
                Status = refundRequest.Status,
                RequestedBy = refundRequest.RequestedBy,
                ProcessedBy = refundRequest.ProcessedBy,
                RequestedAt = refundRequest.RequestedAt,
                ProcessedAt = refundRequest.ProcessedAt,
                Remarks = refundRequest.Remarks,
                BankDetails = refundRequest.BankDetails,
                RefundReference = refundRequest.RefundReference,
                TenderBid = refundRequest.TenderBid,
                PaymentLink = refundRequest.PaymentLink,
                ProcessedByUser = refundRequest.ProcessedByUser,
                Transactions = refundRequest.Transactions.ToList()
            };
        }

        private RefundTransactionViewModel MapToTransactionViewModel(RefundTransaction transaction)
        {
            return new RefundTransactionViewModel
            {
                Id = transaction.Id,
                RefundRequestId = transaction.RefundRequestId,
                TransactionReference = transaction.TransactionReference,
                Amount = transaction.Amount,
                Status = transaction.Status,
                ProcessedAt = transaction.ProcessedAt,
                BankResponse = transaction.BankResponse,
                FailureReason = transaction.FailureReason,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                CreatedBy = transaction.CreatedBy
            };
        }

        #endregion
    }
}
