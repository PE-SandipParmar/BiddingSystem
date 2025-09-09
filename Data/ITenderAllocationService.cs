using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface ITenderAllocationService
    {
        /// <summary>
        /// Validates if a tender can be allocated to a specific bid
        /// </summary>
        /// <param name="tenderId">The tender ID</param>
        /// <param name="bidId">The bid ID</param>
        /// <returns>Validation result with success status and error messages</returns>
        Task<AllocationValidationResult> ValidateAllocationAsync(int tenderId, int bidId);

        /// <summary>
        /// Allocates a tender to a specific bid
        /// </summary>
        /// <param name="tenderId">The tender ID</param>
        /// <param name="bidId">The bid ID to allocate to</param>
        /// <param name="allocatedBy">User ID who is performing the allocation</param>
        /// <param name="remarks">Optional remarks about the allocation</param>
        /// <returns>Allocation result with success status and details</returns>
        Task<AllocationResult> AllocateTenderAsync(int tenderId, int bidId, int allocatedBy, string? remarks = null);

        /// <summary>
        /// Deallocates a tender (removes allocation)
        /// </summary>
        /// <param name="tenderId">The tender ID</param>
        /// <param name="deallocatedBy">User ID who is performing the deallocation</param>
        /// <param name="remarks">Optional remarks about the deallocation</param>
        /// <returns>Deallocation result with success status and details</returns>
        Task<AllocationResult> DeallocateTenderAsync(int tenderId, int deallocatedBy, string? remarks = null);

        /// <summary>
        /// Gets all bids for a tender that are eligible for allocation
        /// </summary>
        /// <param name="tenderId">The tender ID</param>
        /// <returns>List of eligible bids</returns>
        Task<List<TenderBid>> GetEligibleBidsAsync(int tenderId);

        /// <summary>
        /// Gets allocation history for a tender
        /// </summary>
        /// <param name="tenderId">The tender ID</param>
        /// <returns>Allocation history details</returns>
        Task<AllocationHistory?> GetAllocationHistoryAsync(int tenderId);
    }

    public class AllocationValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        public static AllocationValidationResult Success() => new() { IsValid = true };
        public static AllocationValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
    }

    public class AllocationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public Tender? Tender { get; set; }
        public TenderBid? AllocatedBid { get; set; }

        public static AllocationResult Success(string message, Tender tender, TenderBid? allocatedBid = null) => 
            new() { IsSuccess = true, Message = message, Tender = tender, AllocatedBid = allocatedBid };
        
        public static AllocationResult Failure(params string[] errors) => 
            new() { IsSuccess = false, Errors = errors.ToList() };
    }

    public class AllocationHistory
    {
        public int TenderId { get; set; }
        public string TenderTitle { get; set; } = string.Empty;
        public int? AllocatedBidId { get; set; }
        public string? AllocatedBidderName { get; set; }
        public DateTime? AllocatedAt { get; set; }
        public int? AllocatedBy { get; set; }
        public string? AllocatedByUserName { get; set; }
        public string? AllocationRemarks { get; set; }
        public bool IsCurrentlyAllocated { get; set; }
    }
}
