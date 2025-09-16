using Microsoft.AspNetCore.Routing;
using System.Net;
using System.Text;

namespace BiddingSystem.Data
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }
        public static void SendSms(string mobileNo, string message, string route = "33")
        {
            try
            {
                // Encode message text to make it URL safe
                string encodedMessage = Uri.EscapeDataString(message);

                string smsurl = "http://182.18.162.128/api/mt/SendSMS" +
                                "?user=niraj.rathod" +
                                "&password=123456" +
                                "&senderid=ALCOPL" +
                                "&channel=trans" +
                                "&DCS=0" +
                                "&flashsms=0" +
                                "&number=" + mobileNo +
                                "&text=" + encodedMessage +
                                "&route=" + route;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(smsurl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                using (Stream s = response.GetResponseStream())
                using (StreamReader readStream = new StreamReader(s))
                {
                    string dataString = readStream.ReadToEnd();
                    Console.WriteLine("SMS API Response: " + dataString);
                }

                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending SMS: " + ex.Message);
            }
        }
        public async Task<bool> SendPaymentLinkSmsAsync(string mobileNumber, string paymentUrl, string bidderName = "")
        {
            try
            {

                // SendSms(mobileNumber, paymentUrl, bidderName); // Commented out - using the main method below
                if (string.IsNullOrWhiteSpace(mobileNumber) || string.IsNullOrWhiteSpace(paymentUrl))
                {
                    _logger.LogWarning("Mobile number or payment URL is empty");
                    return false;
                }

                // Clean mobile number (remove any non-digit characters)
                var cleanMobile = new string(mobileNumber.Where(char.IsDigit).ToArray());
                
                // Ensure it's a valid Indian mobile number
                if (cleanMobile.Length != 10 || !cleanMobile.StartsWith("6") && !cleanMobile.StartsWith("7") && 
                    !cleanMobile.StartsWith("8") && !cleanMobile.StartsWith("9"))
                {
                    _logger.LogWarning($"Invalid mobile number format: {mobileNumber}");
                    return false;
                }

                var displayName = !string.IsNullOrEmpty(bidderName) ? bidderName : "Customer";
                
                // Clean the display name to avoid special characters
                var cleanName = CleanNameForSms(displayName);
                
                // Create the simplest possible message that should work with the SMS API
                var message = $"Dear {cleanName} your payment link is ready Please contact us for details Team Alight Consultants";
                string encodedMessage = Uri.EscapeDataString(message);

                // Build SMS URL exactly as provided
                //var smsUrl = $"http://182.18.162.128/api/mt/SendSMS?user=niraj.rathod&password=123456&senderid=ALCOPL&channel=trans&DCS=0&flashsms=0&number={cleanMobile}&text={Uri.EscapeDataString(message)}&route=33";
                string smsurl = "http://182.18.162.128/api/mt/SendSMS" +
                            "?user=" + Uri.EscapeDataString("niraj.rathod") +
                            "&password=" + Uri.EscapeDataString("123456") +
                            "&senderid=" + Uri.EscapeDataString("ALCOPL") +
                            "&channel=trans" +
                            "&DCS=0" +
                            "&flashsms=0" +
                            "&number=" + Uri.EscapeDataString(cleanMobile) +
                            "&text=" + encodedMessage +
                            "&route=33";
                _logger.LogInformation($"Sending SMS to {cleanMobile}: {message}");

                // Send SMS using WebRequest (as in your example)
                var request = WebRequest.Create(smsurl);
                var response = await request.GetResponseAsync();
                
                using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream);
                var responseData = await reader.ReadToEndAsync();
                
                response.Close();
                
                _logger.LogInformation($"SMS API Response: {responseData}");
                
                // Check if response indicates success (you may need to adjust this based on actual API response)
                return !responseData.Contains("error") && !responseData.Contains("ErrorCode");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SMS to {mobileNumber}: {ex.Message}");
                return false;
            }
        }

        private static string CleanNameForSms(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Customer";

            // Only clean the name, not the entire message
            return name
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("!", "")
                .Replace("@", "at")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("*", "")
                .Replace("+", "")
                .Replace("=", "")
                .Replace("|", "")
                .Replace("\\", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("?", "")
                .Replace("/", "")
                .Replace("~", "")
                .Replace("`", "")
                .Trim()
                .Replace("  ", " ");
        }

        private static string CreateShortUrl(string originalUrl)
        {
            try
            {
                // Extract just the domain and a short identifier for SMS
                var uri = new Uri(originalUrl);
                var domain = uri.Host;
                
                // Get the token parameter if it exists
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var token = query["token"];
                
                if (!string.IsNullOrEmpty(token) && token.Length > 8)
                {
                    // Use first 8 characters of token for brevity
                    token = token.Substring(0, 8);
                }
                
                // Create a shorter URL format
                return $"http://{domain}/pay/{token}";
            }
            catch
            {
                // If URL parsing fails, return a simple message
                return "Visit our website for payment link";
            }
        }

        private static string CleanMessageForSms(string message)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            // Clean the entire message to ensure SMS API compatibility
            return message
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("!", "")
                .Replace("@", "at")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("*", "")
                .Replace("+", "")
                .Replace("=", "")
                .Replace("|", "")
                .Replace("\\", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("?", "")
                .Replace("/", "")
                .Replace("~", "")
                .Replace("`", "")
                .Trim()
                .Replace("  ", " ");
        }
    }
}
