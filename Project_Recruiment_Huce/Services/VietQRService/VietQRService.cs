using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Project_Recruiment_Huce.Models.Payment;

namespace Project_Recruiment_Huce.Services.VietQRService
{
    /// <summary>
    /// Service to generate VietQR using VietQR.io API
    /// Documentation: https://www.vietqr.io/danh-sach-api/link-tao-ma-nhanh/api-tao-ma-qr
    /// </summary>
    public class VietQRService : IVietQRService
    {
        private static readonly string API_URL = "https://api.vietqr.io/v2/generate";
        private static readonly string CLIENT_ID = ConfigurationManager.AppSettings["VietQR:ClientId"] ?? "";
        private static readonly string API_KEY = ConfigurationManager.AppSettings["VietQR:ApiKey"] ?? "";
        
        // Bank configuration
        private static readonly string ACCOUNT_NO = ConfigurationManager.AppSettings["Payment:AccountNumber"] ?? "0859226688";
        private static readonly string ACCOUNT_NAME = ConfigurationManager.AppSettings["Payment:AccountName"] ?? "BUI DAI PHU";
        private static readonly int ACQ_ID = int.Parse(ConfigurationManager.AppSettings["Payment:BankBIN"] ?? "970422"); // MB Bank BIN

        /// <summary>
        /// Generates a VietQR code asynchronously via API
        /// </summary>
        /// <param name="amount">Transaction amount in VND</param>
        /// <param name="content">Transaction description/content</param>
        /// <param name="timeoutSeconds">API call timeout (default: 5 seconds)</param>
        /// <returns>Base64 QR code data URL or fallback URL on error</returns>
        public async Task<string> GenerateQRAsync(int amount, string content, int timeoutSeconds = 5)
        {
            try
            {
                var requestBody = new VietQRRequest
                {
                    accountNo = ACCOUNT_NO,
                    accountName = ACCOUNT_NAME,
                    acqId = ACQ_ID,
                    amount = amount,
                    addInfo = content, // Max 25 characters
                    format = "text",
                    template = "print"
                };

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                    // Add authentication headers if configured
                    if (!string.IsNullOrEmpty(CLIENT_ID))
                        client.DefaultRequestHeaders.Add("x-client-id", CLIENT_ID);
                    if (!string.IsNullOrEmpty(API_KEY))
                        client.DefaultRequestHeaders.Add("x-api-key", API_KEY);

                    var json = JsonConvert.SerializeObject(requestBody);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(API_URL, httpContent);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Helpers.PaymentLogger.Error($"VietQR API error: {response.StatusCode} - {responseContent}");
                        return GenerateFallbackQRUrl(amount, content);
                    }

                    var result = JsonConvert.DeserializeObject<VietQRResponse>(responseContent);
                    
                    if (result?.code == "00" && result?.data?.qrDataURL != null)
                    {
                        Helpers.PaymentLogger.Info($"VietQR generated successfully. Amount: {amount}, Content: {content}");
                        return result.data.qrDataURL; // Return Base64 data URL
                    }
                    else
                    {
                        Helpers.PaymentLogger.Warning($"VietQR generation failed. Code: {result?.code}, Desc: {result?.desc}");
                        return GenerateFallbackQRUrl(amount, content);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Helpers.PaymentLogger.Warning($"VietQR API timeout after {timeoutSeconds}s. Using fallback URL.");
                return GenerateFallbackQRUrl(amount, content);
            }
            catch (Exception ex)
            {
                Helpers.PaymentLogger.Error("Error generating VietQR", ex);
                return GenerateFallbackQRUrl(amount, content);
            }
        }

        /// <summary>
        /// Generates a VietQR code synchronously (wrapper for async method)
        /// </summary>
        /// <param name="amount">Transaction amount in VND</param>
        /// <param name="content">Transaction description/content</param>
        /// <returns>Base64 QR code data URL or fallback URL on error</returns>
        public string GenerateQR(int amount, string content)
        {
            return GenerateQRAsync(amount, content).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generates a fallback QR code URL when API fails or times out
        /// </summary>
        /// <param name="amount">Transaction amount in VND</param>
        /// <param name="content">Transaction description/content</param>
        /// <param name="bankCode">Bank code (default: 970422 for MBBank)</param>
        /// <returns>VietQR.io direct image URL</returns>
        public string GenerateFallbackQRUrl(int amount, string content, string bankCode = "970422")
        {
            // VietQR Quick Link format (no authentication required)
            var template = "compact2";
            
            // URL encode content
            var encodedContent = Uri.EscapeDataString(content);
            
            return $"https://img.vietqr.io/image/{bankCode}-{ACCOUNT_NO}-{template}.png?amount={amount}&addInfo={encodedContent}&accountName={Uri.EscapeDataString(ACCOUNT_NAME)}";
        }
    }
}
