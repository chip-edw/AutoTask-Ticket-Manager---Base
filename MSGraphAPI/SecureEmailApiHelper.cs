using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;


//may need these later: using System.Threading.Tasks; using System.Text; using System.Net.Http; using System;

namespace AutoTaskTicketManager_Base.MSGraphAPI
{
    public class SecureEmailApiHelper
    {
        private readonly HttpClient _httpClient;

        private static readonly Serilog.ILogger _logger = Log.ForContext<SecureEmailApiHelper>();

        public SecureEmailApiHelper(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        // High-level method for sending email
        public async Task SendEmailAsync(string url, string accessToken, string subject, string body, string recipientEmail)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

            // Construct the email payload
            var emailPayload = new
            {
                message = new
                {
                    subject,
                    body = new { contentType = "html", content = body },
                    toRecipients = new[]
                    {
                    new { emailAddress = new { address = recipientEmail } }
                }
                },
                saveToSentItems = "true"
            };

            string jsonPayload = JsonConvert.SerializeObject(emailPayload);

            // Send the POST request
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to send email: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }

        // High-level method for updating draft messages
        public async Task UpdateDraftAsync(string url, string accessToken, string draftId, string updatedContent)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

            var updatePayload = new
            {
                body = new { contentType = "html", content = updatedContent }
            };

            string jsonPayload = JsonConvert.SerializeObject(updatePayload);

            var request = new HttpRequestMessage(HttpMethod.Patch, $"{url}/{draftId}")
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to update draft: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }


        //Method to mark an inbound e-mail as read
        public async Task MarkEmailAsReadAsync(string userEmail, string messageId, string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(messageId))
                throw new ArgumentException("User email and message ID must be provided.");

            string url = $"https://graph.microsoft.com/v1.0/users/{userEmail}/messages/{messageId}";

            var payload = new
            {
                isRead = true
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to mark email as read: {response.StatusCode} - {errorContent}");
            }
        }


        // Add more methods for other email operations as needed...
    }

}
