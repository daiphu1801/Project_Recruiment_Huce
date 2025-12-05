using System;
using System.IO;
using System.Web;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Logger for payment and webhook operations
    /// Logs to file: ~/Logs/Payment/payment-{date}.log
    /// </summary>
    public static class PaymentLogger
    {
        private static readonly object _lockObj = new object();
        private static readonly string _logDirectory = "~/Logs/Payment";

        public static void Info(string message)
        {
            Log("INFO", message);
        }

        public static void Warning(string message)
        {
            Log("WARNING", message);
        }

        public static void Error(string message, Exception ex = null)
        {
            var fullMessage = message;
            if (ex != null)
            {
                fullMessage += $"\nException: {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            Log("ERROR", fullMessage);
        }

        public static void Webhook(string action, object data)
        {
            var message = $"Webhook {action}:\n{Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented)}";
            Log("WEBHOOK", message);
        }

        private static void Log(string level, string message)
        {
            try
            {
                lock (_lockObj)
                {
                    var logDir = HttpContext.Current?.Server.MapPath(_logDirectory) 
                        ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Payment");

                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    var logFile = Path.Combine(logDir, $"payment-{DateTime.Now:yyyy-MM-dd}.log");
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}\n";

                    File.AppendAllText(logFile, logEntry);
                }
            }
            catch
            {
                // Fail silently to not break application
            }
        }

        /// <summary>
        /// Log webhook request details
        /// </summary>
        public static void LogWebhookRequest(string ipAddress, string userAgent, string requestBody)
        {
            var message = $"Webhook Request:\nIP: {ipAddress}\nUser-Agent: {userAgent}\nBody: {requestBody}";
            Log("WEBHOOK_REQUEST", message);
        }
    }
}
