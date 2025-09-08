namespace BiddingSystem.Models
{
    public class MonthlyTrends
    {
        public List<string> Months { get; set; } = new();
        public List<int> TenderCounts { get; set; } = new();
        public List<int> BidCounts { get; set; } = new();
        public List<decimal> TotalBidAmounts { get; set; } = new();
        public List<decimal> TotalEMDSDAmounts { get; set; } = new();
        public List<decimal> TotalCollectedEMDs { get; set; } = new();
    }
}
