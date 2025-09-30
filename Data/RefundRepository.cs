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

        // Fixed GetRefundListAsync method with corrected Checker query
        // Replace the existing method in RefundRepository.cs


        // Updated GetRefundListAsync method in RefundRepository.cs
        // Replace the existing method with this version

        public async Task<PaginatedList<RefundListViewModel>> GetRefundListAsync(RefundSearchFilter filter, string userRole)
        {
            using var connection = new SqlConnection(_connectionString);
            var offset = (filter.PageNumber - 1) * filter.PageSize;

            string sql;
            if (userRole == "Maker" || (userRole == "Admin" && !filter.ShowPendingOnly))
            {
                // For Maker: Show ALL individual payment links that are eligible for refund
                // This includes multiple payment types for the same bid
                sql = @"
            WITH RefundCTE AS (
                SELECT 
                    tb.Id as TenderBidId,
                    t.Id as TenderId,
                    t.TenderId as TenderIdString,
                    t.TenderTitle,
                    tb.BidderName,
                    tb.CompanyName,
                    pl.Id as PaymentLinkId,
                    pl.PaymentType,
                    CASE pl.PaymentType 
                        WHEN 1 THEN 'Bid Amount'
                        WHEN 2 THEN 'EMD Amount'
                        WHEN 3 THEN 'Processing Fee'
                        WHEN 4 THEN 'Security Deposit'
                        ELSE 'Other'
                    END as PaymentTypeDisplay,
                    pl.Amount as PaymentAmount,
                    pl.Status as PaymentLinkStatus,
                    pl.LinkId,
                    pt.RazorpayPaymentId,
                    pt.TransactionDate,
                    rp.Id as RefundPaymentId,
                    rp.RefundStatus,
                    rp.ReasonForRefund,
                    rp.RazorpayRefundId,
                    rp.RefundErrorMessage,
                    ROW_NUMBER() OVER (ORDER BY t.TenderId, tb.BidderName, pl.PaymentType) as RowNum,
                    COUNT(*) OVER() as TotalCount
                FROM TenderBids tb
                INNER JOIN Tenders t ON tb.TenderId = t.Id
                INNER JOIN PaymentLinks pl ON pl.TenderBidId = tb.Id
                INNER JOIN PaymentTransactions pt ON 
                    pt.PaymentLinkId = pl.LinkId 
                    AND pt.Status = 'Success'
                    AND pt.RazorpayPaymentId IS NOT NULL
                LEFT JOIN RefundPayments rp ON 
                    rp.PaymentLinkId = pl.Id 
                    AND rp.RefundStatus NOT IN ('Rejected') -- Exclude rejected refunds
                WHERE tb.Status = 'Rejected'  -- Bid is rejected
                    AND pl.Status = 2 -- Payment Link is Used/Paid
                    AND t.Status = 3 -- Tender is Closed/Awarded
                    AND tb.IsActive = 1
                    AND pl.IsActive = 1
                    AND rp.Id IS NULL -- No existing refund (or only rejected refunds exist)
                    AND (@TenderId IS NULL OR @TenderId = '' OR t.TenderId LIKE '%' + @TenderId + '%')
                    AND (@TenderName IS NULL OR @TenderName = '' OR t.TenderTitle LIKE '%' + @TenderName + '%')
                    AND (@PaymentType IS NULL OR pl.PaymentType = @PaymentType)
            )
            SELECT * FROM RefundCTE
            WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
            ORDER BY TenderIdString, BidderName, PaymentType";
            }
            else // Checker, Approver role or Admin with ShowPendingOnly
            {
                // For Checker: Show pending AND failed refunds for approval/retry
                sql = @"
            WITH RefundCTE AS (
                SELECT 
                    tb.Id as TenderBidId,
                    t.Id as TenderId,
                    t.TenderId as TenderIdString,
                    t.TenderTitle,
                    tb.BidderName,
                    tb.CompanyName,
                    rp.PaymentLinkId,
                    COALESCE(rp.PaymentType, pl.PaymentType) as PaymentType,
                    CASE COALESCE(rp.PaymentType, pl.PaymentType)
                        WHEN 1 THEN 'Bid Amount'
                        WHEN 2 THEN 'EMD Amount'
                        WHEN 3 THEN 'Processing Fee'
                        WHEN 4 THEN 'Security Deposit'
                        ELSE 'Other'
                    END as PaymentTypeDisplay,
                    rp.RefundAmount as PaymentAmount,
                    COALESCE(pl.Status, 2) as PaymentLinkStatus,
                    pl.LinkId,
                    COALESCE(rp.OriginalPaymentId, pt.RazorpayPaymentId) as RazorpayPaymentId,
                    pt.TransactionDate,
                    rp.Id as RefundPaymentId,
                    rp.RefundStatus,
                    rp.ReasonForRefund,
                    rp.RazorpayRefundId,
                    rp.RefundErrorMessage,
                    rp.InitiatedAt,
                    ROW_NUMBER() OVER (ORDER BY 
                        CASE 
                            WHEN rp.RefundStatus = 'Failed' THEN 1
                            WHEN rp.RefundStatus = 'Pending' THEN 2
                            ELSE 3
                        END,
                        rp.InitiatedAt DESC
                    ) as RowNum,
                    COUNT(*) OVER() as TotalCount
                FROM RefundPayments rp
                INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                INNER JOIN Tenders t ON rp.TenderId = t.Id
                LEFT JOIN PaymentLinks pl ON rp.PaymentLinkId = pl.Id
                LEFT JOIN PaymentTransactions pt ON 
                    pl.LinkId = pt.PaymentLinkId
                    AND pt.Status = 'Success'
                    AND pt.RazorpayPaymentId IS NOT NULL
                WHERE rp.RefundStatus IN ('Pending', 'Failed')
                    AND (@TenderId IS NULL OR @TenderId = '' OR t.TenderId LIKE '%' + @TenderId + '%')
                    AND (@TenderName IS NULL OR @TenderName = '' OR t.TenderTitle LIKE '%' + @TenderName + '%')
                    AND (@PaymentType IS NULL OR COALESCE(rp.PaymentType, pl.PaymentType) = @PaymentType)
            )
            SELECT * FROM RefundCTE
            WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
            ORDER BY RowNum";
            }

            var parameters = new
            {
                TenderId = filter.TenderId ?? "",
                TenderName = filter.TenderName ?? "",
                PaymentType = filter.PaymentType.HasValue ? (int)filter.PaymentType.Value : (int?)null,
                Offset = offset,
                PageSize = filter.PageSize
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

                // Determine the payment link status string
                string paymentLinkStatusString = "Active";
                if (item.PaymentLinkStatus == 2) // Used status enum value
                {
                    paymentLinkStatusString = "Used";
                }

                // For Maker view - show transaction details
                string transactionInfo = null;
                if (item.RazorpayPaymentId != null && userRole == "Maker")
                {
                    var txnDate = item.TransactionDate != null
                        ? Convert.ToDateTime(item.TransactionDate).ToString("dd MMM yyyy HH:mm")
                        : "";
                    transactionInfo = $"TXN: {item.RazorpayPaymentId} on {txnDate}";
                }

                list.Items.Add(new RefundListViewModel
                {
                    TenderBidId = item.TenderBidId,
                    TenderId = item.TenderId,
                    TenderIdString = item.TenderIdString,
                    TenderTitle = item.TenderTitle,
                    BidderName = item.BidderName,
                    CompanyName = item.CompanyName,
                    PaymentType = item.PaymentType != null ? (PaymentType)item.PaymentType : PaymentType.Other,
                    PaymentTypeDisplay = item.PaymentTypeDisplay ?? "Other",
                    PaymentLinkId = item.PaymentLinkId,
                    PaymentLinkStatus = paymentLinkStatusString,
                    RazorpayPaymentId = item.RazorpayPaymentId,
                    PaymentAmount = item.PaymentAmount,
                    TotalAmountToRefund = item.PaymentAmount,
                    RefundPaymentId = item.RefundPaymentId,
                    RefundStatus = item.RefundStatus,
                    ReasonForRefund = item.ReasonForRefund ?? "Tender awarded to another bidder",
                    HasPendingRefund = item.RefundPaymentId != null,
                    RazorpayRefundId = item.RazorpayRefundId,
                    RefundErrorMessage = item.RefundErrorMessage,
                    TransactionDetails = transactionInfo
                });
            }

            list.TotalPages = (int)Math.Ceiling(list.TotalCount / (double)filter.PageSize);
            return list;
        }


        public async Task<bool> InitiateRefundsAsync(List<RefundRequestItem> refundItems, string reason, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var item in refundItems)
                {
                    // Check if refund already exists for this payment link
                    var checkSql = @"
                        SELECT COUNT(*) FROM RefundPayments 
                        WHERE PaymentLinkId = @PaymentLinkId AND RefundStatus IN ('Pending', 'Approved')";

                    var exists = await connection.QuerySingleAsync<int>(checkSql,
                        new { PaymentLinkId = item.PaymentLinkId }, transaction);

                    if (exists > 0) continue;

                    // Get payment link and tender bid details with Razorpay payment info
                    var paymentSql = @"
                        SELECT 
                            pl.Id, 
                            pl.Amount, 
                            pl.PaymentType,
                            pl.LinkId,
                            tb.TenderId, 
                            pt.RazorpayPaymentId 
                        FROM PaymentLinks pl 
                        INNER JOIN TenderBids tb ON pl.TenderBidId = tb.Id
                        LEFT JOIN PaymentTransactions pt ON 
                            pt.PaymentLinkId = pl.LinkId 
                            AND pt.Status = 'Success'
                            AND pt.RazorpayPaymentId IS NOT NULL
                        WHERE pl.Id = @PaymentLinkId";

                    var payment = await connection.QuerySingleOrDefaultAsync<dynamic>(paymentSql,
                        new { PaymentLinkId = item.PaymentLinkId }, transaction);

                    if (payment == null) continue;

                    // Insert refund record with payment type
                    var insertSql = @"
                        INSERT INTO RefundPayments (
                            TenderBidId, TenderId, PaymentType, PaymentLinkId, 
                            OriginalPaymentId, RefundAmount, ReasonForRefund, 
                            RefundStatus, InitiatedBy, InitiatedAt, CreatedAt
                        ) VALUES (
                            @TenderBidId, @TenderId, @PaymentType, @PaymentLinkId,
                            @OriginalPaymentId, @RefundAmount, @ReasonForRefund,
                            'Pending', @InitiatedBy, GETUTCDATE(), GETUTCDATE()
                        )";

                    await connection.ExecuteAsync(insertSql, new
                    {
                        TenderBidId = item.TenderBidId,
                        TenderId = payment.TenderId,
                        PaymentType = (int)item.PaymentType,
                        PaymentLinkId = item.PaymentLinkId,
                        OriginalPaymentId = payment.RazorpayPaymentId,
                        RefundAmount = payment.Amount,
                        ReasonForRefund = reason,
                        InitiatedBy = userId
                    }, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating refunds");
                transaction.Rollback();
                throw;
            }
        }

        // Fixed ProcessRefundsAsync method - replace the existing method in RefundRepository.cs

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
                        // Get refund payment details - use strongly typed query result
                        var refundDetailsSql = @"
                    SELECT 
                        rp.Id,
                        rp.TenderBidId,
                        rp.TenderId,
                        CAST(rp.PaymentType as INT) as PaymentType,
                        rp.PaymentLinkId,
                        rp.RefundAmount,
                        rp.ReasonForRefund,
                        rp.RefundStatus,
                        pl.LinkId as PaymentLinkLinkId,
                        CAST(pl.PaymentType as INT) as PaymentLinkPaymentType,
                        pt.RazorpayPaymentId,
                        pt.RazorpayOrderId,
                        tb.BidderName,
                        tb.BidderEmail,
                        t.TenderId as TenderIdString
                    FROM RefundPayments rp WITH (UPDLOCK, ROWLOCK)
                    INNER JOIN PaymentLinks pl ON rp.PaymentLinkId = pl.Id
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN PaymentTransactions pt ON 
                        pt.PaymentLinkId = pl.LinkId 
                        AND pt.Status = 'Success'
                        AND pt.RazorpayPaymentId IS NOT NULL
                    WHERE rp.Id = @RefundId AND rp.RefundStatus IN ('Pending', 'Failed')";

                        var refundDetails = await connection.QuerySingleOrDefaultAsync<RefundDetailsDto>(
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
                        if (status == "Approved" && !string.IsNullOrEmpty(refundDetails.RazorpayPaymentId))
                        {
                            try
                            {
                                _logger.LogInformation($"Processing Razorpay refund for payment: {refundDetails.RazorpayPaymentId}");

                                // Safe conversion of PaymentType
                                PaymentType paymentTypeEnum = (PaymentType)(refundDetails.PaymentType ?? 0);
                                string paymentTypeStr = paymentTypeEnum.ToString();

                                // Create notes for the refund
                                var notes = new Dictionary<string, object>
                        {
                            { "tender_id", refundDetails.TenderIdString ?? "" },
                            { "bidder_name", refundDetails.BidderName ?? "" },
                            { "payment_type", paymentTypeStr },
                            { "payment_link_id", refundDetails.PaymentLinkId.ToString() },
                            { "refund_reason", refundDetails.ReasonForRefund ?? "" },
                            { "refund_payment_id", refundId.ToString() }
                        };

                                // Call Razorpay API
                                var refundResponse = await _razorpay.ProcessRefund(
                                    refundDetails.RazorpayPaymentId,
                                    refundDetails.RefundAmount,
                                    $"{paymentTypeStr} - {refundDetails.ReasonForRefund}",
                                    notes
                                );

                                if (refundResponse.Success)
                                {
                                    razorpayRefundId = refundResponse.RefundId;
                                    razorpayProcessed = true;
                                    _logger.LogInformation($"Razorpay refund successful. Refund ID: {razorpayRefundId}");

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
                                    'REFUND',
                                    @CustomerEmail,
                                    GETUTCDATE(),
                                    GETUTCDATE(),
                                    @RefundReason
                                )";

                                    await connection.ExecuteAsync(insertTransactionSql, new
                                    {
                                        PaymentLinkId = $"REFUND_{refundDetails.PaymentLinkLinkId}",
                                        TenderId = refundDetails.TenderId,
                                        TenderBidId = refundDetails.TenderBidId,
                                        RazorpayRefundId = razorpayRefundId,
                                        RazorpayOrderId = refundDetails.RazorpayOrderId,
                                        Amount = -refundDetails.RefundAmount,
                                        CustomerEmail = refundDetails.BidderEmail,
                                        RefundReason = refundDetails.ReasonForRefund
                                    }, transaction);

                                    successfulRefunds.Add(refundId);
                                }
                                else
                                {
                                    refundStatus = "Failed";
                                    refundErrorMessage = refundResponse.ErrorMessage;
                                    failedRefunds.Add((refundId, refundResponse.ErrorMessage));
                                    allSuccessful = false;
                                    _logger.LogError($"Razorpay refund failed for RefundId {refundId}: {refundResponse.ErrorMessage}");
                                }
                            }
                            catch (Exception razorEx)
                            {
                                refundStatus = "Failed";
                                refundErrorMessage = $"Razorpay Error: {razorEx.Message}";
                                failedRefunds.Add((refundId, razorEx.Message));
                                allSuccessful = false;
                                _logger.LogError(razorEx, $"Exception processing Razorpay refund for RefundId {refundId}");
                            }
                        }
                        else if (status == "Approved" && string.IsNullOrEmpty(refundDetails.RazorpayPaymentId))
                        {
                            _logger.LogWarning($"No Razorpay payment found for refund {refundId}. Marking as approved for manual processing.");
                            successfulRefunds.Add(refundId);
                        }
                        else if (status == "Rejected")
                        {
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
                    WHERE Id = @RefundId AND RefundStatus IN ('Pending', 'Failed')";

                        await connection.ExecuteAsync(updateSql, new
                        {
                            RefundId = refundId,
                            Status = refundStatus,
                            ApprovedBy = userId,
                            Remarks = remarks,
                            RazorpayRefundId = razorpayRefundId,
                            RefundErrorMessage = refundErrorMessage
                        }, transaction);

                        // Update payment link refund status if successful
                        if (razorpayProcessed && !string.IsNullOrEmpty(razorpayRefundId))
                        {
                            var updateLinkSql = @"
                        UPDATE PaymentLinks 
                        SET RefundStatus = 'Refunded',
                            RefundId = @RefundId,
                            RefundAmount = @RefundAmount,
                            RefundDate = GETUTCDATE()
                        WHERE Id = @PaymentLinkId";

                            await connection.ExecuteAsync(updateLinkSql,
                                new
                                {
                                    RefundId = razorpayRefundId,
                                    RefundAmount = refundDetails.RefundAmount,
                                    PaymentLinkId = refundDetails.PaymentLinkId
                                }, transaction);
                        }
                    }
                    catch (Exception ex)
                    {
                        failedRefunds.Add((refundId, ex.Message));
                        allSuccessful = false;
                        _logger.LogError(ex, $"Error processing refund {refundId}");
                        continue;
                    }
                }

                transaction.Commit();

                // Log results
                _logger.LogInformation($"Processed refunds with {successfulRefunds.Count} successes and {failedRefunds.Count} failures");

                // If there were failures but also successes, throw PartialSuccessException
                if (failedRefunds.Any() && successfulRefunds.Any())
                {
                    var errorMessage = $"Partially successful: {successfulRefunds.Count} refunds processed, {failedRefunds.Count} failed. " +
                        $"Failed RefundIds: {string.Join(", ", failedRefunds.Select(f => f.RefundId))}";
                    throw new PartialSuccessException(errorMessage, successfulRefunds, failedRefunds);
                }
                else if (failedRefunds.Any() && !successfulRefunds.Any())
                {
                    // All failed - return false
                    return false;
                }

                return true;
            }
            catch (PartialSuccessException)
            {
                // Re-throw partial success exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in ProcessRefundsAsync - rolling back all changes");
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error during transaction rollback");
                }
                throw;
            }
        }

        public class RefundDetailsDto
        {
            public int Id { get; set; }
            public int TenderBidId { get; set; }
            public int TenderId { get; set; }
            public int? PaymentType { get; set; }
            public int PaymentLinkId { get; set; }
            public decimal RefundAmount { get; set; }
            public string? ReasonForRefund { get; set; }
            public string? RefundStatus { get; set; }
            public string? PaymentLinkLinkId { get; set; }
            public int? PaymentLinkPaymentType { get; set; }
            public string? RazorpayPaymentId { get; set; }
            public string? RazorpayOrderId { get; set; }
            public string? BidderName { get; set; }
            public string? BidderEmail { get; set; }
            public string? TenderIdString { get; set; }
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
                        AND (@PaymentType IS NULL OR rp.PaymentType = @PaymentType)
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
                        rp.PaymentType,
                        CASE rp.PaymentType 
                            WHEN 1 THEN 'Bid Amount (EMD)'
                            WHEN 2 THEN 'Security Deposit'
                            WHEN 3 THEN 'Processing Fee'
                            ELSE 'Other'
                        END as PaymentTypeDisplay,
                        rp.OriginalPaymentId,
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
                            WHEN rp.RazorpayRefundId IS NOT NULL THEN 1 
                            ELSE 0 
                        END as PaymentProcessed
                    FROM RefundPayments rp
                    INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                    INNER JOIN Tenders t ON rp.TenderId = t.Id
                    LEFT JOIN Users u ON rp.ApprovedBy = u.Id
                    WHERE rp.RefundStatus = @RefundStatus
                        AND (@TenderId IS NULL OR @TenderId = '' OR t.TenderId LIKE '%' + @TenderId + '%')
                        AND (@TenderName IS NULL OR @TenderName = '' OR t.TenderTitle LIKE '%' + @TenderName + '%')
                        AND (@PaymentType IS NULL OR rp.PaymentType = @PaymentType)
                        AND (@FromDate IS NULL OR rp.ApprovedAt >= @FromDate)
                    ORDER BY rp.ApprovedAt DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var parameters = new
                {
                    RefundStatus = filter.RefundStatus ?? "Approved",
                    TenderId = filter.TenderId ?? "",
                    TenderName = filter.TenderName ?? "",
                    PaymentType = filter.PaymentType.HasValue ? (int)filter.PaymentType.Value : (int?)null,
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
                        rp.PaymentType,
                        CASE rp.PaymentType 
                            WHEN 1 THEN 'Bid Amount (EMD)'
                            WHEN 2 THEN 'Security Deposit'
                            WHEN 3 THEN 'Processing Fee'
                            ELSE 'Other'
                        END as PaymentTypeDisplay,
                        rp.OriginalPaymentId,
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
                            WHEN rp.RazorpayRefundId IS NOT NULL THEN 1 
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

        public async Task<Dictionary<PaymentType, RefundStatistics>> GetRefundStatisticsByPaymentTypeAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    PaymentType,
                    COUNT(CASE WHEN RefundStatus = 'Pending' THEN 1 END) as TotalPendingRefunds,
                    COUNT(CASE WHEN RefundStatus = 'Approved' THEN 1 END) as TotalApprovedRefunds,
                    COUNT(CASE WHEN RefundStatus = 'Rejected' THEN 1 END) as TotalRejectedRefunds,
                    COUNT(CASE WHEN RefundStatus = 'Failed' THEN 1 END) as TotalFailedRefunds,
                    SUM(RefundAmount) as TotalRefundAmount,
                    SUM(CASE WHEN RefundStatus = 'Pending' THEN RefundAmount ELSE 0 END) as PendingRefundAmount,
                    SUM(CASE WHEN RefundStatus = 'Approved' THEN RefundAmount ELSE 0 END) as ApprovedRefundAmount
                FROM RefundPayments
                WHERE PaymentType IS NOT NULL
                GROUP BY PaymentType";

            var results = await connection.QueryAsync<dynamic>(sql);
            var statistics = new Dictionary<PaymentType, RefundStatistics>();

            foreach (var result in results)
            {
                var paymentType = (PaymentType)result.PaymentType;
                statistics[paymentType] = new RefundStatistics
                {
                    TotalPendingRefunds = result.TotalPendingRefunds,
                    TotalApprovedRefunds = result.TotalApprovedRefunds,
                    TotalRejectedRefunds = result.TotalRejectedRefunds,
                    TotalFailedRefunds = result.TotalFailedRefunds,
                    TotalRefundAmount = result.TotalRefundAmount ?? 0,
                    PendingRefundAmount = result.PendingRefundAmount ?? 0,
                    ApprovedRefundAmount = result.ApprovedRefundAmount ?? 0
                };
            }

            return statistics;
        }

        public async Task<RetryRefundResult> RetryFailedRefundAsync(int refundPaymentId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get the failed refund details with payment link info
                // FIXED: Cast PaymentType as INT in SQL query
                var refundDetailsSql = @"
            SELECT 
                rp.Id,
                rp.TenderBidId,
                rp.TenderId,
                CAST(rp.PaymentType as INT) as PaymentType,
                rp.PaymentLinkId,
                rp.RefundAmount,
                rp.ReasonForRefund,
                rp.RefundStatus,
                pl.LinkId,
                CAST(pl.PaymentType as INT) as PaymentLinkPaymentType,
                pt.RazorpayPaymentId,
                pt.RazorpayOrderId,
                pt.Amount as PaymentAmount,
                tb.BidderName,
                tb.BidderEmail,
                t.TenderId as TenderIdString
            FROM RefundPayments rp WITH (UPDLOCK, ROWLOCK)
            INNER JOIN PaymentLinks pl ON rp.PaymentLinkId = pl.Id
            INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
            INNER JOIN Tenders t ON rp.TenderId = t.Id
            INNER JOIN PaymentTransactions pt ON 
                pt.PaymentLinkId = pl.LinkId 
                AND pt.Status = 'Success'
                AND pt.RazorpayPaymentId IS NOT NULL
            WHERE 
                rp.Id = @RefundPaymentId
                AND rp.RefundStatus IN ('Failed', 'Pending', 'Retry')";

                // Use strongly typed DTO for better type safety
                var refundDetails = await connection.QuerySingleOrDefaultAsync<RetryRefundDto>(
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

                if (string.IsNullOrEmpty(refundDetails.RazorpayPaymentId))
                {
                    transaction.Rollback();
                    return new RetryRefundResult
                    {
                        Success = false,
                        Message = "Original payment not found. Cannot process refund without original payment reference."
                    };
                }

                _logger.LogInformation($"Retrying refund for payment: {refundDetails.RazorpayPaymentId}");

                // Safe conversion of PaymentType
                PaymentType paymentTypeEnum = (PaymentType)(refundDetails.PaymentType ?? 0);
                string paymentTypeStr = paymentTypeEnum.ToString();

                // Create notes for the refund
                var notes = new Dictionary<string, object>
        {
            { "tender_id", refundDetails.TenderIdString ?? "" },
            { "bidder_name", refundDetails.BidderName ?? "" },
            { "payment_type", paymentTypeStr },
            { "refund_reason", refundDetails.ReasonForRefund ?? "" },
            { "refund_payment_id", refundPaymentId.ToString() },
            { "retry_attempt", "true" },
            { "retried_by_user_id", userId.ToString() }
        };

                // Attempt to process refund through Razorpay
                var refundResponse = await _razorpay.ProcessRefund(
                    refundDetails.RazorpayPaymentId,
                    refundDetails.RefundAmount,
                    $"{paymentTypeStr} - {refundDetails.ReasonForRefund} (Retry)",
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

                    // Update payment link refund status
                    var updateLinkSql = @"
                UPDATE PaymentLinks 
                SET RefundStatus = 'Refunded',
                    RefundId = @RefundId,
                    RefundAmount = @RefundAmount,
                    RefundDate = GETUTCDATE()
                WHERE Id = @PaymentLinkId";

                    await connection.ExecuteAsync(updateLinkSql,
                        new
                        {
                            RefundId = refundResponse.RefundId,
                            RefundAmount = refundDetails.RefundAmount,
                            PaymentLinkId = refundDetails.PaymentLinkId
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
                    'REFUND',
                    @CustomerEmail,
                    GETUTCDATE(),
                    GETUTCDATE(),
                    @RefundReason
                )";

                    await connection.ExecuteAsync(insertTransactionSql, new
                    {
                        PaymentLinkId = $"REFUND_{refundDetails.LinkId}",
                        TenderId = refundDetails.TenderId,
                        TenderBidId = refundDetails.TenderBidId,
                        RazorpayRefundId = refundResponse.RefundId,
                        RazorpayOrderId = refundDetails.RazorpayOrderId,
                        Amount = -refundDetails.RefundAmount,
                        CustomerEmail = refundDetails.BidderEmail,
                        RefundReason = $"{refundDetails.ReasonForRefund} (Retry)"
                    }, transaction);

                    transaction.Commit();

                    _logger.LogInformation($"Refund retry successful. Refund ID: {refundResponse.RefundId}");

                    return new RetryRefundResult
                    {
                        Success = true,
                        Message = $"Refund processed successfully. Amount ₹{refundDetails.RefundAmount:N2} has been refunded.",
                        RazorpayRefundId = refundResponse.RefundId,
                        ProcessedAt = DateTime.UtcNow
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

                return new RetryRefundResult
                {
                    Success = false,
                    Message = $"An error occurred while retrying the refund: {ex.Message}"
                };
            }
        }

        public class RetryRefundDto
        {
            public int Id { get; set; }
            public int TenderBidId { get; set; }
            public int TenderId { get; set; }
            public int? PaymentType { get; set; }
            public int PaymentLinkId { get; set; }
            public decimal RefundAmount { get; set; }
            public string? ReasonForRefund { get; set; }
            public string? RefundStatus { get; set; }
            public string? LinkId { get; set; }
            public int? PaymentLinkPaymentType { get; set; }
            public string? RazorpayPaymentId { get; set; }
            public string? RazorpayOrderId { get; set; }
            public decimal? PaymentAmount { get; set; }
            public string? BidderName { get; set; }
            public string? BidderEmail { get; set; }
            public string? TenderIdString { get; set; }
        }

        public async Task<List<PaymentRefundSummary>> GetPaymentSummaryForBidAsync(int tenderBidId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    pl.Id as PaymentLinkId,
                    pl.TenderBidId,
                    pl.PaymentType,
                    CASE pl.PaymentType 
                        WHEN 1 THEN 'Bid Amount (EMD)'
                        WHEN 2 THEN 'Security Deposit'
                        WHEN 3 THEN 'Processing Fee'
                        ELSE 'Other'
                    END as PaymentTypeDisplay,
                    pl.Amount,
                    pl.LinkId,
                    pl.Status as PaymentStatus,
                    pt.RazorpayPaymentId,
                    pt.TransactionDate as PaymentDate,
                    rp.Id as RefundPaymentId,
                    rp.RefundStatus,
                    rp.RazorpayRefundId,
                    rp.InitiatedAt as RefundInitiatedDate,
                    rp.ApprovedAt as RefundApprovedDate,
                    rp.RefundErrorMessage,
                    rp.RefundAmount
                FROM PaymentLinks pl
                LEFT JOIN PaymentTransactions pt ON 
                    pt.PaymentLinkId = pl.LinkId 
                    AND pt.Status = 'Success'
                    AND pt.PaymentMethod != 'REFUND'
                LEFT JOIN RefundPayments rp ON 
                    rp.PaymentLinkId = pl.Id
                    AND rp.RefundStatus IN ('Pending', 'Approved', 'Failed')
                WHERE pl.TenderBidId = @TenderBidId 
                    AND pl.IsActive = 1
                ORDER BY pl.PaymentType";

            var results = await connection.QueryAsync<PaymentRefundSummary>(sql, new { TenderBidId = tenderBidId });
            return results.ToList();
        }

        public async Task<bool> HasPendingOrApprovedRefundAsync(int paymentLinkId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT COUNT(*) 
                FROM RefundPayments 
                WHERE PaymentLinkId = @PaymentLinkId 
                    AND RefundStatus IN ('Pending', 'Approved')";

            var count = await connection.QuerySingleAsync<int>(sql, new { PaymentLinkId = paymentLinkId });
            return count > 0;
        }

        public async Task<List<RefundHistoryItem>> GetRefundHistoryForPaymentAsync(int paymentLinkId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    rp.Id as RefundPaymentId,
                    rp.RefundStatus,
                    rp.RefundAmount,
                    rp.InitiatedAt,
                    u1.Username as InitiatedBy,
                    rp.ApprovedAt,
                    u2.Username as ApprovedBy,
                    rp.RazorpayRefundId,
                    rp.RefundErrorMessage as ErrorMessage,
                    rp.CheckerRemarks as Remarks
                FROM RefundPayments rp
                LEFT JOIN Users u1 ON rp.InitiatedBy = u1.Id
                LEFT JOIN Users u2 ON rp.ApprovedBy = u2.Id
                WHERE rp.PaymentLinkId = @PaymentLinkId
                ORDER BY rp.InitiatedAt DESC";

            var results = await connection.QueryAsync<RefundHistoryItem>(sql, new { PaymentLinkId = paymentLinkId });
            return results.ToList();
        }

        public async Task<RefundValidationResult> ValidateRefundRequestAsync(List<RefundRequestItem> refundItems)
        {
            var result = new RefundValidationResult { IsValid = true };

            using var connection = new SqlConnection(_connectionString);

            foreach (var item in refundItems)
            {
                var validationItem = new RefundValidationItem
                {
                    PaymentLinkId = item.PaymentLinkId,
                    PaymentType = item.PaymentType,
                    IsValid = true
                };

                // Check if payment link exists and is paid
                var paymentCheckSql = @"
                    SELECT pl.Status, pl.Amount, tb.Status as BidStatus
                    FROM PaymentLinks pl
                    INNER JOIN TenderBids tb ON pl.TenderBidId = tb.Id
                    WHERE pl.Id = @PaymentLinkId";

                var payment = await connection.QueryFirstOrDefaultAsync<dynamic>(paymentCheckSql, 
                    new { PaymentLinkId = item.PaymentLinkId });

                if (payment == null)
                {
                    validationItem.IsValid = false;
                    validationItem.ErrorReason = "Payment link not found";
                }
                else if (payment.Status != 2) // Not Used/Paid
                {
                    validationItem.IsValid = false;
                    validationItem.ErrorReason = "Payment not completed";
                }
                else if (payment.BidStatus != "Rejected")
                {
                    validationItem.IsValid = false;
                    validationItem.ErrorReason = "Bid must be rejected to initiate refund";
                }
                else
                {
                    // Check for existing refund
                    if (await HasPendingOrApprovedRefundAsync(item.PaymentLinkId))
                    {
                        validationItem.IsValid = false;
                        validationItem.ErrorReason = "Refund already exists for this payment";
                    }
                }

                if (!validationItem.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Payment {item.PaymentType}: {validationItem.ErrorReason}");
                }

                result.ValidationItems.Add(validationItem);
            }

            return result;
        }

        public async Task<int> GetPendingRefundCountAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = "SELECT COUNT(*) FROM RefundPayments WHERE RefundStatus = 'Pending'";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<int> GetFailedRefundCountAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = "SELECT COUNT(*) FROM RefundPayments WHERE RefundStatus = 'Failed'";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<BulkRetryResult> BulkRetryFailedRefundsAsync(List<int> refundPaymentIds, int userId)
        {
            var result = new BulkRetryResult
            {
                TotalAttempted = refundPaymentIds.Count,
                Results = new List<IndividualRetryResult>()
            };

            foreach (var refundId in refundPaymentIds)
            {
                var retryResult = await RetryFailedRefundAsync(refundId, userId);
                
                var individualResult = new IndividualRetryResult
                {
                    RefundPaymentId = refundId,
                    Success = retryResult.Success,
                    RazorpayRefundId = retryResult.RazorpayRefundId,
                    ErrorMessage = retryResult.Success ? null : retryResult.Message
                };

                if (retryResult.Success)
                    result.SuccessfulCount++;
                else
                    result.FailedCount++;

                result.Results.Add(individualResult);
            }

            return result;
        }

        public async Task<RefundExportData> GetRefundExportDataAsync(RefundSearchFilter filter)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    t.TenderId,
                    t.TenderTitle,
                    tb.BidderName,
                    tb.CompanyName,
                    CASE rp.PaymentType 
                        WHEN 1 THEN 'Bid Amount (EMD)'
                        WHEN 2 THEN 'Security Deposit'
                        WHEN 3 THEN 'Processing Fee'
                        ELSE 'Other'
                    END as PaymentType,
                    pl.Amount as PaymentAmount,
                    pt.RazorpayPaymentId as PaymentId,
                    pt.TransactionDate as PaymentDate,
                    rp.RefundStatus,
                    rp.RefundAmount,
                    rp.RazorpayRefundId as RefundId,
                    rp.InitiatedAt as RefundInitiatedDate,
                    rp.ApprovedAt as RefundApprovedDate,
                    u1.Username as InitiatedBy,
                    u2.Username as ApprovedBy,
                    rp.ReasonForRefund as RefundReason,
                    rp.RefundErrorMessage as ErrorMessage
                FROM RefundPayments rp
                INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
                INNER JOIN Tenders t ON rp.TenderId = t.Id
                INNER JOIN PaymentLinks pl ON rp.PaymentLinkId = pl.Id
                LEFT JOIN PaymentTransactions pt ON pt.PaymentLinkId = pl.LinkId AND pt.Status = 'Success'
                LEFT JOIN Users u1 ON rp.InitiatedBy = u1.Id
                LEFT JOIN Users u2 ON rp.ApprovedBy = u2.Id
                WHERE 1=1
                    AND (@TenderId IS NULL OR t.TenderId LIKE '%' + @TenderId + '%')
                    AND (@TenderName IS NULL OR t.TenderTitle LIKE '%' + @TenderName + '%')
                    AND (@PaymentType IS NULL OR rp.PaymentType = @PaymentType)
                    AND (@RefundStatus IS NULL OR rp.RefundStatus = @RefundStatus)
                    AND (@FromDate IS NULL OR rp.InitiatedAt >= @FromDate)
                    AND (@ToDate IS NULL OR rp.InitiatedAt <= @ToDate)
                ORDER BY rp.InitiatedAt DESC";

            var parameters = new
            {
                filter.TenderId,
                filter.TenderName,
                PaymentType = filter.PaymentType.HasValue ? (int)filter.PaymentType.Value : (int?)null,
                filter.RefundStatus,
                filter.FromDate,
                filter.ToDate
            };

            var rows = await connection.QueryAsync<RefundExportRow>(sql, parameters);

            var exportData = new RefundExportData
            {
                Rows = rows.ToList(),
                GeneratedAt = DateTime.UtcNow,
                Summary = new RefundExportSummary
                {
                    TotalRefunds = rows.Count(),
                    TotalRefundAmount = rows.Sum(r => r.RefundAmount),
                    RefundsByStatus = rows.GroupBy(r => r.RefundStatus)
                        .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                    AmountByPaymentType = rows.GroupBy(r => r.PaymentType)
                        .ToDictionary(
                            g => Enum.Parse<PaymentType>(g.Key.Replace(" ", "").Replace("(", "").Replace(")", "")), 
                            g => g.Sum(r => r.RefundAmount))
                }
            };

            return exportData;
        }
    }

    // Custom Exception for partial success scenarios
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
}