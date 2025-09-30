using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BiddingSystem.Models;
using BiddingSystem.ViewModels; // Import ViewModels namespace

namespace BiddingSystem.Data
{
    public interface IRefundRepository
    {
        /// <summary>
        /// Gets paginated list of refunds based on user role showing individual payment types
        /// Maker sees eligible payments for refund, Checker sees pending approvals
        /// </summary>
        Task<PaginatedList<RefundListViewModel>> GetRefundListAsync(RefundSearchFilter filter, string userRole);

        /// <summary>
        /// Initiates refund requests for selected payments with individual payment types
        /// Each payment link (EMD, SD, Processing Fee) can be refunded separately
        /// Uses RefundRequestItem from ViewModels namespace
        /// </summary>
        Task<bool> InitiateRefundsAsync(List<RefundRequestItem> refundItems, string reason, int userId);

        /// <summary>
        /// Processes refund approvals/rejections and integrates with Razorpay for actual refund processing
        /// Handles individual payment refunds with their specific Razorpay payment IDs
        /// </summary>
        Task<bool> ProcessRefundsAsync(List<int> refundPaymentIds, string action, string remarks, int userId);

        /// <summary>
        /// Gets refunds filtered by status (Approved, Rejected, Failed, etc.) with payment type details
        /// </summary>
        Task<PaginatedList<ApprovedRefundViewModel>> GetRefundsByStatusAsync(RefundSearchFilter filter);

        /// <summary>
        /// Gets detailed information for a specific refund payment including payment type and link details
        /// </summary>
        Task<ApprovedRefundViewModel?> GetRefundDetailsAsync(int refundPaymentId);

        /// <summary>
        /// Gets refund statistics for dashboard/reporting across all payment types
        /// </summary>
        Task<RefundStatistics> GetRefundStatisticsAsync();

        /// <summary>
        /// Gets statistics grouped by payment type (EMD, SD, Processing Fee)
        /// </summary>
        Task<Dictionary<PaymentType, RefundStatistics>> GetRefundStatisticsByPaymentTypeAsync();

        /// <summary>
        /// Retries a failed refund with the specific payment link and Razorpay details
        /// </summary>
        Task<RetryRefundResult> RetryFailedRefundAsync(int refundPaymentId, int userId);

        /// <summary>
        /// Gets all payment links and their refund status for a tender bid
        /// Shows EMD, SD, and Processing Fee payments separately
        /// </summary>
        Task<List<PaymentRefundSummary>> GetPaymentSummaryForBidAsync(int tenderBidId);

        /// <summary>
        /// Checks if a payment link already has a pending or approved refund
        /// </summary>
        Task<bool> HasPendingOrApprovedRefundAsync(int paymentLinkId);

        /// <summary>
        /// Gets refund history for a specific payment link
        /// </summary>
        Task<List<RefundHistoryItem>> GetRefundHistoryForPaymentAsync(int paymentLinkId);

        /// <summary>
        /// Validates if refunds can be initiated for the given payment links
        /// Checks payment status, existing refunds, and tender status
        /// </summary>
        Task<RefundValidationResult> ValidateRefundRequestAsync(List<RefundRequestItem> refundItems);

        /// <summary>
        /// Gets pending refund count for dashboard notifications
        /// </summary>
        Task<int> GetPendingRefundCountAsync();

        /// <summary>
        /// Gets failed refund count that need retry
        /// </summary>
        Task<int> GetFailedRefundCountAsync();

        /// <summary>
        /// Bulk retry for multiple failed refunds
        /// </summary>
        Task<BulkRetryResult> BulkRetryFailedRefundsAsync(List<int> refundPaymentIds, int userId);

        /// <summary>
        /// Exports refund data for reporting
        /// </summary>
        Task<RefundExportData> GetRefundExportDataAsync(RefundSearchFilter filter);
    }

    // Extension of PartialSuccessException for better error handling
    // This stays in Data namespace as it's specific to repository operations
    //public class PartialSuccessException : Exception
    //{
    //    public List<int> SuccessfulRefunds { get; }
    //    public List<(int RefundId, string Error)> FailedRefunds { get; }

    //    public PartialSuccessException(string message, List<int> successfulRefunds,
    //        List<(int, string)> failedRefunds) : base(message)
    //    {
    //        SuccessfulRefunds = successfulRefunds;
    //        FailedRefunds = failedRefunds;
    //    }
    //}
}