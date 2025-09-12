// Location: /Data/RefundRepository.cs
// REPLACE your existing RefundRepository.cs with this complete version

using Dapper;
using BiddingSystem.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using BiddingSystem.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiddingSystem.Services;

namespace BiddingSystem.Data
{
    public class RefundRepository : IRefundRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly IRazorpayPaymentService _razorpayService;
        private readonly IRazorpayService _razorpay;
        private readonly ILogger<RefundRepository> _logger;

        public RefundRepository(
            IConfiguration configuration,
            IRazorpayPaymentService razorpayService,
            IRazorpayService razorpay,
            ILogger<RefundRepository> logger)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _razorpayService = razorpayService;
            _razorpay = razorpay;
            _logger = logger;
        }

        // Fixed ProcessRefundsAsync method with proper transaction handling
        // This replaces the ProcessRefundsAsync method in RefundRepository.cs

        public async Task<bool> ProcessRefundsAsync(List<int> refundPaymentIds, string action, string remarks, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            var successfulRefunds = new List<int>();
            var failedRefunds = new List<(int RefundId, string Error)>();
            var allSuccessful = true;

            try
            {
                var status = action == "Approve" ? "Approved" : "Rejected";
                _logger.LogInformation($"Processing {refundPaymentIds.Count} refunds with action: {action}");

                foreach (var refundId in refundPaymentIds)
                {
                    try
                    {
                        // Get refund payment details with original payment information
                        var refundDetailsSql = @"
                    SELECT 
                        rp.Id,
                        rp.TenderBidId,
                        rp.TenderId,
                        rp.RefundAmount,
                        rp.ReasonForRefund,
                        rp.RefundStatus,
                        pt.RazorpayPaymentId,
                        pt.RazorpayOrderId,
                        pt.Amount as OriginalAmount,
                        tb.BidderName,
                        tb.BidderEmail,
                        t.TenderId as TenderIdString
                    FROM RefundPayments rp WITH (UPDLOCK, ROWLOCK)
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN PaymentTransactions pt ON 
                        pt.TenderBidId = rp.TenderBidId 
                        AND pt.Status = 'Success'
                        AND pt.RazorpayPaymentId IS NOT NULL
                    WHERE rp.Id = @RefundId AND rp.RefundStatus = 'Pending'";

                        var refundDetails = await connection.QuerySingleOrDefaultAsync<dynamic>(
                            refundDetailsSql,
                            new { RefundId = refundId },
                            transaction);

                        if (refundDetails == null)
                        {
                            _logger.LogWarning($"Refund payment {refundId} not found or already processed");
                            continue;
                        }

                        string razorpayRefundId = null;
                        string refundStatus = status;
                        string refundErrorMessage = null;
                        bool razorpayProcessed = false;

                        // If approved and has Razorpay payment, process the refund
                        if (status == "Approved" && refundDetails.RazorpayPaymentId != null)
                        {
                            try
                            {
                                _logger.LogInformation($"Processing Razorpay refund for payment: {refundDetails.RazorpayPaymentId}");

                                // Create notes for the refund
                                var notes = new Dictionary<string, object>
                                {
                                    { "tender_id", refundDetails.TenderIdString },
                                    { "bidder_name", refundDetails.BidderName },
                                    { "refund_reason", refundDetails.ReasonForRefund },
                                    { "refund_payment_id", refundId.ToString() }
                                };

                                // *** CALL RAZORPAY API ***
                                var refundResponse = await _razorpay.ProcessRefund(
                                    refundDetails.RazorpayPaymentId,
                                    refundDetails.RefundAmount,
                                    refundDetails.ReasonForRefund,
                                    notes
                                );

                                if (refundResponse.Success)
                                {
                                    razorpayRefundId = refundResponse.RefundId;
                                    razorpayProcessed = true;
                                    _logger.LogInformation($"Razorpay refund successful. Refund ID: {razorpayRefundId}");

                                    // Only insert transaction record if Razorpay was successful
                                    var insertTransactionSql = @"
                                INSERT INTO PaymentTransactions (
                                    PaymentLinkId,
                                    TenderId,
                                    TenderBidId,
                                    RazorpayPaymentId,
                                    RazorpayOrderId,
                                    Amount,
                                    Status,
                                    PaymentMethod,
                                    CustomerEmail,
                                    CustomerPhone,
                                    TransactionDate,
                                    CreatedAt,
                                    ErrorCode,
                                    ErrorDescription
                                ) VALUES (
                                    @PaymentLinkId,
                                    @TenderId,
                                    @TenderBidId,
                                    @RazorpayRefundId,
                                    @RazorpayOrderId,
                                    @Amount,
                                    @Status,
                                    'REFUND',
                                    @CustomerEmail,
                                    NULL,
                                    GETUTCDATE(),
                                    GETUTCDATE(),
                                    NULL,
                                    @RefundReason
                                )";

                                    await connection.ExecuteAsync(insertTransactionSql, new
                                    {
                                        PaymentLinkId = $"REFUND_{refundDetails.RazorpayPaymentId}",
                                        TenderId = refundDetails.TenderId,
                                        TenderBidId = refundDetails.TenderBidId,
                                        RazorpayRefundId = razorpayRefundId,
                                        RazorpayOrderId = refundDetails.RazorpayOrderId,
                                        Amount = -refundDetails.RefundAmount, // Negative amount for refund
                                        Status = "Refunded",
                                        CustomerEmail = refundDetails.BidderEmail,
                                        RefundReason = refundDetails.ReasonForRefund
                                    }, transaction);

                                    successfulRefunds.Add(refundId);
                                }
                                else
                                {
                                    // Razorpay refund failed - DO NOT update database as approved
                                    refundStatus = "Failed";
                                    refundErrorMessage = refundResponse.ErrorMessage;
                                    failedRefunds.Add((refundId, refundResponse.ErrorMessage));
                                    allSuccessful = false;
                                    _logger.LogError($"Razorpay refund failed for RefundId {refundId}: {refundResponse.ErrorMessage}");
                                }
                            }
                            catch (Exception razorEx)
                            {
                                // Razorpay exception - DO NOT update database as approved
                                refundStatus = "Failed";
                                refundErrorMessage = $"Razorpay Error: {razorEx.Message}";
                                failedRefunds.Add((refundId, razorEx.Message));
                                allSuccessful = false;
                                _logger.LogError(razorEx, $"Exception processing Razorpay refund for RefundId {refundId}");
                            }
                        }
                        else if (status == "Approved" && refundDetails.RazorpayPaymentId == null)
                        {
                            // No Razorpay payment ID - this is okay for manual refunds
                            _logger.LogWarning($"No Razorpay payment found for refund {refundId}. Marking as approved for manual processing.");
                            successfulRefunds.Add(refundId);
                        }
                        else if (status == "Rejected")
                        {
                            // Rejection doesn't need Razorpay processing
                            successfulRefunds.Add(refundId);
                        }

                        // Update refund payment status in database
                        var updateSql = @"
                    UPDATE RefundPayments 
                    SET RefundStatus = @Status,
                        ApprovedBy = @ApprovedBy,
                        ApprovedAt = GETUTCDATE(),
                        CheckerRemarks = @Remarks,
                        RazorpayRefundId = @RazorpayRefundId,
                        RefundErrorMessage = @RefundErrorMessage,
                        RefundProcessedAt = CASE WHEN @RazorpayRefundId IS NOT NULL THEN GETUTCDATE() ELSE NULL END
                    WHERE Id = @RefundId AND RefundStatus = 'Pending'";

                        await connection.ExecuteAsync(updateSql, new
                        {
                            RefundId = refundId,
                            Status = refundStatus,
                            ApprovedBy = userId,
                            Remarks = remarks,
                            RazorpayRefundId = razorpayRefundId,
                            RefundErrorMessage = refundErrorMessage
                        }, transaction);

                        // Only update TenderBid payment status if Razorpay refund was successful
                        if (razorpayProcessed && razorpayRefundId != null)
                        {
                            var updateBidSql = @"
                        UPDATE tb 
                        SET tb.PaymentStatus = 'Refunded',
                            tb.UpdatedAt = GETUTCDATE()
                        FROM TenderBids tb
                        INNER JOIN RefundPayments rp ON tb.Id = rp.TenderBidId
                        WHERE rp.Id = @RefundId";

                            await connection.ExecuteAsync(updateBidSql,
                                new { RefundId = refundId }, transaction);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Error processing this specific refund
                        failedRefunds.Add((refundId, ex.Message));
                        allSuccessful = false;
                        _logger.LogError(ex, $"Error processing refund {refundId}");

                        // Continue with next refund instead of failing entire batch
                        continue;
                    }
                }

                // Decision point: Commit or Rollback
                if (action == "Approve" && failedRefunds.Any())
                {
                    // For approvals with Razorpay failures, we have options:

                    // Option 1: Rollback everything if ANY refund fails (STRICT MODE)
                    // Uncomment this if you want all-or-nothing behavior
                    /*
                    transaction.Rollback();
                    _logger.LogError($"Rolling back all refunds due to {failedRefunds.Count} failures");
                    throw new Exception($"Failed to process {failedRefunds.Count} refund(s). All changes rolled back. Errors: " + 
                        string.Join("; ", failedRefunds.Select(f => $"RefundId {f.RefundId}: {f.Error}")));
                    */

                    // Option 2: Commit successful refunds, mark failed ones as "Failed" (PARTIAL SUCCESS MODE)
                    // This is currently active - allows partial success
                    transaction.Commit();
                    _logger.LogWarning($"Processed refunds with {successfulRefunds.Count} successes and {failedRefunds.Count} failures");

                    // You might want to return details about what succeeded and what failed
                    if (failedRefunds.Any())
                    {
                        var errorMessage = $"Partially successful: {successfulRefunds.Count} refunds processed, {failedRefunds.Count} failed. " +
                            $"Failed RefundIds: {string.Join(", ", failedRefunds.Select(f => f.RefundId))}";
                        throw new PartialSuccessException(errorMessage, successfulRefunds, failedRefunds);
                    }
                }
                else
                {
                    // All successful or rejections (which don't need Razorpay)
                    transaction.Commit();
                    _logger.LogInformation($"Successfully processed {successfulRefunds.Count} refunds");
                }

                return allSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in ProcessRefundsAsync - rolling back all changes");
                transaction.Rollback();
                throw;
            }
        }

        // Add this custom exception class for partial success scenarios
        public class PartialSuccessException : Exception
        {
            public List<int> SuccessfulRefunds { get; }
            public List<(int RefundId, string Error)> FailedRefunds { get; }

            public PartialSuccessException(string message, List<int> successfulRefunds, List<(int, string)> failedRefunds)
                : base(message)
            {
                SuccessfulRefunds = successfulRefunds;
                FailedRefunds = failedRefunds;
            }
        }

        // Rest of your existing methods remain the same
        public async Task<PaginatedList<RefundListViewModel>> GetRefundListAsync(RefundSearchFilter filter, string userRole)
        {
            using var connection = new SqlConnection(_connectionString);
            var offset = (filter.PageNumber - 1) * filter.PageSize;

            string sql;
            if (userRole == "Maker" || userRole == "Admin")
            {
                // For Maker: Show eligible bidders for refund
                sql = @"
                    WITH RefundCTE AS (
                        SELECT 
                            tb.Id as TenderBidId,
                            t.Id as TenderId,
                            t.TenderId as TenderIdString,
                            t.TenderTitle,
                            tb.BidderName,
                            tb.CompanyName,
                            tb.TotalAmount as TotalAmountToRefund,
                            rp.Id as RefundPaymentId,
                            rp.RefundStatus,
                            rp.ReasonForRefund,
                            rp.RazorpayRefundId,
                            rp.RefundErrorMessage,
                            ROW_NUMBER() OVER (ORDER BY t.TenderId, tb.BidderName) as RowNum,
                            COUNT(*) OVER() as TotalCount
                        FROM TenderBids tb
                        INNER JOIN Tenders t ON tb.TenderId = t.Id
                        LEFT JOIN RefundPayments rp ON tb.Id = rp.TenderBidId 
                            AND rp.RefundStatus IN ('Pending', 'Approved', 'Failed')
                        WHERE tb.Status = 'Rejected'
                            AND tb.PaymentStatus = 'Paid'
                            AND t.Status = 3
                            AND tb.IsActive = 1
                            AND (@TenderId IS NULL OR t.TenderId LIKE '%' + @TenderId + '%')
                            AND (@TenderName IS NULL OR t.TenderTitle LIKE '%' + @TenderName + '%')
                    )
                    SELECT * FROM RefundCTE
                    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
                    ORDER BY TenderIdString, BidderName";
            }
            else // Checker role
            {
                // For Checker: Show pending refunds for approval
                sql = @"
                    WITH RefundCTE AS (
                        SELECT 
                            tb.Id as TenderBidId,
                            t.Id as TenderId,
                            t.TenderId as TenderIdString,
                            t.TenderTitle,
                            tb.BidderName,
                            tb.CompanyName,
                            rp.RefundAmount as TotalAmountToRefund,
                            rp.Id as RefundPaymentId,
                            rp.RefundStatus,
                            rp.ReasonForRefund,
                            rp.RazorpayRefundId,
                            rp.RefundErrorMessage,
                            ROW_NUMBER() OVER (ORDER BY rp.InitiatedAt DESC) as RowNum,
                            COUNT(*) OVER() as TotalCount
                        FROM RefundPayments rp
                        INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                        INNER JOIN Tenders t ON rp.TenderId = t.Id
                        WHERE rp.RefundStatus = 'Pending'
                            AND (@TenderId IS NULL OR t.TenderId LIKE '%' + @TenderId + '%')
                            AND (@TenderName IS NULL OR t.TenderTitle LIKE '%' + @TenderName + '%')
                    )
                    SELECT * FROM RefundCTE
                    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
                    ORDER BY TenderIdString, BidderName";
            }

            var parameters = new
            {
                filter.TenderId,
                filter.TenderName,
                Offset = offset,
                filter.PageSize
            };

            var results = await connection.QueryAsync<dynamic>(sql, parameters);

            var list = new PaginatedList<RefundListViewModel>
            {
                Items = new List<RefundListViewModel>(),
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize
            };

            foreach (var item in results)
            {
                list.TotalCount = (int)item.TotalCount;
                list.Items.Add(new RefundListViewModel
                {
                    TenderBidId = item.TenderBidId,
                    TenderId = item.TenderId,
                    TenderIdString = item.TenderIdString,
                    TenderTitle = item.TenderTitle,
                    BidderName = item.BidderName,
                    CompanyName = item.CompanyName,
                    TotalAmountToRefund = item.TotalAmountToRefund,
                    RefundPaymentId = item.RefundPaymentId,
                    RefundStatus = item.RefundStatus,
                    ReasonForRefund = item.ReasonForRefund ?? "Tender awarded to another bidder",
                    HasPendingRefund = item.RefundPaymentId != null,
                    RazorpayRefundId = item.RazorpayRefundId,
                    RefundErrorMessage = item.RefundErrorMessage
                });
            }

            list.TotalPages = (int)Math.Ceiling(list.TotalCount / (double)filter.PageSize);
            return list;
        }

        public async Task<bool> InitiateRefundsAsync(List<int> tenderBidIds, string reason, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var tenderBidId in tenderBidIds)
                {
                    // Check if refund already exists
                    var checkSql = @"
                        SELECT COUNT(*) FROM RefundPayments 
                        WHERE TenderBidId = @TenderBidId AND RefundStatus IN ('Pending', 'Approved')";

                    var exists = await connection.QuerySingleAsync<int>(checkSql,
                        new { TenderBidId = tenderBidId }, transaction);

                    if (exists > 0) continue;

                    // Get tender bid details
                    var bidSql = @"
                        SELECT tb.*, t.Id as TenderId 
                        FROM TenderBids tb 
                        INNER JOIN Tenders t ON tb.TenderId = t.Id
                        WHERE tb.Id = @TenderBidId";

                    var bid = await connection.QuerySingleOrDefaultAsync<dynamic>(bidSql,
                        new { TenderBidId = tenderBidId }, transaction);

                    if (bid == null) continue;

                    // Insert refund record - NO RAZORPAY CALL HERE
                    var insertSql = @"
                        INSERT INTO RefundPayments (
                            TenderBidId, TenderId, RefundAmount, ReasonForRefund, 
                            RefundStatus, InitiatedBy, InitiatedAt, CreatedAt
                        ) VALUES (
                            @TenderBidId, @TenderId, @RefundAmount, @ReasonForRefund,
                            'Pending', @InitiatedBy, GETUTCDATE(), GETUTCDATE()
                        )";

                    await connection.ExecuteAsync(insertSql, new
                    {
                        TenderBidId = tenderBidId,
                        TenderId = bid.TenderId,
                        RefundAmount = bid.TotalAmount,
                        ReasonForRefund = reason,
                        InitiatedBy = userId
                    }, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<RefundStatistics> GetRefundStatisticsAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    COUNT(CASE WHEN RefundStatus = 'Pending' THEN 1 END) as TotalPendingRefunds,
                    COUNT(CASE WHEN RefundStatus = 'Approved' THEN 1 END) as TotalApprovedRefunds,
                    COUNT(CASE WHEN RefundStatus = 'Rejected' THEN 1 END) as TotalRejectedRefunds,
                    COUNT(CASE WHEN RefundStatus = 'Failed' THEN 1 END) as TotalFailedRefunds,
                    SUM(RefundAmount) as TotalRefundAmount,
                    SUM(CASE WHEN RefundStatus = 'Pending' THEN RefundAmount ELSE 0 END) as PendingRefundAmount,
                    SUM(CASE WHEN RefundStatus = 'Approved' THEN RefundAmount ELSE 0 END) as ApprovedRefundAmount
                FROM RefundPayments";

            var stats = await connection.QuerySingleOrDefaultAsync<RefundStatistics>(sql);
            return stats ?? new RefundStatistics();
        }

        public async Task<PaginatedList<ApprovedRefundViewModel>> GetRefundsByStatusAsync(RefundSearchFilter filter)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                var offset = (filter.PageNumber - 1) * filter.PageSize;

                var sql = @"
                    -- First get the count
                    SELECT COUNT(*)
                    FROM RefundPayments rp
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN Users u ON rp.ApprovedBy = u.Id
                    WHERE rp.RefundStatus = @RefundStatus
                        AND (@TenderId IS NULL OR @TenderId = '' OR t.TenderId LIKE '%' + @TenderId + '%')
                        AND (@TenderName IS NULL OR @TenderName = '' OR t.TenderTitle LIKE '%' + @TenderName + '%')
                        AND (@FromDate IS NULL OR rp.ApprovedAt >= @FromDate);

                    -- Then get the data
                    SELECT 
                        rp.Id as RefundPaymentId,
                        tb.Id as TenderBidId,
                        t.Id as TenderId,
                        t.TenderId as TenderIdString,
                        t.TenderTitle,
                        tb.BidderName,
                        tb.BidderEmail,
                        tb.CompanyName,
                        rp.RefundAmount,
                        rp.ReasonForRefund,
                        rp.RefundStatus,
                        rp.InitiatedAt,
                        rp.ApprovedAt,
                        rp.CheckerRemarks,
                        rp.RazorpayRefundId,
                        rp.RefundErrorMessage,
                        u.Username as ApprovedByName,
                        CASE 
                            WHEN tb.PaymentStatus = 'Refunded' THEN 1 
                            ELSE 0 
                        END as PaymentProcessed
                    FROM RefundPayments rp
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN Users u ON rp.ApprovedBy = u.Id
                    WHERE rp.RefundStatus = @RefundStatus
                        AND (@TenderId IS NULL OR @TenderId = '' OR t.TenderId LIKE '%' + @TenderId + '%')
                        AND (@TenderName IS NULL OR @TenderName = '' OR t.TenderTitle LIKE '%' + @TenderName + '%')
                        AND (@FromDate IS NULL OR rp.ApprovedAt >= @FromDate)
                    ORDER BY rp.ApprovedAt DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var parameters = new
                {
                    RefundStatus = filter.RefundStatus ?? "Approved",
                    TenderId = filter.TenderId ?? "",
                    TenderName = filter.TenderName ?? "",
                    FromDate = filter.FromDate,
                    Offset = offset,
                    PageSize = filter.PageSize
                };

                using var multi = await connection.QueryMultipleAsync(sql, parameters);

                var totalCount = await multi.ReadSingleAsync<int>();
                var items = (await multi.ReadAsync<ApprovedRefundViewModel>()).ToList();

                var result = new PaginatedList<ApprovedRefundViewModel>
                {
                    Items = items,
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };

                result.TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in GetRefundsByStatusAsync");
                throw;
            }
        }

        public async Task<ApprovedRefundViewModel?> GetRefundDetailsAsync(int refundPaymentId)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                var sql = @"
                    SELECT 
                        rp.Id as RefundPaymentId,
                        tb.Id as TenderBidId,
                        t.Id as TenderId,
                        t.TenderId as TenderIdString,
                        t.TenderTitle,
                        tb.BidderName,
                        tb.BidderEmail,
                        tb.CompanyName,
                        rp.RefundAmount,
                        rp.ReasonForRefund,
                        rp.RefundStatus,
                        rp.InitiatedAt,
                        rp.ApprovedAt,
                        rp.CheckerRemarks,
                        rp.RazorpayRefundId,
                        rp.RefundErrorMessage,
                        u.Username as ApprovedByName,
                        CASE 
                            WHEN tb.PaymentStatus = 'Refunded' THEN 1 
                            ELSE 0 
                        END as PaymentProcessed
                    FROM RefundPayments rp
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN Users u ON rp.ApprovedBy = u.Id
                    WHERE rp.Id = @RefundPaymentId";

                var parameters = new { RefundPaymentId = refundPaymentId };
                var result = await connection.QueryFirstOrDefaultAsync<ApprovedRefundViewModel>(sql, parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in GetRefundDetailsAsync");
                return null;
            }
        }
        public async Task<RetryRefundResult> RetryFailedRefundAsync(int refundPaymentId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get the failed refund details
                var refundDetailsSql = @"
            SELECT 
                rp.*,
                pt.RazorpayPaymentId,
                pt.RazorpayOrderId,
                pt.Amount as FinalRefundAmount,  -- Get refund amount from actual payment
                pt.PaymentMethod as OriginalPaymentMethod,
                pt.TransactionDate as OriginalPaymentDate,
                tb.BidderName,
                tb.BidderEmail,
                t.TenderId as TenderIdString
            FROM RefundPayments rp WITH (UPDLOCK, ROWLOCK)
            INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
            INNER JOIN Tenders t ON rp.TenderId = t.Id
            INNER JOIN PaymentTransactions pt ON  -- Changed to INNER JOIN to ensure payment exists
                pt.TenderBidId = rp.TenderBidId 
                AND pt.TenderId = rp.TenderId  -- Added TenderId check for additional validation
                AND pt.Status = 'Success'  -- Only successful payments
                AND pt.RazorpayPaymentId IS NOT NULL  -- Must have valid payment ID
                AND (pt.PaymentMethod != 'REFUND' OR pt.PaymentMethod IS NULL)  -- Exclude previous refund transactions
            WHERE 
                rp.Id = @RefundPaymentId
                AND rp.RefundStatus IN ('Failed', 'Pending', 'Retry')  -- Allow retry for failed/pending refunds";

                var refundDetails = await connection.QuerySingleOrDefaultAsync<dynamic>(
                    refundDetailsSql,
                    new { RefundPaymentId = refundPaymentId },
                    transaction);

                if (refundDetails == null)
                {
                    transaction.Rollback();
                    return new RetryRefundResult
                    {
                        Success = false,
                        Message = "Refund not found or is not in failed status."
                    };
                }

                if (refundDetails.RazorpayPaymentId == null)
                {
                    transaction.Rollback();
                    return new RetryRefundResult
                    {
                        Success = false,
                        Message = "Original payment not found. Cannot process refund without original payment reference."
                    };
                }

                // Clear previous error and attempt retry
                var clearErrorSql = @"
            UPDATE RefundPayments 
            SET RefundErrorMessage = NULL
            WHERE Id = @RefundPaymentId";

                await connection.ExecuteAsync(clearErrorSql,
                    new { RefundPaymentId = refundPaymentId },
                    transaction);

                _logger.LogInformation($"Retrying refund for payment: {refundDetails.RazorpayPaymentId}");

                // Create notes for the refund
                var notes = new Dictionary<string, object>
        {
            { "tender_id", refundDetails.TenderIdString },
            { "bidder_name", refundDetails.BidderName },
            { "refund_reason", refundDetails.ReasonForRefund },
            { "refund_payment_id", refundPaymentId.ToString() },
            { "retry_attempt", "true" },
            { "retried_by_user_id", userId.ToString() }
        };

                // Attempt to process refund through Razorpay
                var refundResponse = await _razorpay.ProcessRefund(
                    refundDetails.RazorpayPaymentId,
                    refundDetails.FinalRefundAmount,
                    refundDetails.ReasonForRefund + " (Retry)",
                    notes
                );

                if (refundResponse.Success)
                {
                    // Update refund payment as successful
                    var updateSuccessSql = @"
                UPDATE RefundPayments 
                SET RefundStatus = 'Approved',
                    RazorpayRefundId = @RazorpayRefundId,
                    RefundErrorMessage = NULL,
                    ApprovedBy = @ApprovedBy,
                    ApprovedAt = GETUTCDATE(),
                    RefundProcessedAt = GETUTCDATE()
                WHERE Id = @RefundPaymentId";

                    await connection.ExecuteAsync(updateSuccessSql, new
                    {
                        RefundPaymentId = refundPaymentId,
                        RazorpayRefundId = refundResponse.RefundId,
                        ApprovedBy = userId
                    }, transaction);

                    // Insert refund transaction record
                    var insertTransactionSql = @"
                INSERT INTO PaymentTransactions (
                    PaymentLinkId,
                    TenderId,
                    TenderBidId,
                    RazorpayPaymentId,
                    RazorpayOrderId,
                    Amount,
                    Status,
                    PaymentMethod,
                    CustomerEmail,
                    TransactionDate,
                    CreatedAt,
                    ErrorDescription
                ) VALUES (
                    @PaymentLinkId,
                    @TenderId,
                    @TenderBidId,
                    @RazorpayRefundId,
                    @RazorpayOrderId,
                    @Amount,
                    'Refunded',
                    'REFUND_RETRY',
                    @CustomerEmail,
                    GETUTCDATE(),
                    GETUTCDATE(),
                    'Refund retry successful'
                )";

                    await connection.ExecuteAsync(insertTransactionSql, new
                    {
                        PaymentLinkId = $"REFUND_RETRY_{refundDetails.RazorpayPaymentId}",
                        TenderId = refundDetails.TenderId,
                        TenderBidId = refundDetails.TenderBidId,
                        RazorpayRefundId = refundResponse.RefundId,
                        RazorpayOrderId = refundDetails.RazorpayOrderId,
                        Amount = -refundDetails.FinalRefundAmount,
                        CustomerEmail = refundDetails.BidderEmail
                    }, transaction);

                    // Update TenderBid payment status
                    var updateBidSql = @"
                UPDATE tb 
                SET tb.PaymentStatus = 'Refunded'
                FROM TenderBids tb
                WHERE tb.Id = @TenderBidId";

                    await connection.ExecuteAsync(updateBidSql,
                        new { TenderBidId = refundDetails.TenderBidId },
                        transaction);

                    transaction.Commit();

                    _logger.LogInformation($"Refund retry successful. Refund ID: {refundResponse.RefundId}");

                    return new RetryRefundResult
                    {
                        Success = true,
                        Message = $"Refund processed successfully. Amount ₹{refundDetails.FinalRefundAmount:N2} has been refunded.",
                        RazorpayRefundId = refundResponse.RefundId
                    };
                }
                else
                {
                    // Update with new error message
                    var updateFailureSql = @"
                UPDATE RefundPayments 
                SET RefundErrorMessage = @ErrorMessage
                WHERE Id = @RefundPaymentId";

                    await connection.ExecuteAsync(updateFailureSql, new
                    {
                        RefundPaymentId = refundPaymentId,
                        ErrorMessage = $"Retry failed: {refundResponse.ErrorMessage}"
                    }, transaction);

                    transaction.Commit();

                    _logger.LogError($"Refund retry failed: {refundResponse.ErrorMessage}");

                    return new RetryRefundResult
                    {
                        Success = false,
                        Message = $"Refund retry failed: {refundResponse.ErrorMessage}"
                    };
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, $"Error retrying refund {refundPaymentId}");

                // Update error message in database
                try
                {
                    using var errorConnection = new SqlConnection(_connectionString);
                    var updateErrorSql = @"
                UPDATE RefundPayments 
                SET RefundErrorMessage = @ErrorMessage
                WHERE Id = @RefundPaymentId";

                    await errorConnection.ExecuteAsync(updateErrorSql, new
                    {
                        RefundPaymentId = refundPaymentId,
                        ErrorMessage = $"Retry exception: {ex.Message}"
                    });
                }
                catch
                {
                    // Log but don't throw
                }

                return new RetryRefundResult
                {
                    Success = false,
                    Message = $"An error occurred while retrying the refund: {ex.Message}"
                };
            }
        }

    }
}