using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface ITenderBidService
    {
        Task<IEnumerable<TenderBid>> GetAllBidsAsync();
        Task<TenderBid?> GetBidByIdAsync(int id);
        Task<TenderBid?> GetBidByPaymentReferenceAsync(string paymentReference);
        Task<IEnumerable<TenderBid>> SearchBidsAsync(string? bidderName, string? companyName, string? status, string? paymentStatus);
        Task<TenderBid> CreateBidAsync(TenderBid bid);
        Task<TenderBid> UpdateBidAsync(TenderBid bid);
        Task<bool> DeleteBidAsync(int id);
        Task<IEnumerable<TenderBid>> GetBidsByTenderIdAsync(int tenderId);
        Task<IEnumerable<TenderBid>> GetBidsByStatusAsync(string status);
        Task<IEnumerable<TenderBid>> GetBidsByPaymentStatusAsync(string paymentStatus);
        Task<(IEnumerable<TenderBid> bids, int totalCount)> GetBidsPagedAsync(int page, int pageSize, string? searchTerm = null, string? status = null, string? paymentStatus = null);
        Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus, string? paymentReference = null, DateTime? paymentDate = null);
        Task<string> GenerateUniquePaymentReferenceAsync();
        Task<bool> ValidateBidAsync(TenderBid bid);
        Task<decimal> CalculateTotalAmountAsync(decimal? bidAmount, decimal? emdAmount, decimal? processingFee);
        Task<IEnumerable<TenderBidDocument>> GetBidDocumentsAsync(int bidId);
        Task<TenderBidDocument> AddBidDocumentAsync(int bidId, IFormFile file, BidDocumentType documentType);
        Task<bool> DeleteBidDocumentAsync(int documentId);
        Task<bool> CheckDuplicateBidAsync(int tenderId, string bidderEmail);
    }
}
