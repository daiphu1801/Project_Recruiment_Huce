using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_Recruiment_Huce.Models.Payment
{
    /// <summary>
    /// Response from VietQR API
    /// </summary>
    public class VietQRResponse
    {
        /// <summary>
        /// Response code (e.g., "00" for success)
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// Response description
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// QR code data
        /// </summary>
        public VietQRData data { get; set; }
    }

    /// <summary>
    /// VietQR data containing QR code information
    /// </summary>
    public class VietQRData
    {
        /// <summary>
        /// Bank BIN code
        /// </summary>
        public int acqId { get; set; }

        /// <summary>
        /// Account holder name
        /// </summary>
        public string accountName { get; set; }

        /// <summary>
        /// QR code content string
        /// </summary>
        public string qrCode { get; set; }

        /// <summary>
        /// Base64 encoded QR code image (data URL format)
        /// </summary>
        public string qrDataURL { get; set; }
    }
}
