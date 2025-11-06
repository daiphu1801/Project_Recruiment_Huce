using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// HTML Sanitizer Helper - Lọc HTML để chỉ giữ lại các tag an toàn
    /// Cho phép các tag formatting cơ bản từ Quill editor, loại bỏ script và event handlers
    /// </summary>
    public static class HtmlSanitizerHelper
    {
        // Danh sách các tag HTML được phép (whitelist)
        private static readonly HashSet<string> AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "strong", "b", "em", "i", "u", "s", "strike",
            "ul", "ol", "li",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "blockquote", "pre", "code",
            "a", "span", "div"
        };

        // Danh sách các attribute được phép theo tag
        private static readonly Dictionary<string, HashSet<string>> AllowedAttributes = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "a", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href", "title", "target" } },
            { "p", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class", "style" } },
            { "div", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class", "style" } },
            { "span", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class", "style" } },
            { "h1", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
            { "h2", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
            { "h3", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
            { "h4", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
            { "h5", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
            { "h6", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } }
        };

        // Regex để tìm các pattern nguy hiểm
        private static readonly Regex DangerousPatterns = new Regex(
            @"javascript:|on\w+\s*=|&lt;script|&lt;/script|<script|</script>|&lt;iframe|&lt;/iframe|<iframe|</iframe>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Sanitize HTML - loại bỏ các tag và attribute nguy hiểm
        /// </summary>
        /// <param name="html">HTML input từ Quill editor</param>
        /// <returns>HTML đã được sanitize, chỉ giữ lại các tag an toàn</returns>
        public static string Sanitize(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Kiểm tra các pattern nguy hiểm trước
            if (DangerousPatterns.IsMatch(html))
            {
                // Loại bỏ hoàn toàn các pattern nguy hiểm
                html = DangerousPatterns.Replace(html, string.Empty);
            }

            // Loại bỏ các tag không được phép
            // Pattern: <tag> hoặc </tag> hoặc <tag />
            var tagPattern = new Regex(@"</?(\w+)[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            html = tagPattern.Replace(html, match =>
            {
                string tagName = match.Groups[1].Value.ToLower();
                
                // Nếu tag không được phép, loại bỏ
                if (!AllowedTags.Contains(tagName))
                {
                    return string.Empty;
                }

                // Nếu tag được phép, kiểm tra và loại bỏ các attribute không hợp lệ
                string tagContent = match.Value;
                return CleanAttributes(tagName, tagContent);
            });

            // Loại bỏ các event handler còn sót lại (onclick, onerror, etc.)
            html = Regex.Replace(html, @"on\w+\s*=\s*[""'][^""']*[""']", string.Empty, RegexOptions.IgnoreCase);

            // Loại bỏ javascript: trong href
            html = Regex.Replace(html, @"href\s*=\s*[""']javascript:[^""']*[""']", "href=\"#\"", RegexOptions.IgnoreCase);

            return html;
        }

        /// <summary>
        /// Làm sạch attributes của một tag, chỉ giữ lại các attribute được phép
        /// </summary>
        private static string CleanAttributes(string tagName, string tagContent)
        {
            // Nếu tag không có attribute, giữ nguyên
            if (!tagContent.Contains("="))
            {
                return tagContent;
            }

            // Extract tag name và attributes
            var match = Regex.Match(tagContent, @"<(/?)" + Regex.Escape(tagName) + @"([^>]*)>", RegexOptions.IgnoreCase);
            if (!match.Success)
                return tagContent;

            string closingSlash = match.Groups[1].Value;
            string attributes = match.Groups[2].Value;

            if (string.IsNullOrWhiteSpace(attributes))
            {
                return $"<{closingSlash}{tagName}>";
            }

            // Parse và validate attributes
            var allowedAttrs = AllowedAttributes.ContainsKey(tagName) 
                ? AllowedAttributes[tagName] 
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var validAttributes = new List<string>();
            var attrPattern = new Regex(@"(\w+)\s*=\s*([""'])(.*?)\2", RegexOptions.IgnoreCase);
            
            foreach (Match attrMatch in attrPattern.Matches(attributes))
            {
                string attrName = attrMatch.Groups[1].Value.ToLower();
                string quote = attrMatch.Groups[2].Value;
                string attrValue = attrMatch.Groups[3].Value;

                // Chỉ giữ lại attribute được phép và không phải event handler
                if (allowedAttrs.Contains(attrName) && !attrName.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                {
                    // Validate href - chỉ cho phép http, https, mailto, #
                    if (attrName == "href")
                    {
                        if (IsSafeUrl(attrValue))
                        {
                            validAttributes.Add($"{attrName}={quote}{attrValue}{quote}");
                        }
                    }
                    else
                    {
                        validAttributes.Add($"{attrName}={quote}{attrValue}{quote}");
                    }
                }
            }

            string cleanAttrs = validAttributes.Count > 0 ? " " + string.Join(" ", validAttributes) : "";
            return $"<{closingSlash}{tagName}{cleanAttrs}>";
        }

        /// <summary>
        /// Kiểm tra URL có an toàn không (chỉ cho phép http, https, mailto, #)
        /// </summary>
        private static bool IsSafeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            url = url.Trim();
            
            // Cho phép relative URLs (#, /path)
            if (url.StartsWith("#") || url.StartsWith("/"))
                return true;

            // Cho phép http, https, mailto
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Loại bỏ javascript: và data:
            if (url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Strip HTML tags và lấy text thuần túy từ HTML
        /// </summary>
        /// <param name="html">HTML input</param>
        /// <returns>Text thuần túy không có HTML tags</returns>
        public static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Loại bỏ tất cả HTML tags
            string text = Regex.Replace(html, @"<[^>]+>", string.Empty);
            
            // Decode HTML entities
            text = System.Web.HttpUtility.HtmlDecode(text);
            
            // Loại bỏ các khoảng trắng thừa
            text = Regex.Replace(text, @"\s+", " ").Trim();
            
            return text;
        }

        /// <summary>
        /// Lấy preview text từ HTML (strip tags và truncate)
        /// </summary>
        /// <param name="html">HTML input</param>
        /// <param name="maxLength">Độ dài tối đa của preview text</param>
        /// <returns>Preview text không có HTML tags</returns>
        public static string GetPreviewText(string html, int maxLength = 200)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            string text = StripHtml(html);
            
            if (text.Length <= maxLength)
                return text;
            
            // Truncate và thêm "..."
            return text.Substring(0, maxLength).Trim() + "...";
        }
    }
}

