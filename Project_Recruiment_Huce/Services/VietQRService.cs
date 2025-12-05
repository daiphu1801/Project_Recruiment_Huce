using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Service to generate VietQR using VietQR.io API
    /// Documentation: https://www.vietqr.io/danh-sach-api/link-tao-ma-nhanh/api-tao-ma-qr
    /// </summary>
    public class VietQRService
    {
        private static readonly string API_URL = "https://api.vietqr.io/v2/generate";
        private static readonly string CLIENT_ID = ConfigurationManager.AppSettings["VietQR:ClientId"] ?? "";
        private static readonly string API_KEY = ConfigurationManager.AppSettings["VietQR:ApiKey"] ?? "";
        
        // Bank configuration
        private static readonly string ACCOUNT_NO = ConfigurationManager.AppSettings["Payment:AccountNumber"] ?? "0859226688";
        private static readonly string ACCOUNT_NAME = ConfigurationManager.AppSettings["Payment:AccountName"] ?? "BUI DAI PHU";
        private static readonly int ACQ_ID = int.Parse(ConfigurationManager.AppSettings["Payment:BankBIN"] ?? "970422"); // MB Bank BIN

        /// <summary>
        /// Generate VietQR code with transaction info
        /// </summary>
        public async Task<VietQRResponse> GenerateQRAsync(decimal amount, string content, string template = "print")
        {
            try
            {
                var requestBody = new VietQRRequest
                {
                    accountNo = ACCOUNT_NO,
                    accountName = ACCOUNT_NAME,
                    acqId = ACQ_ID,
                    amount = (int)amount,
                    addInfo = content, // Max 25 characters
                    format = "text",
                    template = template
                };

                using (var client = new HttpClient())
                {
                    // Add authentication headers
                    client.DefaultRequestHeaders.Add("x-client-id", CLIENT_ID);
                    client.DefaultRequestHeaders.Add("x-api-key", API_KEY);

                    var json = JsonConvert.SerializeObject(requestBody);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(API_URL, httpContent);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Helpers.PaymentLogger.Error($"VietQR API error: {response.StatusCode} - {responseContent}");
                        return null;
                    }

                    var result = JsonConvert.DeserializeObject<VietQRResponse>(responseContent);
                    
                    if (result?.code == "00")
                    {
                        Helpers.PaymentLogger.Info($"VietQR generated successfully. Amount: {amount}, Content: {content}");
                        return result;
                    }
                    else
                    {
                        Helpers.PaymentLogger.Warning($"VietQR generation failed. Code: {result?.code}, Desc: {result?.desc}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.PaymentLogger.Error("Error generating VietQR", ex);
                return null;
            }
        }

        /// <summary>
        /// Generate QR synchronously (for compatibility)
        /// </summary>
        public VietQRResponse GenerateQR(decimal amount, string content, string template = "compact2")
        {
            return GenerateQRAsync(amount, content, template).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generate fallback QR URL (simple link-based, no API call)
        /// Use when VietQR API is unavailable or for development
        /// </summary>
        public string GenerateFallbackQRUrl(decimal amount, string content)
        {
            // VietQR Quick Link format (no authentication required)
            var bankCode = ConfigurationManager.AppSettings["Payment:BankCode"] ?? "MB";
            var template = "compact2";
            
            // URL encode content
            var encodedContent = Uri.EscapeDataString(content);
            
            return $"https://img.vietqr.io/image/{bankCode}-{ACCOUNT_NO}-{template}.png?amount={amount}&addInfo={encodedContent}&accountName={Uri.EscapeDataString(ACCOUNT_NAME)}";
        }
    }

    #region Models

    public class VietQRRequest
    {
        public string accountNo { get; set; }
        public string accountName { get; set; }
        public int acqId { get; set; }
        public int amount { get; set; }
        public string addInfo { get; set; }
        public string format { get; set; } = "text";
        public string template { get; set; } = "print";
    }

    public class VietQRResponse
    {
        public string code { get; set; }
        public string desc { get; set; }
        public VietQRData data { get; set; }
    }

    public class VietQRData
    {
        public int acqId { get; set; }
        public string accountName { get; set; }
        public string qrCode { get; set; }
        public string qrDataURL { get; set; } // Base64 image data URL
    }

    #endregion
}
