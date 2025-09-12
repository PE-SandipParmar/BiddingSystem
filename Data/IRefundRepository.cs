using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BiddingSystem.Models;
using BiddingSystem.ViewModels;

namespace BiddingSystem.Data
{
    public interface IRefundRepository
    {
        /// <summary>
        /// Gets paginated list of refunds based on user role (Maker sees eligible bidders, Checker sees pending approvals)
        /// </summary>
        Task<PaginatedList<RefundListViewModel>> GetRefundListAsync(RefundSearchFilter filter, string userRole);

        /// <summary>
        /// Initiates refund requests for selected tender bids
        /// </summary>
        Task<bool> InitiateRefundsAsync(List<int> tenderBidIds, string reason, int userId);

        /// <summary>
        /// Processes refund approvals/rejections and integrates with Razorpay for actual refund processing
        /// </summary>
        Task<bool> ProcessRefundsAsync(List<int> refundPaymentIds, string action, string remarks, int userId);

        /// <summary>
        /// Gets refunds filtered by status (Approved, Rejected, Failed, etc.)
        /// </summary>
        Task<PaginatedList<ApprovedRefundViewModel>> GetRefundsByStatusAsync(RefundSearchFilter filter);

        /// <summary>
        /// Gets detailed information for a specific refund payment
        /// </summary>
        Task<ApprovedRefundViewModel?> GetRefundDetailsAsync(int refundPaymentId);

        /// <summary>
        /// Gets refund statistics for dashboard/reporting
        /// </summary>
        Task<RefundStatistics> GetRefundStatisticsAsync();
    }
}