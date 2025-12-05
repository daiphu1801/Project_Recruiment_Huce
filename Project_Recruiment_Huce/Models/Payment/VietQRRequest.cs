using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_Recruiment_Huce.Models.Payment
{
    /// <summary>
    /// Model for VietQR API request
    /// </summary>
    public class VietQRRequest
    {
        /// <summary>
        /// Bank account number
        /// </summary>
        public string accountNo { get; set; }

        /// <summary>
        /// Account holder name
        /// </summary>
        public string accountName { get; set; }

        /// <summary>
        /// Bank BIN code (e.g., 970422 for MBBank)
        /// </summary>
        public int acqId { get; set; }

        /// <summary>
        /// Transaction amount in VND
        /// </summary>
        public int amount { get; set; }

        /// <summary>
        /// Transaction description/content
        /// </summary>
        public string addInfo { get; set; }

        /// <summary>
        /// QR code format: "text" or "base64"
        /// </summary>
        public string format { get; set; } = "text";

        /// <summary>
        /// QR template style: "print", "compact", "compact2", etc.
        /// </summary>
        public string template { get; set; } = "print";
    }
}
