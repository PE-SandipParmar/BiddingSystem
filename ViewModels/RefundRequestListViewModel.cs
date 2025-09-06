using BiddingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiddingSystem.ViewModels
{
    public class RefundRequestListViewModel
    {
        public List<RefundRequestViewModel> RefundRequests { get; set; } = new();
        
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        // Statistics
        public Dictionary<RefundStatus, int> StatusCounts { get; set; } = new();
        public Dictionary<RefundType, int> TypeCounts { get; set; } = new();
        public Dictionary<RefundReason, int> ReasonCounts { get; set; } = new();
        
        // Filters
        public string SearchTerm { get; set; } = string.Empty;
        public RefundStatus? Status { get; set; }
        public RefundType? Type { get; set; }
        public RefundReason? Reason { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        // Dropdown options
        public List<SelectListItem> AvailableStatuses { get; set; } = new();
        public List<SelectListItem> AvailableTypes { get; set; } = new();
        public List<SelectListItem> AvailableReasons { get; set; } = new();
    }
}
