using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Security helper for SePay webhook validation
    /// </summary>
    public static class SePaySecurityHelper
    {
        private static readonly List<string> _allowedIPs = new List<string>();
        private static readonly string _webhookSecret;

        static SePaySecurityHelper()
        {
            // Load allowed IPs from config
            var ipConfig = ConfigurationManager.AppSettings["SePay:AllowedIPs"] ?? "";
            _allowedIPs = ipConfig.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ip => ip.Trim())
                .ToList();

            // Load webhook secret
            _webhookSecret = ConfigurationManager.AppSettings["SePay:WebhookSecret"] ?? "";
        }

        /// <summary>
        /// Validate if the request IP is in whitelist
        /// </summary>
        public static bool IsValidIP(string ipAddress)
        {
            // If no IPs configured, allow all (for development)
            if (_allowedIPs.Count == 0)
            {
                PaymentLogger.Warning($"No IP whitelist configured. Allowing IP: {ipAddress}");
                return true;
            }

            // Check if IP is in whitelist
            var isValid = _allowedIPs.Contains(ipAddress);
            
            if (!isValid)
            {
                PaymentLogger.Warning($"Blocked request from unauthorized IP: {ipAddress}");
            }

            return isValid;
        }

        /// <summary>
        /// Validate webhook signature
        /// SePay typically sends signature in header or as part of payload
        /// Format: HMAC-SHA256(payload, secret)
        /// </summary>
        public static bool ValidateSignature(string payload, string receivedSignature)
        {
            if (string.IsNullOrEmpty(_webhookSecret))
            {
                PaymentLogger.Warning("No webhook secret configured. Skipping signature validation.");
                return true; // For development
            }

            if (string.IsNullOrEmpty(receivedSignature))
            {
                PaymentLogger.Warning("No signature provided in webhook request.");
                return false;
            }

            try
            {
                var expectedSignature = GenerateSignature(payload);
                var isValid = expectedSignature.Equals(receivedSignature, StringComparison.OrdinalIgnoreCase);

                if (!isValid)
                {
                    PaymentLogger.Warning($"Signature mismatch. Expected: {expectedSignature}, Received: {receivedSignature}");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                PaymentLogger.Error("Error validating signature", ex);
                return false;
            }
        }

        /// <summary>
        /// Generate HMAC-SHA256 signature
        /// </summary>
        public static string GenerateSignature(string payload)
        {
            if (string.IsNullOrEmpty(_webhookSecret))
            {
                return string.Empty;
            }

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Validate request is not too old (prevent replay attacks)
        /// </summary>
        public static bool IsRequestTimestampValid(DateTime? timestamp, int maxAgeMinutes = 5)
        {
            if (!timestamp.HasValue)
            {
                return true; // Skip if no timestamp provided
            }

            var age = DateTime.Now - timestamp.Value;
            var isValid = age.TotalMinutes <= maxAgeMinutes;

            if (!isValid)
            {
                PaymentLogger.Warning($"Request timestamp too old: {timestamp.Value}, Age: {age.TotalMinutes} minutes");
            }

            return isValid;
        }
    }
}
