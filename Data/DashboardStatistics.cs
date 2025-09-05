namespace BiddingSystem.Models
{
    public class DashboardStatistics
    {
        public decimal TotalExpenditure { get; set; }
        public int ActiveLeases { get; set; }
        public int TotalEmployeesOnLease { get; set; }
        public int TotalVendors { get; set; }
        public decimal MonthlyLeaseToPay { get; set; }
        public decimal SecurityDepositPaid { get; set; }
        public decimal BrokeragePaid { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int FinancialYear { get; set; }
    }

    public class MonthlyTrendData
    {
        public string MonthName { get; set; }
        public decimal Amount { get; set; }
    }

    public class LeasePaymentStatusData
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public string Color { get; set; }
    }

    public class DepartmentLeaseDistribution
    {
        public string DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
        public decimal TotalRent { get; set; }
    }

    public class TopVendorData
    {
        public string VendorName { get; set; }
        public int LeaseCount { get; set; }
        public decimal TotalMonthlyRent { get; set; }
        public decimal TotalDeposit { get; set; }
    }

    public class LeaseExpiryAlert
    {
        public int Id { get; set; }
        public string RefNo { get; set; }
        public string EmployeeName { get; set; }
        public string VendorName { get; set; }
        public decimal MonthlyRentPayable { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class PaymentSummary
    {
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public string Color { get; set; }
    }

    public class RecentActivity
    {
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Status { get; set; }
    }

    public class DashboardViewModel
    {
        public DashboardStatistics Statistics { get; set; }
        public List<MonthlyTrendData> MonthlyTrend { get; set; }
        public List<LeasePaymentStatusData> LeasePaymentStatus { get; set; }
        public List<DepartmentLeaseDistribution> DepartmentDistribution { get; set; }
        public List<TopVendorData> TopVendors { get; set; }
        public List<LeaseExpiryAlert> ExpiringLeases { get; set; }
        public List<PaymentSummary> PaymentSummary { get; set; }
        public List<RecentActivity> RecentActivities { get; set; }
    }
}
