using BiddingSystem.Models;
using BiddingSystem.ViewModels;

namespace BiddingSystem.Data
{
    public interface IRefundRepository
    {
        Task<PaginatedList<ApprovedRefundViewModel>> GetRefundsByStatusAsync(RefundSearchFilter filter);
        Task<PaginatedList<RefundListViewModel>> GetRefundListAsync(RefundSearchFilter filter, string userRole);
        Task<bool> InitiateRefundsAsync(List<int> tenderBidIds, string reason, int userId);
        Task<bool> ProcessRefundsAsync(List<int> refundPaymentIds, string action, string remarks, int userId);
        Task<RefundStatistics> GetRefundStatisticsAsync(); // New method for Admin statistics
    }
}
