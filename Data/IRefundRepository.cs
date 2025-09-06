using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface IRefundRepository
    {
        // RefundRequest CRUD operations
        Task<RefundRequest?> GetRefundRequestByIdAsync(int id);
        Task<RefundRequest?> GetRefundRequestByRefundIdAsync(string refundId);
        Task<IEnumerable<RefundRequest>> GetAllRefundRequestsAsync();
        Task<IEnumerable<RefundRequest>> GetRefundRequestsByTenderBidIdAsync(int tenderBidId);
        Task<IEnumerable<RefundRequest>> GetRefundRequestsByPaymentLinkIdAsync(int paymentLinkId);
        Task<IEnumerable<RefundRequest>> GetRefundRequestsByStatusAsync(RefundStatus status);
        Task<IEnumerable<RefundRequest>> GetRefundRequestsByUserAsync(string requestedBy);
        
        // Search and filtering
        Task<IEnumerable<RefundRequest>> SearchRefundRequestsAsync(
            string searchTerm = "", 
            RefundStatus? status = null, 
            RefundType? type = null, 
            RefundReason? reason = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            int page = 1, 
            int pageSize = 25);
        
        Task<int> GetSearchCountAsync(
            string searchTerm = "", 
            RefundStatus? status = null, 
            RefundType? type = null, 
            RefundReason? reason = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null);
        
        // Statistics and counts
        Task<Dictionary<RefundStatus, int>> GetStatusCountsAsync();
        Task<Dictionary<RefundType, int>> GetTypeCountsAsync();
        Task<Dictionary<RefundReason, int>> GetReasonCountsAsync();
        Task<decimal> GetTotalRefundedAmountAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetPendingRefundAmountAsync();
        
        // CRUD operations
        Task<RefundRequest> CreateRefundRequestAsync(RefundRequest refundRequest);
        Task<bool> UpdateRefundRequestAsync(RefundRequest refundRequest);
        Task<bool> DeleteRefundRequestAsync(int id);
        
        // Status updates
        Task<bool> UpdateRefundStatusAsync(int id, RefundStatus status, int? processedBy = null, string? remarks = null);
        Task<bool> ApproveRefundAsync(int id, decimal approvedAmount, int processedBy, string? remarks = null);
        Task<bool> RejectRefundAsync(int id, int processedBy, string? remarks = null);
        Task<bool> ProcessRefundAsync(int id, int processedBy, string? refundReference = null);
        
        // RefundTransaction operations
        Task<RefundTransaction?> GetRefundTransactionByIdAsync(int id);
        Task<IEnumerable<RefundTransaction>> GetRefundTransactionsByRequestIdAsync(int refundRequestId);
        Task<RefundTransaction> CreateRefundTransactionAsync(RefundTransaction transaction);
        Task<bool> UpdateRefundTransactionAsync(RefundTransaction transaction);
        Task<bool> UpdateTransactionStatusAsync(int id, RefundTransactionStatus status, string? bankResponse = null, string? failureReason = null);
        
        // Validation methods
        Task<bool> CanCreateRefundRequestAsync(int tenderBidId, int paymentLinkId);
        Task<bool> RefundRequestExistsAsync(int tenderBidId, int paymentLinkId);
        Task<bool> IsRefundRequestEligibleAsync(int tenderBidId);
        
        // Dashboard and reporting
        Task<IEnumerable<RefundRequest>> GetRecentRefundRequestsAsync(int count = 10);
        Task<IEnumerable<RefundRequest>> GetRefundRequestsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<object> GetRefundStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}
