using BiddingSystem.Models;
using BiddingSystem.ViewModels;

namespace BiddingSystem.Data
{
    public interface IRefundService
    {
        // RefundRequest operations
        Task<RefundRequestViewModel?> GetRefundRequestByIdAsync(int id);
        Task<RefundRequestViewModel?> GetRefundRequestByRefundIdAsync(string refundId);
        Task<RefundRequestListViewModel> GetRefundRequestsAsync(
            string searchTerm = "", 
            RefundStatus? status = null, 
            RefundType? type = null, 
            RefundReason? reason = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            int page = 1, 
            int pageSize = 25);
        
        Task<RefundRequestListViewModel> GetRefundRequestsByUserAsync(string requestedBy, int page = 1, int pageSize = 25);
        Task<RefundRequestListViewModel> GetRefundRequestsByTenderBidAsync(int tenderBidId);
        
        // CRUD operations
        Task<RefundRequestViewModel> CreateRefundRequestAsync(RefundRequestCreateViewModel model, string requestedBy);
        Task<RefundRequestViewModel> UpdateRefundRequestAsync(RefundRequestEditViewModel model, int updatedBy);
        Task<bool> DeleteRefundRequestAsync(int id);
        
        // Status management
        Task<bool> ApproveRefundRequestAsync(int id, decimal approvedAmount, int processedBy, string? remarks = null);
        Task<bool> RejectRefundRequestAsync(int id, int processedBy, string? remarks = null);
        Task<bool> ProcessRefundRequestAsync(int id, int processedBy, string? refundReference = null);
        Task<bool> CompleteRefundRequestAsync(int id, int processedBy, string? remarks = null);
        Task<bool> FailRefundRequestAsync(int id, int processedBy, string? failureReason = null);
        
        // Validation
        Task<bool> CanCreateRefundRequestAsync(int tenderBidId, int paymentLinkId);
        Task<bool> IsRefundRequestEligibleAsync(int tenderBidId);
        Task<RefundValidationResult> ValidateRefundRequestAsync(RefundRequestCreateViewModel model);
        
        // Statistics and reporting
        Task<object> GetRefundStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<Dictionary<RefundStatus, int>> GetStatusCountsAsync();
        Task<Dictionary<RefundType, int>> GetTypeCountsAsync();
        Task<Dictionary<RefundReason, int>> GetReasonCountsAsync();
        Task<decimal> GetTotalRefundedAmountAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetPendingRefundAmountAsync();
        
        // Dashboard data
        Task<IEnumerable<RefundRequestViewModel>> GetRecentRefundRequestsAsync(int count = 10);
        Task<IEnumerable<RefundRequestViewModel>> GetRefundRequestsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        
        // RefundTransaction operations
        Task<RefundTransaction> CreateRefundTransactionAsync(int refundRequestId, decimal amount, string? createdBy = null);
        Task<bool> UpdateTransactionStatusAsync(int transactionId, RefundTransactionStatus status, string? bankResponse = null, string? failureReason = null);
        Task<IEnumerable<RefundTransaction>> GetRefundTransactionsByRequestIdAsync(int refundRequestId);
        
        // Email notifications
        Task<bool> SendRefundRequestNotificationAsync(int refundRequestId);
        Task<bool> SendRefundStatusUpdateNotificationAsync(int refundRequestId, RefundStatus newStatus);
        Task<bool> SendRefundApprovalNotificationAsync(int refundRequestId);
        Task<bool> SendRefundRejectionNotificationAsync(int refundRequestId, string? reason = null);
        Task<bool> SendRefundCompletionNotificationAsync(int refundRequestId);
        
        // Integration with existing systems
        Task<bool> UpdateTenderBidPaymentStatusAsync(int tenderBidId, string newStatus);
        Task<bool> UpdatePaymentLinkStatusAsync(int paymentLinkId, PaymentLinkStatus newStatus);
        Task<bool> CreateEMDSDTransactionAsync(int refundRequestId, decimal amount, string transactionType = "Refund");
        
        // Audit and logging
        Task LogRefundActionAsync(int refundRequestId, string action, int userId, string? details = null);
        Task<IEnumerable<object>> GetRefundAuditLogAsync(int refundRequestId);
    }

    public class RefundValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public static RefundValidationResult Success() => new() { IsValid = true };
        public static RefundValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
    }
}
