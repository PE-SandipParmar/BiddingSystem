namespace BiddingSystem.Models
{
    public class DashboardStatistics
    {
        public int TotalTenders { get; set; }
        public int TotalBids { get; set; }
        public int ActiveTenders { get; set; }
        public int SubmittedBids { get; set; }
        public int PendingRefunds { get; set; }
        public int ProcessedRefunds { get; set; }
        public decimal TotalBidAmount { get; set; }
        public decimal TotalEMDSDAmount { get; set; }
        public decimal TotalSDAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
    }

    public class MonthlyTrendData
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class LeasePaymentStatusData
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class DepartmentLeaseDistribution
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal TotalRent { get; set; }
    }

    public class TopVendorData
    {
        public string VendorName { get; set; } = string.Empty;
        public int LeaseCount { get; set; }
        public decimal TotalMonthlyRent { get; set; }
        public decimal TotalDeposit { get; set; }
    }

    public class LeaseExpiryAlert
    {
        public int Id { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public decimal MonthlyRentPayable { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class PaymentSummary
    {
        public string PaymentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class RecentActivity
    {
        public int Id { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

}
