using Project_Recruiment_Huce.Models.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Recruiment_Huce.Services.VietQRService
{
    /// <summary>
    /// Interface for VietQR code generation service
    /// </summary>
    public interface IVietQRService
    {
        /// <summary>
        /// Generates a VietQR code asynchronously via API
        /// </summary>
        /// <param name="amount">Transaction amount in VND</param>
        /// <param name="content">Transaction description/content</param>
        /// <param name="timeoutSeconds">API call timeout (default: 5 seconds)</param>
        /// <returns>Base64 QR code data URL or fallback URL on error</returns>
        Task<string> GenerateQRAsync(int amount, string content, int timeoutSeconds = 5);

        /// <summary>
        /// Generates a VietQR code synchronously (wrapper for async method)
        /// </summary>
        /// <param name="amount">Transaction amount in VND</param>
        /// <param name="content">Transaction description/content</param>
        /// <returns>Base64 QR code data URL or fallback URL on error</returns>
        string GenerateQR(int amount, string content);

        /// <summary>
        /// Generates a fallback QR code URL when API fails or times out
        /// </summary>
        /// <param name="amount">Transaction amount in VND</param>
        /// <param name="content">Transaction description/content</param>
        /// <param name="bankCode">Bank code (default: 970422 for MBBank)</param>
        /// <returns>VietQR.io direct image URL</returns>
        string GenerateFallbackQRUrl(int amount, string content, string bankCode = "970422");
    }
}
