using BiddingSystem.Models;
using BiddingSystem.ViewModels;

namespace BiddingSystem.Data
{
    public interface ITenderBidRepository
    {
        // Basic CRUD Operations
        Task<IEnumerable<TenderBid>> GetAllBidsAsync();
        Task<TenderBid?> GetBidByIdAsync(int id);
        Task<TenderBid?> GetBidByPaymentReferenceAsync(string paymentReference);
        Task<IEnumerable<TenderBid>> SearchBidsAsync(string? bidderName, string? companyName, string? status, string? paymentStatus);
        Task<TenderBid> CreateBidAsync(TenderBid bid);
        Task<TenderBid> UpdateBidAsync(TenderBid bid);
        Task<bool> DeleteBidAsync(int id);

        // Bid Queries
        Task<IEnumerable<TenderBid>> GetBidsByTenderIdAsync(int tenderId);
        Task<IEnumerable<TenderBid>> GetBidsByStatusAsync(string status);
        Task<IEnumerable<TenderBid>> GetBidsByPaymentStatusAsync(string paymentStatus);
        Task<(IEnumerable<TenderBid> bids, int totalCount)> GetBidsPagedAsync(int page, int pageSize, string? searchTerm = null, string? status = null, string? paymentStatus = null);

        // Payment Operations
        Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus, string? paymentReference = null, DateTime? paymentDate = null);
        Task<string> GenerateUniquePaymentReferenceAsync();

        // Document Operations
        Task<IEnumerable<TenderBidDocument>> GetBidDocumentsAsync(int bidId);
        Task<TenderBidDocument> AddBidDocumentAsync(TenderBidDocument document);
        Task<bool> DeleteBidDocumentAsync(int documentId);

        // Validation
        Task<bool> CheckDuplicateBidAsync(int tenderId, string bidderEmail);

        // NEW: Add these missing methods for the payment dashboard functionality
        Task<RefundPaymentInfo?> GetRefundInfoForBidAsync(int tenderBidId);
        Task<List<TenderBid>> GetBidsByEmailOrPhoneAsync(string email, string phone);
    }
}