namespace BiddingSystem.Data
{
    public interface ISmsService
    {
        Task<bool> SendPaymentLinkSmsAsync(string mobileNumber, string paymentUrl, string bidderName = "");
    }
}
