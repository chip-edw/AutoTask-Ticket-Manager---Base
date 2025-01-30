using AutoTaskTicketManager_Base.MSGraphAPI;
using HtmlAgilityPack;
using Serilog;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace AutoTaskTicketManager_Base.Common.Utilities
{
    public class ContentProcessor
    {

        private static readonly Serilog.ILogger _logger = Log.ForContext<ContentProcessor>();

        #region HTML Related Methods
        internal static async Task<string> ConvertHtmlToTextAndNormalize(string htmlContent)
        {
            try
            {
                // Decode HTML entities
                string decodedContent = DecodeHtmlContent(htmlContent);

                // Load into HTML parser
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(decodedContent);

                // Extract text from body or fallback to full document
                HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
                string rawContent = bodyNode?.InnerHtml ?? htmlDoc.DocumentNode.InnerHtml;

                // Replace common line-break tags with newline characters
                rawContent = rawContent
                .Replace("<br>", "\n")
                    .Replace("<br/>", "\n")
                    .Replace("<br />", "\n")
                    .Replace("</p>", "\n")
                    .Replace("</div>", "\n");

                // Strip remaining HTML tags but preserve newlines
                string parsedContent = HtmlEntity.DeEntitize(Regex.Replace(rawContent, "<[^>]+>", string.Empty));

                // Normalize content for better marker detection
                parsedContent = NormalizeEmailContent(parsedContent);

                // Process for common email markers (Gmail, Outlook, etc.)
                string extractedText = ProcessEmailMarkers(parsedContent);



                // Clean up remaining artifacts
                extractedText = CleanupExtractedText(extractedText);

                return extractedText;
            }
            catch (Exception ex)
            {
                string adminErrMsg = $"Error in ConvertHtmlToText: {ex.Message}";
                _logger.Error(adminErrMsg);

                await EmailManager.MSGraphSendAdminErrorNotificationMail(adminErrMsg);

                return "Failed to extract Email Text";
            }
        }

        // Decode HTML entities and unescape
        public static string DecodeHtmlContent(string htmlContent)
        {
            // Decode HTML entities and unescape special characters
            return Regex.Unescape(HttpUtility.HtmlDecode(htmlContent));
        }

        #endregion

        //######################### Region Separator ############################################

        #region Text String Related Methpods

        // Cleanup artifacts (e.g., &nbsp;, excessive whitespace)
        public static string CleanupExtractedText(string text)
        {
            return text
                .Replace("&nbsp;", " ")                       // Replace non-breaking spaces
                .Replace("\r\n", "\n")                       // Normalize CRLF to LF
                .Replace("\r", "\n")                         // Normalize CR to LF
                .Replace("\n", Environment.NewLine)          // Ensure cross-platform compatibility
                .Replace("\"", "\\\"")                       // Escape double quotes for JSON
                .Replace(" \\ ", "\\\\")                      // Escape single backslash
                .Trim();
        }

        #endregion

        //######################### Region Separator ############################################

        #region Email Related Methods

        // Process for common email markers (e.g., Gmail or Outlook headers)
        private static string ProcessEmailMarkers(string content)
        {
            // Normalize the content to ensure consistent line breaks
            content = NormalizeEmailContent(content);

            // List of reply or forward markers
            var markers = new[]
            {
                @"On\s.+wrote:",                     // Gmail: "On [date], [sender] wrote:"
                @"From:\s.+",                       // Outlook: "From: [sender]"
                @"Sent:\s.+",                       // Outlook: "Sent: [date]"
                @"----\sOriginal\sMessage\s----",   // General: "---- Original Message ----"
                @"Forwarded\sby\s.+",               // General: "Forwarded by [sender]"
            };

            foreach (var marker in markers)
            {
                // Look for the first occurrence of a marker
                var match = Regex.Match(content, marker, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    // Extract content up to the marker
                    return content.Substring(0, match.Index).Trim();
                }
            }

            // If no marker is found, return the full content
            return content.Trim();
        }

        private static string NormalizeEmailContent(string content)
        {
            // Decode HTML entities and replace non-breaking spaces
            content = HttpUtility.HtmlDecode(content)?.Replace("&nbsp;", " ");

            // Replace HTML line breaks with actual newlines
            content = Regex.Replace(content, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);

            // Remove all other HTML tags
            content = Regex.Replace(content, @"<.*?>", string.Empty);

            // Trim the content but preserve existing line breaks
            content = Regex.Replace(content, @"(\r\n|\r|\n)+", "\n"); // Normalize line breaks

            return content.Trim();
        }

        public static string EmailSubjectCleanup(string rawSubject)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            string cleanSubject = rawSubject

                //convert to all lower
                //.ToLower() // Removing becausing it is changing the Case of Acronyms that should remain all caps
                //clean any 're:' or 'fwd'
                .Replace("re:", "", StringComparison.OrdinalIgnoreCase)
                .Replace("fw:", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            //convert to title case
            // string titleCaseString = textInfo.ToTitleCase(cleanSubject.ToLower()); // Removing the 'ToLower()' becausing it is changing the Case of Acronyms that should remain all caps
            string titleCaseString = textInfo.ToTitleCase(cleanSubject);

            return titleCaseString;
        }

        public static string RemoveTicketNumberFromEmailSubject(string subject)
        {
            //pattern match for ticket in subject
            string pattern = "\\bT20\\d{6}[.]{1}\\d{4}|\\bT20\\d{6}[.]{1}\\d{3}";

            Match match = Regex.Match(subject, pattern);
            if (match.Success)
            {
                string subjectCleaned = subject.Remove(match.Index, match.Length);
                subjectCleaned = subjectCleaned.Replace("[]", "");

                return subjectCleaned;
            }
            else
            {
                return subject;
            }

        }

        public static string ReturnTicketNumberFromEmailSubject(string subject)
        {
            //pattern match for ticket in subject
            string pattern = "\\bT20\\d{6}[.]{1}\\d{4}|\\bT20\\d{6}[.]{1}\\d{3}";

            Match match = Regex.Match(subject, pattern);
            if (match.Success)
            {
                //string ticketNumber = match.Groups[1].Value;
                string ticketNumber = match.Value;

                return ticketNumber;
            }

            return "No Match";

        }

        public static string SenderCleanup(string sender, int name = 1)
        {
            try
            {
                var jsonData = JsonDocument.Parse(sender);
                var emailAddress = jsonData.RootElement.GetProperty("emailAddress");
                var nameValue = emailAddress.GetProperty("name").GetString();
                var addressValue = emailAddress.GetProperty("address").GetString();

                if (name == 1)
                {
                    return nameValue;
                }

                else
                {
                    return addressValue;
                }

            }
            catch (Newtonsoft.Json.JsonException)
            {
                _logger.Debug("Invalid JSON format sent to EmailHelper.SenderCleanup(sender)");
                return "Unknown Sender";
            }
            catch (KeyNotFoundException)
            {
                _logger.Debug("The 'name' key is not present in the JSON data sent to EmailHelper.SenderCleanup(sender)");
                return "Unknown Sender";
            }
        }



        #endregion
    }
}
