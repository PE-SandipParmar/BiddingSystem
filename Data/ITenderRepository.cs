using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface ITenderRepository
    {
        // Basic CRUD operations
        Task<Tender?> GetByIdAsync(int id);
        Task<Tender?> GetByTenderIdAsync(string tenderId);
        Task<List<Tender>> GetAllAsync();
        Task<int> CreateAsync(Tender tender);
        Task<bool> UpdateAsync(Tender tender);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string tenderId, int? excludeId = null);

        // Tender status operations
        Task<bool> PublishTenderAsync(int id);
        Task<bool> CloseTenderAsync(int id);
        Task<bool> CancelTenderAsync(int id);
        Task<bool> UpdateStatusAsync(int id, TenderStatus status);

        // Search and filtering
        Task<List<Tender>> SearchTendersAsync(string searchTerm, TenderStatus? status = null, string? department = null);
        Task<(List<Tender> Tenders, int TotalCount)> GetPagedTendersAsync(int page, int pageSize, string? searchTerm = null, TenderStatus? status = null, string? department = null);

        // Statistics and analytics
        Task<int> GetTotalTendersCountAsync();
        Task<int> GetActiveTendersCountAsync();
        Task<Dictionary<TenderStatus, int>> GetTenderCountByStatusAsync();
        Task<List<Tender>> GetRecentTendersAsync(int days = 7, int limit = 10);
        Task<List<Tender>> GetTendersByDepartmentAsync(string department);

        // Tender documents
        Task<List<TenderDocument>> GetTenderDocumentsAsync(int tenderId);
        Task<TenderDocument?> GetTenderDocumentByIdAsync(int documentId);
        Task<int> AddTenderDocumentAsync(TenderDocument document);
        Task<bool> DeleteTenderDocumentAsync(int documentId);

        // Tender bids
        Task<List<TenderBid>> GetTenderBidsAsync(int tenderId);
        Task<TenderBid?> GetTenderBidByIdAsync(int bidId);
        Task<int> AddTenderBidAsync(TenderBid bid);
        Task<bool> UpdateTenderBidAsync(TenderBid bid);
        Task<bool> DeleteTenderBidAsync(int bidId);
        Task<List<TenderBid>> GetBidsByStatusAsync(BidStatus status);
        Task<List<TenderBid>> GetBidsByPaymentStatusAsync(PaymentStatus paymentStatus);

        // Payment operations
        Task<bool> UpdateBidPaymentStatusAsync(int bidId, PaymentStatus status, string? paymentReference = null);
        Task<List<TenderBid>> GetPendingPaymentsAsync();
        Task<decimal> GetTotalEmdCollectedAsync();
        Task<decimal> GetTotalProcessingFeesCollectedAsync();

        // Dashboard statistics
        Task<Dictionary<string, object>> GetDashboardStatisticsAsync();
        Task<List<Tender>> GetExpiringTendersAsync(int days = 7);
        Task<List<Tender>> GetUpcomingTendersAsync(int days = 7);
    }
}
