using BiddingSystem.Models;

namespace BiddingSystem.ViewModels
{
    public class RefundRequestDetailsViewModel
    {
        public RefundRequestViewModel RefundRequest { get; set; } = new();
        public List<RefundTransactionViewModel> Transactions { get; set; } = new();
        
        // Permission flags
        public bool CanEdit { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanProcess { get; set; }
        public bool CanComplete { get; set; }
        public bool CanFail { get; set; }
        public bool CanDelete { get; set; }
        
        // Checker-Maker Workflow Permission flags
        public bool CanSubmitForFirstCheck { get; set; }
        public bool CanFirstCheck { get; set; }
        public bool CanSubmitForSecondCheck { get; set; }
        public bool CanSecondCheck { get; set; }
        public bool CanMarkReadyForProcessing { get; set; }
    }
}
