using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;


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


        #region Send Email Async
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

        #endregion


        #region Mark Inbound Email as Read
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

        #endregion


        #region "FetchAndProcessUnreadEmailsAsync"
        /// <summary>
        /// Fetches and Processes the UnRead Emails asynchroniously
        /// Is called by EmailManager.CheckEmail()
        /// Result is returned to EmailManager.CheckEmail() and Passed to EmailManager.ProcessAddresses method to look for Valid Support Distros
        /// </summary>
        /// <param name="webApiUrl">URL of the MSGraph API to call (supposed to return Json)</param>
        /// <param name="accessToken">Access token used as a bearer security token to call the web API</param>
        /// <param name="processResult">Callback to EmailManager.CheckEmail() so the ProcessAddresses can process the result of the call to the web API</param>
        public async Task FetchAndProcessUnreadEmailsAsync(string webApiUrl, string accessToken, Func<JsonNode, Task> processResultAsync)
        {
            try
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var defaultRequestHeaders = _httpClient.DefaultRequestHeaders;
                    if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                    {
                        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    }
                    defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    HttpResponseMessage response = await _httpClient.GetAsync(webApiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JsonNode result = JsonNode.Parse(json)!;

                        // Check count of unread e-mails. If 0, then return and wait 1 more minute.
                        // The result has the count of messages in the e-mail folder.
                        // The nodes JArray below will have the count of messages.

                        // We are currently only getting the count out of the Jarray here,
                        // and if the message count is >0, then we process the results separately.
                        // However, the JArray also has other attributes such as if there are attachments.

                        JsonArray? nodes = (result as JsonObject)!.ToArray()[1]!.Value as JsonArray;

                        // Set unread email count
                        EmailManager.SetUnreadCount(nodes!.Count);

                        if (nodes.Count > 0)
                        {
                            await processResultAsync(result);
                        }
                        else
                        {
                            Log.Verbose("Mailbox new unread mail count = " + nodes.Count);
                            EmailManager.SetUnreadCount(nodes.Count);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to call the web API: {ex}");

                // Note that if you got response.Code == 403 and response.content.code == "Authorization_RequestDenied"
                // this is because the tenant admin has not granted consent for the application to call the Web API
            }
        }
        #endregion


        #region "PostDraftReplyToAllMessage"
        /// <summary>
        /// Creates a MS Graph Reply All Draft Message using POST method
        /// </summary>
        /// <param name="webApiUrl"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task PostDraftReplyToAllMessage(string webApiUrl, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                var defaultRequestHeaders = _httpClient.DefaultRequestHeaders;
                if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // body ###########################################

                HttpContent httpContent = new StringContent("{ " + "'isRead'" + ":" + "'true'" + " }", encoding: System.Text.Encoding.UTF8, "application/json");

                //#################################################


                HttpResponseMessage response = await _httpClient.PostAsync(webApiUrl, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();

                    //Deserialise response.Content returned from ticket creation
                    var result = JObject.Parse(data);
                    string draftMessageId = (string)result["id"];

                    // save draftMessageID to Email static class.
                    EmailManager.SetField("DraftMsgId", draftMessageId);

                    return;
                }
                else
                {
                    Log.Error($"ProtectedAPICallHelper.PostDraftReplyToAllMessage - Failed to call the web API: {response.StatusCode}");
                    string content = await response.Content.ReadAsStringAsync();

                    // Note that if you got reponse.Code == 403 and reponse.content.code == "Authorization_RequestDenied"
                    // this is because the tenant admin as not granted consent for the application to call the Web API
                }

                return;

            }

        }
        #endregion


        #region "PostSendResourceNotificationMessage"
        public async Task PostSendResourceNotificationMessage(string webApiUrl, string accessToken, string email,
                        string firstName, string ticketNumber, string emailSubject, string emailDescription)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                var contentType = "Text";

                //Get content for Message
                string content = emailDescription;


                //Get the Support email for updating the From: address
                //(this is required as this email is the one that is authorized to send from MS Graph
                var fromEmail = StartupConfiguration.GetConfig("SupportMailBox");

                var body = @"{
                " + "\n" +
                                @"  ""message"": {
                " + "\n" +
                                @"    ""subject"":" + '"' + emailSubject + '"' + "," +
                "\n" +
                                @"    ""body"": {
                " + "\n" +
                                @"      ""contentType"":" + '"' + contentType + '"' + "," +
                    "\n" +
                                @"      ""content"":" + '"' + content + '"' +
                    "\n" +
                                @"    },
                " + "\n" +
                                @"    ""toRecipients"": [
                " + "\n" +
                                @"      {
                " + "\n" +
                                @"        ""emailAddress"": {
                " + "\n" +
                                @"          ""address"":" + '"' + email + '"' +
                    "\n" +
                                @"        }
                " + "\n" +
                                @"      }
                " + "\n" +
                                @"    ]
                " + "\n" +
                                @"  },
                " + "\n" +
                                @"  ""saveToSentItems"": ""true""
                " + "\n" +
                                @"}";

                var client = new RestClient();
                var request = new RestRequest(webApiUrl, Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", accessToken);

                request.RequestFormat = DataFormat.Json;

                request.AddParameter("application/json-patch+json", body, ParameterType.RequestBody);

                RestResponse response = client.Execute(request);

                Log.Debug($"Email sent to {firstName} that {ticketNumber} was reopened pending a proper Resolution being added");
                Log.Verbose($" Response from EmailHelper.ProtectedApiCallHelper.PostSendResourceNotificationMessage is:   {response.Content}");

            }

        }
        #endregion


        #region "PostSendAdminErrorNotificationMessage"
        //PostSendAdminErrorNotificationMessage
        public async Task PostSendAdminErrorNotificationMessage(string webApiUrl, string adminErrorMsg, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                var contentType = "Text";

                //Get content for Message
                string content = adminErrorMsg;

                //Get Admin Email
                var adminEmail = StartupConfiguration.GetConfig("AdminEmail");


                //Get the Support email for updating the From: address
                //(this is required as this email is the one that is authorized to send from MS Graph
                var fromEmail = StartupConfiguration.GetConfig("SupportMailBox");

                var body = @"{
                " + "\n" +
                                @"  ""message"": {
                " + "\n" +
                                @"    ""subject"": ""ERROR - Autotask Ticket Management Worker Service"",
                " + "\n" +
                                @"    ""body"": {
                " + "\n" +
                                @"      ""contentType"":" + '"' + contentType + '"' + "," +
                    "\n" +
                                @"      ""content"":" + '"' + content + '"' +
                    "\n" +
                                @"    },
                " + "\n" +
                                @"    ""toRecipients"": [
                " + "\n" +
                                @"      {
                " + "\n" +
                                @"        ""emailAddress"": {
                " + "\n" +
                                @"          ""address"":" + '"' + adminEmail + '"' +
                    "\n" +
                                @"        }
                " + "\n" +
                                @"      }
                " + "\n" +
                                @"    ]
                " + "\n" +
                                @"  },
                " + "\n" +
                                @"  ""saveToSentItems"": ""true""
                " + "\n" +
                                @"}";

                var client = new RestClient();
                var request = new RestRequest(webApiUrl, Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", accessToken);

                request.RequestFormat = DataFormat.Json;

                request.AddParameter("application/json-patch+json", body, ParameterType.RequestBody);

                RestResponse response = client.Execute(request);

                Log.Verbose($" Response from EmailHelper.ProtectedApiCallHelper.PostSendAdminErrorNotificationMessage is:   {response.Content}");

            }

        }
        #endregion



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


        // Add more methods for other email operations as needed...
    }

}
