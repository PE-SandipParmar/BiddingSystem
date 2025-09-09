using BiddingSystem.Models;
using Microsoft.Extensions.Logging;

namespace BiddingSystem.Data
{
    public class TenderAllocationService : ITenderAllocationService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TenderAllocationService> _logger;

        public TenderAllocationService(
            ITenderRepository tenderRepository,
            ITenderBidRepository tenderBidRepository,
            IUserRepository userRepository,
            ILogger<TenderAllocationService> logger)
        {
            _tenderRepository = tenderRepository;
            _tenderBidRepository = tenderBidRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<AllocationValidationResult> ValidateAllocationAsync(int tenderId, int bidId)
        {
            var result = new AllocationValidationResult();

            try
            {
                // Get tender details
                var tender = await _tenderRepository.GetByIdAsync(tenderId);
                if (tender == null)
                {
                    result.Errors.Add("Tender not found.");
                    return result;
                }

                // Get bid details
                var bid = await _tenderBidRepository.GetBidByIdAsync(bidId);
                if (bid == null)
                {
                    result.Errors.Add("Bid not found.");
                    return result;
                }

                // Validate tender status
                if (tender.Status != TenderStatus.Published)
                {
                    result.Errors.Add("Tender must be in Published status to be allocated.");
                }

                // Check if tender is already allocated
                if (tender.IsAllocated)
                {
                    result.Errors.Add("Tender is already allocated to another bid.");
                }

                // Validate bid belongs to this tender
                if (bid.TenderId != tenderId)
                {
                    result.Errors.Add("Bid does not belong to this tender.");
                }

                // Check if bid is already allocated
                if (bid.IsAllocated)
                {
                    result.Errors.Add("Bid is already allocated to another tender.");
                }

                // Check minimum bid requirement (1 bid)
                var allBids = await _tenderBidRepository.GetBidsByTenderIdAsync(tenderId);
                if (allBids.Count() < 1)
                {
                    result.Errors.Add("Tender must have at least 1 bid before allocation.");
                }

                // Check if bid has paid EMD
                if (bid.PaymentStatus != "Paid")
                {
                    result.Errors.Add("Bid must have paid EMD before allocation.");
                }

                // Check if bid is in valid status
                if (bid.Status != "Submitted" && bid.Status != "Under Review")
                {
                    result.Errors.Add("Bid must be in Submitted or Under Review status for allocation.");
                }

                // Add warnings for review
                if (bid.Status == "Under Review")
                {
                    result.Warnings.Add("Bid is currently under review. Please ensure this is the intended allocation.");
                }

                result.IsValid = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating allocation for tender {TenderId} and bid {BidId}", tenderId, bidId);
                result.Errors.Add("An error occurred while validating the allocation.");
            }

            return result;
        }

        public async Task<AllocationResult> AllocateTenderAsync(int tenderId, int bidId, int allocatedBy, string? remarks = null)
        {
            try
            {
                // Validate allocation first
                var validation = await ValidateAllocationAsync(tenderId, bidId);
                if (!validation.IsValid)
                {
                    return AllocationResult.Failure(validation.Errors.ToArray());
                }

                // Get current user
                var user = await _userRepository.GetByIdAsync(allocatedBy);
                if (user == null)
                {
                    return AllocationResult.Failure("User performing allocation not found.");
                }

                // Get tender and bid
                var tender = await _tenderRepository.GetByIdAsync(tenderId);
                var bid = await _tenderBidRepository.GetBidByIdAsync(bidId);

                if (tender == null || bid == null)
                {
                    return AllocationResult.Failure("Tender or bid not found.");
                }

                // Perform allocation in a transaction-like manner
                var allocationDate = DateTime.UtcNow;

                // Update tender
                tender.AllocatedBidId = bidId;
                tender.AllocatedAt = allocationDate;
                tender.AllocatedBy = allocatedBy;
                tender.AllocationRemarks = remarks;
                tender.UpdatedAt = allocationDate;

                // Update bid
                bid.IsAllocated = true;
                bid.AllocationDate = allocationDate;
                bid.AllocationRemarks = remarks;
                bid.UpdatedAt = allocationDate;
                bid.Status = "Accepted"; // Change bid status to Accepted when allocated

                // Save changes
                await _tenderRepository.UpdateAsync(tender);
                await _tenderBidRepository.UpdateBidAsync(bid);

                _logger.LogInformation("Tender {TenderId} allocated to bid {BidId} by user {UserId}", 
                    tenderId, bidId, allocatedBy);

                return AllocationResult.Success(
                    $"Tender '{tender.TenderTitle}' has been successfully allocated to bid from '{bid.BidderName}'.",
                    tender,
                    bid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating tender {TenderId} to bid {BidId}", tenderId, bidId);
                return AllocationResult.Failure("An error occurred while allocating the tender.");
            }
        }

        public async Task<AllocationResult> DeallocateTenderAsync(int tenderId, int deallocatedBy, string? remarks = null)
        {
            try
            {
                // Get tender
                var tender = await _tenderRepository.GetByIdAsync(tenderId);
                if (tender == null)
                {
                    return AllocationResult.Failure("Tender not found.");
                }

                if (!tender.IsAllocated)
                {
                    return AllocationResult.Failure("Tender is not currently allocated.");
                }

                // Get allocated bid
                var allocatedBid = await _tenderBidRepository.GetBidByIdAsync(tender.AllocatedBidId!.Value);
                if (allocatedBid == null)
                {
                    return AllocationResult.Failure("Allocated bid not found.");
                }

                // Get current user
                var user = await _userRepository.GetByIdAsync(deallocatedBy);
                if (user == null)
                {
                    return AllocationResult.Failure("User performing deallocation not found.");
                }

                var deallocationDate = DateTime.UtcNow;

                // Update tender
                tender.AllocatedBidId = null;
                tender.AllocatedAt = null;
                tender.AllocatedBy = null;
                tender.AllocationRemarks = remarks;
                tender.UpdatedAt = deallocationDate;

                // Update bid
                allocatedBid.IsAllocated = false;
                allocatedBid.AllocationDate = null;
                allocatedBid.AllocationRemarks = remarks;
                allocatedBid.UpdatedAt = deallocationDate;
                allocatedBid.Status = "Under Review"; // Revert bid status

                // Save changes
                await _tenderRepository.UpdateAsync(tender);
                await _tenderBidRepository.UpdateBidAsync(allocatedBid);

                _logger.LogInformation("Tender {TenderId} deallocated from bid {BidId} by user {UserId}", 
                    tenderId, allocatedBid.Id, deallocatedBy);

                return AllocationResult.Success(
                    $"Tender '{tender.TenderTitle}' has been successfully deallocated from bid by '{allocatedBid.BidderName}'.",
                    tender);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deallocating tender {TenderId}", tenderId);
                return AllocationResult.Failure("An error occurred while deallocating the tender.");
            }
        }

        public async Task<List<TenderBid>> GetEligibleBidsAsync(int tenderId)
        {
            try
            {
                var allBids = await _tenderBidRepository.GetBidsByTenderIdAsync(tenderId);
                
                // Filter eligible bids
                return allBids.Where(bid => 
                    !bid.IsAllocated && 
                    bid.PaymentStatus == "Paid" && 
                    (bid.Status == "Submitted" || bid.Status == "Under Review")
                ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting eligible bids for tender {TenderId}", tenderId);
                return new List<TenderBid>();
            }
        }

        public async Task<AllocationHistory?> GetAllocationHistoryAsync(int tenderId)
        {
            try
            {
                var tender = await _tenderRepository.GetByIdAsync(tenderId);
                if (tender == null)
                {
                    return null;
                }

                var history = new AllocationHistory
                {
                    TenderId = tender.Id,
                    TenderTitle = tender.TenderTitle,
                    AllocatedBidId = tender.AllocatedBidId,
                    AllocatedAt = tender.AllocatedAt,
                    AllocatedBy = tender.AllocatedBy,
                    AllocationRemarks = tender.AllocationRemarks,
                    IsCurrentlyAllocated = tender.IsAllocated
                };

                if (tender.AllocatedBidId.HasValue)
                {
                    var allocatedBid = await _tenderBidRepository.GetBidByIdAsync(tender.AllocatedBidId.Value);
                    if (allocatedBid != null)
                    {
                        history.AllocatedBidderName = allocatedBid.BidderName;
                    }
                }

                if (tender.AllocatedBy.HasValue)
                {
                    var allocatedByUser = await _userRepository.GetByIdAsync(tender.AllocatedBy.Value);
                    if (allocatedByUser != null)
                    {
                        history.AllocatedByUserName = allocatedByUser.Username;
                    }
                }

                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting allocation history for tender {TenderId}", tenderId);
                return null;
            }
        }
    }
}
