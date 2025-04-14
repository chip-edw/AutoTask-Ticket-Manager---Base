using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.AutoTaskAPI.Utilities;
using AutoTaskTicketManager_Base.Common.Utilities;
using AutoTaskTicketManager_Base.ManagementAPI;
using AutoTaskTicketManager_Base.Models;
using Azure.Identity;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AutoTaskTicketManager_Base.MSGraphAPI
{
    public class EmailManager
    {

        public static AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");
        private static ConfidentialClientApp _confidentialClientApp;
        private static readonly Serilog.ILogger _logger = Log.ForContext<EmailManager>();
        private readonly SecureEmailApiHelper _emailApiHelper;
        private readonly IConfiguration _configuration;
        private readonly TicketHandler _ticketHandler;
        private readonly AutoTaskResources _autoTaskResources;
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public EmailManager(SecureEmailApiHelper emailApiHelper, IConfiguration configuration, AutoTaskResources autoTaskResources,
            TicketHandler ticketHandler, DbContextOptions<ApplicationDbContext> dbOptions)
        {
            _emailApiHelper = emailApiHelper ?? throw new ArgumentNullException(nameof(emailApiHelper));
            _autoTaskResources = autoTaskResources ?? throw new ArgumentNullException(nameof(autoTaskResources));
            _ticketHandler = ticketHandler ?? throw new ArgumentNullException(nameof(ticketHandler));
            _configuration = configuration;
            _dbOptions = dbOptions;
        }


        // Initialize the ConfidentialClientApp with dependencies
        public static void Initialize(IConfiguration configuration, IMsalHttpClientFactory httpClientFactory)
        {
            if (_confidentialClientApp == null)
            {
                _confidentialClientApp = new ConfidentialClientApp(configuration, httpClientFactory);
            }
        }

        public static ConfidentialClientApp Instance
        {
            get
            {
                if (_confidentialClientApp == null)
                    throw new InvalidOperationException("EmailManager is not initialized. Call Initialize() first.");
                return _confidentialClientApp;
            }
        }


        public static async Task<string> GetAccessTokenAsync()
        {
            // Use the singleton instance to get the token
            return await Instance.GetAccessToken();
        }




        /// <summary>
        /// Used to verify if an e-mail address (Usually an email Distribution) is one of the emails that can trigger AT ticket creation or Update.
        /// </summary>
        /// <param name="Tkey"></param>
        /// <returns></returns>
        public static Int64 GetDistros(string Tkey)
        {
            try
            {
                Int64 value = StartupConfiguration.supportDistros[Tkey];
                return value;
            }
            catch
            {
                return -1;
            }

        }


        #region "StartupFunctions"


        public static string SetField(string fieldName, string fieldValue)
        {

            if (fieldName.First().ToString() != "@") //We need to omit dealing with "@" at this time and we aren't using it anyway.
            {
                // Need to convert the first character in the fieldName to Upper Case to match the field naming convention of classes
                string c1 = fieldName.First().ToString();
                c1 = c1.ToUpper();

                fieldName = fieldName.Substring(1, fieldName.Length - 1);

                fieldName = c1 + fieldName;

                //Now try to populate the class field
                try
                {
                    var field = typeof(Email).GetField(fieldName,
                        BindingFlags.Static |
                        BindingFlags.NonPublic);

                    if (field != null)
                    {
                        field.SetValue(null, fieldValue);

                        return ($"{fieldName}: class Variable Update Successful");

                    }

                }

                catch
                {
                    _logger.Debug($"{fieldName} does not exist as a class field");

                }

            }

            return ($"{fieldName} does not exist as a class field");

        }

        public static string SetBool(string fieldName, bool value)
        {

            if (fieldName.First().ToString() != "@") //We need to omit dealing with "@" at this time and we aren't using it anyway.
            {
                // Need to convert the first character in the fieldName to Upper Case to match the field naming convention of classes
                string c1 = fieldName.First().ToString();
                c1 = c1.ToUpper();

                fieldName = fieldName.Substring(1, fieldName.Length - 1);

                fieldName = c1 + fieldName;

                //Now try to populate the class field
                try
                {
                    var field = typeof(Email).GetField(fieldName,
                        BindingFlags.Static |
                        BindingFlags.NonPublic);

                    if (field != null)
                    {
                        field.SetValue(null, value);

                        return ($"{fieldName}: class Variable Update Successful");

                    }

                }

                catch
                {
                    _logger.Debug($"{fieldName} does not exist as a class field");

                }

            }

            return ($"{fieldName} does not exist as a class field");

        }

        public static void SetAtCompanyId(int companyId)
        {
            //Now try to populate the class field
            var field = typeof(Email).GetField("AtCompanyId",
                BindingFlags.Static |
                BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, companyId);
            }

        }

        public static int GetAtCompanyId()
        {
            var field = typeof(Email).GetField("AtCompanyId",
                BindingFlags.Static |
                BindingFlags.NonPublic);

            var atCoId = field!.GetValue("AtCompanyId");

            return (int)atCoId!;
        }

        public static void SetUnreadCount(int count)
        {
            var field = typeof(Email).GetField("UnreadCount",
                BindingFlags.Static |
                BindingFlags.NonPublic);

            field!.SetValue(null, count);
        }

        public static int GetUnreadCount()
        {
            var field = typeof(Email).GetField("UnreadCount",
                BindingFlags.Static |
                BindingFlags.NonPublic);

            var count = field!.GetValue("UnreadCount");

            return (int)count!;
        }

        public static string GetField(string fieldId)
        {
            var field = typeof(Email).GetField(fieldId,
            BindingFlags.Static |
            BindingFlags.NonPublic);

            var retValue = field!.GetValue(fieldId);

            return (string)retValue!;
        }

        public static bool GetBool(string fieldId)
        {
            var field = typeof(Email).GetField(fieldId,
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            // If the field is null (not found), throw an exception or handle it appropriately
            if (field == null)
            {
                throw new ArgumentException($"Field '{fieldId}' not found in the Email class.");
            }

            // Get the value of the field (null check ensures field exists)
            var retValue = field.GetValue(null); // Pass 'null' for static fields

            // Cast the value to a boolean and return
            return (bool)retValue;
        }

        /// <summary>
        /// Gets the Customer Settings from the Database for all Companyies where Enabled is set to 1
        /// Allows you to see if the support distro is set and if the EnableEmail flag is set
        /// </summary>
        /// <returns>An Array of AutotaskId, AccountName, SupportEmail, EnableEmail</returns>
        public List<List<KeyValuePair<string, string>>> GetCustomerSettings(long id = -1)
        {
            var customers = new List<List<KeyValuePair<string, string>>>();

            using (var context = new ApplicationDbContext(_dbOptions)) // DbContext name from Models Class
            {
                // Create a query to select the required fields from CustomerSettings
                var query = context.CustomerSettings
                    .Where(cs => cs.Enabled && (id == -1 || cs.AutotaskId == id))
                    .Select(cs => new
                    {
                        cs.AccountName,
                        cs.AutotaskId,
                        cs.SupportEmail,
                        cs.EnableEmail,
                        cs.AutoAssign
                    });

                try
                {
                    // Execute the query and build the list of key-value pairs
                    foreach (var customer in query.ToList())
                    {
                        var customerData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("AccountName", customer.AccountName),
                        new KeyValuePair<string, string>("AutotaskId", customer.AutotaskId.ToString()),
                        new KeyValuePair<string, string>("SupportEmail", customer.SupportEmail),
                        new KeyValuePair<string, string>("EnableEmail", customer.EnableEmail.ToString()),
                        new KeyValuePair<string, string>("AutoAssign", customer.AutoAssign.ToString())
                    };

                        customers.Add(customerData);
                    }

                    _logger.Information("Loaded Active Customers and Settings from EF Core.");
                }
                catch (Exception ex)
                {
                    _logger.Error("Unable to read from database using EF Core", ex);
                }
            }

            return customers;
        }

        #endregion



        //################################################################################################################
        //#                                                                                                              #
        //#                Mail Message processing functions go below this demarc                                        #
        //#                                                                                                              #
        //################################################################################################################

        #region "DraftReplyAllRunAsync"
        /// <summary>
        /// Creates a Draft Reply To All Email by calling - await MSGraphCreateDraftReplyAll(config, app, scopes).
        /// Retrieves the draft Body by calling - await MSGraphGetDraftReplyAllBody(config, app, scopes).
        /// Updates the Draft Body and Subject by calling - await MSGraphUpdateDraftReplyAll(config, app, scopes).
        /// Sends the updated Draft Email by calling - await MSGraphSendDraftReplyAll(config, app, scopes).
        /// </summary>
        /// <returns></returns>
        public static async Task DraftReplyAllRunAsync(SecureEmailApiHelper emailApiHelper)
        {
            _logger.Debug("...");
            _logger.Debug("Begin Async operations to Create MS Graph Draft Reply All Message\n");

            //##################################################

            //Create Draft Reply All
            _logger.Debug("await MSGraphCreateDraftReplyAll()\n");
            await MSGraphCreateDraftReplyAll(emailApiHelper);

            _logger.Debug("await EditAndSendDraftReplyAllMessage()\n");
            //Test CreateAndEditDraftReplyAllMessage
            await EditAndSendDraftReplyAllMessage(emailApiHelper);

            //##################################################
        }

        #endregion

        #region "EditAndSendDraftReplyAllMessage"
        private static async Task EditAndSendDraftReplyAllMessage(SecureEmailApiHelper emailApiHelper)
        {
            //Create function wide variables for manipulating the content of the message body
            string oldContent = "";
            string newContent = "";
            string updatedContent = "";

            JObject sN = JObject.Parse(EmailManager.GetField("Sender"));
            string ticketNumber = EmailManager.GetField("AtTicketNo");

            string subject = EmailManager.GetField("Subject");

            string ticketTitle = ContentProcessor.RemoveTicketNumberFromEmailSubject(EmailManager.GetField("Subject"));

            string senderName = (string)sN["emailAddress"]["name"].ToString();

            string[] scopes = new string[] { $"{config.ApiUrl}.default" }; // Generates a scope -> "https://graph.microsoft.com/.default"
            //var accessToken = await ConfidentialClientApp.Instance.GetAccessToken();
            //var app = ConfidentialClientApp.Instance.GetApplication();

            var clientSecretCredential = new ClientSecretCredential(
                StartupConfiguration.GetMsGraphConfig("Tenant"), StartupConfiguration.GetMsGraphConfig("ClientID"),
                StartupConfiguration.GetMsGraphConfig("ClientSecret"));

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var senderEmailAddress = StartupConfiguration.GetConfig("SupportMailBox");

            var draftMessageId = GetField("DraftMsgId");


            // Retrieve the Draft message being Updated.
            Microsoft.Graph.Models.Message draftReplyAllMessage = await graphClient.Users[senderEmailAddress].Messages[draftMessageId].GetAsync();

            // Need to check if inbound e-mail is text and if it is then convert to HTML
            string contentType = draftReplyAllMessage!.Body!.ContentType!.ToString()!.ToLower();


            if (contentType != "html")
            {
                // Load e-mail content into string
                string textEmail = draftReplyAllMessage!.Body!.Content!.ToString();

                // Create an HTML document with a <div> element so we can later wrap it in the <Body> of the updated Comment of the updated draft message.
                var doc = new HtmlDocument();
                var div = HtmlNode.CreateNode("<div></div>");
                doc.DocumentNode.AppendChild(div);

                // Split the text email into lines
                string[] lines = textEmail.Split('\n');

                // Iterate through the lines and create HTML nodes for each one
                foreach (string line in lines)
                {
                    // Create a <p> element for each line of text
                    var p = HtmlNode.CreateNode("<p></p>");
                    p.InnerHtml = line;

                    // Add the <p> element to the <div> element
                    div.AppendChild(p);
                }

                //Load html doc to string
                oldContent = doc.DocumentNode.InnerHtml;


                //Need to edit the htmlText from the EmailReplySettings so it can be wrapped in the new html comment.
                //also edit the Message styles in the emailreplysettings to something more appropiate.
                //Wrap it all up in the "updatedContent" string and let it rip.
                newContent = EmailService.Default.HtmlMessageText;

                updatedContent = newContent + "<br>" + oldContent + "</Body></html>";

            }
            else if (contentType == "html")
            {
                _logger.Debug("...");

                //load HTML Style into String
                _logger.Debug("HTML e-mail detected. Loading FF HTML Styling.");
                string messageStyleString = EmailService.Default.MessageStyleString;

                // Load customer e-mail content into string
                _logger.Debug("Loading Customer e-mail content into string");
                string htmlEmail = draftReplyAllMessage.Body!.Content!.ToString();

                //Get Logo - Need to add a try statement around Logo with Good logging
                _logger.Debug("Loading Logo for outbound e-mail body creation");
                string logo = GetLogo(GetField("SupportDistro"));

                string logoImageString = $"<p class=MsoNormal align=right style='text-align:right'>" +
                    $"<img " + "width=220 height=101 src=\"" + "data:image/png;base64," +
                    $"{logo}\"" +
                    " alt = \"" + "Logo" + "\" />";

                _logger.Debug("...");
                //Load FF Reply
                _logger.Debug("Load FF Reply");
                newContent = EmailService.Default.HtmlMessageText;

                //update the content br replacing format items in the string to insert Sender Name, Ticket Number, Ticket Title
                _logger.Debug("Update the content br replacing format items in the string to insert Sender Name, Ticket Number, Ticket Title");
                newContent = string.Format(newContent, senderName, ticketNumber, ticketTitle);

                // Load the HTML string into an HtmlDocument object
                _logger.Debug("Create HTML Document Object");
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlEmail);

                // Find the <body> element in the document
                var body = doc.DocumentNode.SelectSingleNode("//body");

                // If the <body> element was found, return its inner HTML
                if (body != null)
                {
                    oldContent = body.InnerHtml;
                }


                updatedContent = "<html><head>" + "<meta http-equiv=\"Content-Type\"content=\"text/html;" +
                    "charset=utf-8\"><meta name =\"Generator\" content=\"Microsoft Word 15 (filtered medium)\">" +
                    "<body lang=\"EN-US\" link=\"#0563C1\" vlink=\"#954F72\" style=\"word-wrap:break-word\">" +
                    "</head>" + logoImageString + newContent + "<br>" + oldContent + "</Body></html>";

            }
            _logger.Debug("...");
            // Update the Draft Message Subject
            _logger.Debug("Update the Draft Message Subject");
            draftReplyAllMessage.Subject = subject;

            // Update the draft message Body
            _logger.Debug("Update the Draft Message Body");
            draftReplyAllMessage.Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = updatedContent
            };

            // Save the updated draft reply all message
            _logger.Debug("Save the Updated Draft Reply All Message in MS Graph");
            var updatedResponse = await graphClient.Users[senderEmailAddress].Messages[draftMessageId]
            .PatchAsync(draftReplyAllMessage);


            //Send the updated draft reply all message
            _logger.Debug("Send the updated Draft Reply All Message");
            var sendRequest = graphClient.Users[senderEmailAddress].Messages[draftMessageId].Send;
            await sendRequest.PostAsync();

            var finalResult = Task.CompletedTask;
            if (finalResult.IsCompletedSuccessfully == true)
            {
                _logger.Debug("EmailHelper.EditAndSendDraftReplyAllMessage - Draft ReplyAll Successfully Sent after AT Ticket Creation\n\n");
            }
            else
            {
                _logger.Debug("EmailHelper.EditAndSendDraftReplyAllMessage - Issue Sending Draft Reply All after AT Ticket Creation\n\n");
            }

        }
        #endregion

        #region "GetLogo"
        /// <summary>
        /// Takes the email address from the sender and based on the email domain determines if the logo is for Flintfox or Flintech,
        /// Returns the logo image as a base 64 encoded string.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static string GetLogo(string emailAddress)
        {
            // Get OS
            string os = StartupConfiguration.DetermineOS();


            //test email address follows delete when done
            emailAddress = "support-whatever@flintfox.com";

            string logo = string.Empty;

            byte[] imageBytes = null;

            if (emailAddress != null)
            {
                emailAddress = emailAddress.ToLower();
                string pattern = "\\bflintfox.com";

                Match matchFlintfox = Regex.Match(emailAddress, pattern);

                if (matchFlintfox.Success)
                {
                    logo = "flintfox";
                    //imageBytes = File.ReadAllBytes(@"C:\\pictures\\flintfox.png");
                    imageBytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Logos", "flintfox.png"));
                    string imageBytes64 = Convert.ToBase64String(imageBytes);

                    return imageBytes64;
                }
                else if (!matchFlintfox.Success)
                {
                    pattern = "\\bflintech.com";

                    Match matchFlintech = Regex.Match(emailAddress, pattern);

                    if (matchFlintech.Success)
                    {
                        logo = "flintech";
                        //imageBytes = File.ReadAllBytes(@"C:\\pictures\\flintech.png");
                        imageBytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Logos", "flintech.png"));
                        string imageBytes64 = Convert.ToBase64String(imageBytes);

                        return imageBytes64;
                    }
                }
                else
                {
                    return "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgl" +
                    "jNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==\"";
                }
            }
            return "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgl" +
                    "jNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==\"";
        }

        #endregion

        #region "MSGraphCreateDraftReplyAll"

        /// <summary>
        /// Does the Prepwork for the Draft ReplyAll then calls the ProtectedApiCallHelper which creates the Draft Reply All
        /// </summary>
        /// <param name="config"></param>
        /// <param name="app"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        //private static async Task MSGraphCreateDraftReplyAll(AuthenticationConfig config, IConfidentialClientApplication app, string[] scopes)
        private static async Task MSGraphCreateDraftReplyAll(SecureEmailApiHelper emailApiHelper)
        {
            var accessToken = await GetAccessTokenAsync();

            if (accessToken != null)
            {
                //filter operators are =:eq !=:ne
                var emailId = StartupConfiguration.GetConfig("SupportMailBox");
                var supportMessageId = GetField("Id");
                var httpClient = new HttpClient();
                var apiCaller = new SecureEmailApiHelper(httpClient);
                var suffixUrl = emailId + "/messages/" + supportMessageId + "/createReplyAll";

                _logger.Verbose("...");
                _logger.Verbose($"Create Draft ReplyAll request to: {config.ApiUrl}v1.0/users/" + suffixUrl);
                _logger.Verbose("...");

                try
                {
                    //Mark That Message Read before we create a draft Reply
                    //Get the Id of the message stored in the Email class
                    var messageId = GetField("Id");
                    // create the url resource suffix to identify the message we are marking complete
                    var suffixUrlRx = emailId + "/messages/" + messageId;

                    // Mark e-mail as read
                    await emailApiHelper.MarkEmailAsReadAsync(emailId, messageId, accessToken);

                    _logger.Debug("...");
                    //Now go ahead and create the draft Reply All
                    _logger.Debug("Create Draft Reply All");
                    await emailApiHelper.PostDraftReplyToAllMessage($"{config.ApiUrl}v1.0/users/" + suffixUrl, accessToken);

                    _logger.Debug("EmailHelper.MSGraphCreateDraftReplyAll -  Draft ReplyAll Created");

                    return;
                }
                catch (Exception ex)
                {
                    _logger.Debug($"Draft ReplyAll Creation failed... {ex}");
                }
            }

            return;
        }



        private static async Task MSGraphMarkMessageRead(SecureEmailApiHelper emailApiHelper)
        {
            var accessToken = await GetAccessTokenAsync();

            if (accessToken != null)
            {
                //filter operators are =:eq !=:ne
                var emailId = StartupConfiguration.GetConfig("SupportMailBox");
                var supportMessageId = GetField("Id");
                var httpClient = new HttpClient();
                var apiCaller = new SecureEmailApiHelper(httpClient);
                var messageId = GetField("Id");

                // create the url resource suffix to identify the message we are marking complete
                var suffixUrlRx = emailId + "/messages/" + messageId;

                try
                {
                    // Trigger the patch method to mark e-mail as read
                    //Get the Id of the message stored in the Email class
                    //await apiCaller.PatchWebApiAndMarkRead($"{config.ApiUrl}v1.0/users/" + suffixUrlRx, accessToken);
                    await emailApiHelper.MarkEmailAsReadAsync(emailId, messageId, accessToken);

                    _logger.Verbose("\n...");
                    _logger.Verbose($"EmailHelper.MSGraphMarkMessageRead: {config.ApiUrl}v1.0/users/" + suffixUrlRx);
                    _logger.Verbose("...\n");

                    return;
                }
                catch (Exception ex)
                {
                    _logger.Debug($"\n EmailHelper.MSGraphMarkMessageRead failed... {ex}");
                }

            }

            return;
        }
        #endregion


        #region "Used for passing MS Graph message Base64 string representation to AutoTask so it can be attached to AT Ticket"
        /// <summary>
        /// "Used for passing MS Graph message Base64 string representation to AutoTask so it can be attached to AT Ticket"
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public static async Task<string> MSGraphGetCompleteMessageById(string msgId, SecureEmailApiHelper emailApiHelper)
        {
            // Retrieve an access token (your implementation may vary)
            var accessToken = await GetAccessTokenAsync();
            if (accessToken == null)
            {
                throw new InvalidOperationException("Access token is null. Ensure token retrieval is configured correctly.");
            }

            // Retrieve the support mailbox email from configuration
            var emailId = StartupConfiguration.GetConfig("SupportMailBox");
            if (string.IsNullOrEmpty(emailId))
            {
                throw new InvalidOperationException("SupportMailBox configuration is missing or empty.");
            }

            // Construct the URL for the Graph API request
            // Ensure that config.ApiUrl ends with a trailing slash (or adjust the URL construction accordingly)
            var url = $"{config.ApiUrl}v1.0/users/{emailId}/messages/{msgId}/$value";

            using (var httpClient = new HttpClient())
            {
                // Set the Authorization header with the Bearer token
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Send the GET request to retrieve the message content as a stream
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Read the content stream
                var stream = await response.Content.ReadAsStreamAsync();

                // Convert the stream to a byte array using your ReadFully method,
                // then convert that byte array to a Base64-encoded string.
                string base64String = Convert.ToBase64String(ReadFully(stream));

                return base64String;
            }
        }

        // Helper method to read the entire stream for method 'MSGraphGetCompleteMessageById()'
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        #endregion



        public static async Task MSGraphSendAdminErrorNotificationMail(string adminErrMsg, SecureEmailApiHelper emailApiHelper)
        {
            var accessToken = await GetAccessTokenAsync();

            if (accessToken != null)
            {
                //To access the desired mailbox we can use either the userPrincipalName(E-mail address) or the ProfileObjectID(User ID)
                //I am going to use the e-mail address as it is more readable.
                //filter operators are =:eq !=:ne
                var emailId = StartupConfiguration.GetConfig("SupportMailBox");
                var httpClient = new HttpClient();
                var apiCaller = new SecureEmailApiHelper(httpClient);
                var suffixUrl = emailId + "/sendmail";
                string errorMsg = adminErrMsg;
                string apiUrl = $"{config.ApiUrl}v1.0/users/";

                try
                {
                    await emailApiHelper.PostSendAdminErrorNotificationMessage(apiUrl + suffixUrl, errorMsg, accessToken);
                }
                catch (Exception ex)
                {
                    _logger.Debug($" EmailHelper.MSGraphSendAdminErrorNotificationMail Failed... {ex}");

                }

            }

        }



        public async Task SendEmailToResource(string email, string firstName, string ticketNumber,
            string emailSubject, string emailDescription, SecureEmailApiHelper emailApiHelper)
        {
            var accessToken = await GetAccessTokenAsync();

            if (accessToken != null)
            {
                //To access the desired mailbox we can use either the userPrincipalName(E-mail address) or the ProfileObjectID(User ID)
                //I am going to use the e-mail address as it is more readable.
                //filter operators are =:eq !=:ne
                var emailId = StartupConfiguration.GetConfig("SupportMailBox");
                var httpClient = new HttpClient();
                var apiCaller = new SecureEmailApiHelper(httpClient);
                var suffixUrl = emailId + "/sendmail";
                string errorMsg = "Delete this later";
                string apiUrl = $"{config.ApiUrl}v1.0/users/";

                try
                {
                    await emailApiHelper.PostSendResourceNotificationMessage(apiUrl + suffixUrl, accessToken, email,
                        firstName, ticketNumber, emailSubject, emailDescription);
                }
                catch (Exception ex)
                {
                    _logger.Debug($" EmailHelper.SendEmailToResource Failed... {ex}");

                }

            }

        }


        public static async Task CheckEmail(SecureEmailApiHelper emailApiHelper, IConfiguration configuration, EmailManager emailManager,
            TicketHandler ticketHandler)
        {
            string accessToken = Authenticate.GetAccessToken();

            try
            {
                //filter operators are =:eq !=:ne
                var emailId = StartupConfiguration.GetConfig("SupportMailBox");
                var filter = "?$filter=isRead ne true";

                var httpClient = new HttpClient();
                //var apiCaller = new ProtectedApiCallHelper(httpClient);
                var suffixUrl = emailId + "/mailfolders/" + "inbox/messages" + filter;


                _logger.Verbose($"email request to: {config.ApiUrl}v1.0/users/" + suffixUrl);
                _logger.Verbose("...");


                //Key call that returns the result from the SecureEmailApiHelper.cs
                //The result is returned and processed by the EmailManager.ProcessAddresses() method.
                await emailApiHelper.FetchAndProcessUnreadEmailsAsync($"{config.ApiUrl}v1.0/users/" + suffixUrl, accessToken, async (result) =>
                {
                    await ProcessAddresses(result, emailApiHelper, configuration, emailManager, ticketHandler);
                });



                //Read count of unread messages. If count = 0 return.
                if (GetUnreadCount() > 0)
                {
                    //Get the Id of the message stored in the Email class
                    var messageId = GetField("Id");
                    _logger.Debug("...");
                    _logger.Debug($"Stored E-mail message ID.\n Message ID: {messageId}\n");
                    var timeRemain = Authenticate.GetExpiresOn() - DateTime.UtcNow;
                    _logger.Debug($"Bearer Token active. {timeRemain} minutes remain");
                    _logger.Debug("...");

                    // create the url resource suffix to identify the message we are marking complete
                    var suffixUrlRx = emailId + "/messages/" + messageId;

                    // Mark e-mail as read
                    await emailApiHelper.MarkEmailAsReadAsync(emailId, messageId, accessToken);

                }


            }
            catch (Exception ex)
            {
                _logger.Error($"Error in: EmailHelper.CheckEmail(): {ex}");
            }

        }


        public static Int64 CheckSupportEmail(SecureEmailApiHelper emailApiHelper)

        {
            string subject = null;
            string body = null;
            string ticketNumber = null;
            string addressCc = null;



            string rawSubject = EmailManager.GetField("Subject");
            string rawSender = EmailManager.GetField("From");


            //Check for empty subject. If empty return a -1 indicating do not process.
            if (String.IsNullOrEmpty(rawSubject) is true | rawSubject.Length < 4)
            {
                _logger.Debug("...");
                _logger.Warning("Detected empty Subject in inbound e-mail. Ignoring e-mail");
                return -1;
            }
            else
            {
                _logger.Debug("...");
                _logger.Debug($"Inbound E-mail Subject:  {rawSubject}");
            }

            //Cleanup the Subject for furthur processing.
            subject = ContentProcessor.EmailSubjectCleanup(rawSubject);

            //Check for Subject Exclusion Key Words and if excluded KeyWord found return -1 indicating do not process.
            var isExclusion = EmailManager.SubjectExclusionKeyWords(EmailManager.GetField("Subject"));

            if (isExclusion.Result == true)
            {
                _logger.Debug("Detected Subject Exclusion Keyword. Ignoring e-mail");
                return -1;
            }

            //Cleanup From e-mail address for futhur processing
            var senderAddress = ContentProcessor.SenderCleanup(rawSender, 0);



            //Check for Sender Exclusion From Email Addressess and if excluded email address found return -1 indicating do not process.
            var isSenderExclusion = EmailManager.SenderExclusionEmailAddresses(senderAddress);

            if (isSenderExclusion.Result == true)
            {
                _logger.Debug("Detected Sender E-mail Exclusion. Ignoring e-mail");
                return -1;
            }

            //Check if Sender is an internal Sender Assignment Resource. If True and the Resource is marked active then update
            //the Email class SenderAssignment bool as True. It will be used later in the TicketHandler to determine if the ticket should be assigned to the sender.
            bool senderAssignmentExists = StartupConfiguration.autoAssignSenders.Values
                .Any(objects =>
                {
                    // Extract resourceEmail and resourceActive from the object array
                    var resourceEmail = objects.OfType<string>().ElementAtOrDefault(1); // Email is the second element
                    var resourceActive = objects.OfType<bool>().FirstOrDefault();      // Active is the last element

                    // Check if the resourceEmail matches and resourceActive is true
                    return resourceEmail?.Equals(senderAddress, StringComparison.OrdinalIgnoreCase) == true
                           && resourceActive == true;
                });



            if (senderAssignmentExists == true)
            {
                //Get the SenderAssignment status

                _logger.Debug($"Sender Assign = {senderAssignmentExists} will attempt assigning ticket to sender \n ");

                EmailManager.SetBool("SenderAssignment", senderAssignmentExists);
            }
            else
            {
                //if senderAssignmentExists is not true let's go ahead and update the Email Static class bool to false 
                EmailManager.SetBool("SenderAssignment", senderAssignmentExists);
            }


            //Check for Ticket Number "Completed" Status in Subject

            //Get ticketNumber if it exists. A result of "No Match" means there was no ticket number
            ticketNumber = ContentProcessor.ReturnTicketNumberFromEmailSubject(rawSubject);

            if (ticketNumber != null && ticketNumber != "No Match")
            {
                _logger.Debug($"Found Ticket number in Subject:  {ticketNumber}");
                //Check for Ticket Number "Complete" Status in Subject
                //##
                // The second parameter being passed is to bypass the need to compare the AT CompanyID against the support distro
                // which is not needed in this case and would not be successful since I dont have the company ID in this case
                //My easy way of repurposing an existing Method that had wider scope without needing to change much.

                string ticketStatus = TicketHandler.GetTicketByNumberUpdateTicketClass(ticketNumber, true);

                if (ticketStatus == "Complete")
                {
                    _logger.Debug("Detected ticket exists and is in Complete Status. Ignoring e-mail");
                    return -1;
                }

            }


            //Get the Recipients in the TO:
            string toRecipients = EmailManager.GetField("ToRecipients");

            _logger.Debug($"\nThe TO: recipients are:  {toRecipients}");

            JsonNode nodes = JsonArray.Parse(toRecipients);

            foreach (JsonObject aNode in nodes.AsArray())
            {
                foreach (var property in aNode.ToArray())
                {
                    string fieldValue = property.Value.ToString();

                    var emailDict = JsonValue.Parse(fieldValue);

                    // Check the TO: for a Support Distro. If we find a Support Distro dont bother checking the CC:
                    if (emailDict != null)
                    {
                        //parse e-mail address for a support distro


                        string address = (string)emailDict["address"];

                        if (EmailManager.GetDistros(address) >= 0)
                        {
                            //Found Match.  Need to add something to check if the distro is enabled...........
                            //################################################################################
                            _logger.Debug("...");
                            _logger.Debug($"Process Email Found match of TO: {address} in supportDistro Dictionary\n");

                            //Write support distro to Email class for later reference when looking up logo
                            EmailManager.SetField("SupportDistro", address);

                            //Get AT Id
                            Int64 ATID = EmailManager.GetDistros(address);


                            // Check for Duplicate Ticket before creating
                            //  Compare title to the dictionary 'companiesTicketsNotCompleted' and make sure title not already exists
                            bool cleanSubjctExists = StartupConfiguration.companiesTicketsNotCompleted.ContainsKey(subject);
                            bool rawSubjectExists = StartupConfiguration.companiesTicketsNotCompleted.ContainsKey(rawSubject);

                            if (cleanSubjctExists || rawSubjectExists)
                            {
                                //if cleaned email rawSubject or cleanSubjects exists as an open ticket title then set ATID to -1 so e-mail does not get processed
                                ATID = -1;
                                string existingTicketNumber = StartupConfiguration.companiesTicketsNotCompleted.GetValueOrDefault(subject);
                                _logger.Information($"Ticket Generation skipped. Email with Subject: '{rawSubject}' already has a ticket:" +
                                    $" {existingTicketNumber}");


                            }

                            // if the ticket title of an open ticket does not exist then ATID stays the same and processes as normal

                            //We also need to update the 'StartupConfiguration.companiesTicketsNotCompleted' with the new subject soon as a new ticket is created
                            //that way we catch duplicates right away if a customer or employee quickly replies the first e-mail without the new ticket number.


                            return ATID;
                        }

                    }

                }

            }
            _logger.Debug("...");
            _logger.Debug("Process E-mail No TO: address match found in supportDistro Dictionary\n");

            // If did not find a "Support" Distro in the TO: then Check the CC:
            //Get the Recipients in the CC:
            string CcRecipients = EmailManager.GetField("CcRecipients");

            _logger.Debug("Moving on to check the CC: addresses for a match in the supportDistro Dictionary");

            _logger.Debug($"The CC: recipients are:  {CcRecipients}");

            JsonNode nodesCc = JsonArray.Parse(CcRecipients)!;

            foreach (JsonObject aNodeCc in nodesCc.AsArray())
            {
                foreach (var propertyCc in aNodeCc!.ToArray())
                {
                    string fieldValueCc = propertyCc.Value!.ToString();

                    var emailDictCc = JsonValue.Parse(fieldValueCc);

                    // Check the CC: for a Support Distro. If we do not find a Support Distro return -1
                    if (emailDictCc != null)
                    {
                        //parse e-mail address for a support distro
                        addressCc = (string)emailDictCc["address"]!;

                        Int64 foundAddress = EmailManager.GetDistros(addressCc);
                        if (foundAddress >= 0)
                        {

                            //Found Match now do something
                            _logger.Verbose($" \nProcess Email Found match of CC: {addressCc} in supportDistro Dictionary \n");

                            //Write support distro to Email class for later reference when looking up logo                            
                            EmailManager.SetField("SupportDistro", addressCc);

                            //Get AT Id
                            Int64 ATID = EmailManager.GetDistros(addressCc);

                            // Check for Duplicate Ticket before creating
                            //  Compare title to the dictionary 'companiesTicketsNotCompleted' and make sure title not already exists
                            bool cleanSubjctExists = StartupConfiguration.companiesTicketsNotCompleted.ContainsKey(subject);
                            bool rawSubjectExists = StartupConfiguration.companiesTicketsNotCompleted.ContainsKey(rawSubject);

                            if (cleanSubjctExists || rawSubjectExists)
                            {
                                //if cleaned email rawSubject or cleanSubjects exists as an open ticket title then set ATID to -1 so e-mail does not get processed
                                ATID = -1;
                                string existingTicketNumber = StartupConfiguration.companiesTicketsNotCompleted.GetValueOrDefault(subject);
                                _logger.Information($"Ticket Generation skipped. Email with Subject: '{rawSubject}' already has a ticket:" +
                                    $" {existingTicketNumber}");

                                // if the ticket title of an open ticket does not exist then ATID stays the same and processes as normal

                                //We also need to update the 'AppConfig.companiesTicketsNotCompleted' with the new subject soon as a new ticket is created
                                //that way we catch duplicates right away if a customer or employee quickly replies the first e-mail without the new ticket number.
                            }

                            return ATID;
                        }


                    }


                }


            }
            _logger.Debug($"\n Process E-mail No CC: match for {addressCc} in supportDistro Dictionary \n");
            return -1;
        }

        //################################################

        #region "ProcessAddresses"

        /// <summary>
        /// ProcessAddresses the result of the Web API call and Populate Email static class
        /// </summary>
        /// <param name="result">Object with Addresses to Process</param>

        async static Task ProcessAddresses(JsonNode result, SecureEmailApiHelper emailApiHelper, IConfiguration configuration, EmailManager emailManager, TicketHandler _ticketHandler)
        {

            JsonArray nodes = ((result as JsonObject)!.ToArray()[1])!.Value as JsonArray;

            if (nodes!.Count == 0)
            {
                _logger.Debug("Mailbox new unread mail count = " + nodes.Count);

                SetUnreadCount(nodes.Count);

                return;
            }

            else


            {
                _logger.Verbose("Mailbox new unread mail count = " + nodes.Count);

                _logger.Debug("\n\n");
                _logger.Debug("****");
                _logger.Debug("Begin Processing Unread E-mail");

                SetUnreadCount(nodes.Count);
                foreach (JsonObject aNode in nodes.ToArray())
                {
                    foreach (var property in aNode.ToArray())
                    {
                        if (property.Value != null)

                        {
                            string fieldName = property.Key.ToString();
                            string fieldValue = property.Value.ToString();

                            //Populate Email static class with message attributes
                            string rtn = SetField(fieldName, fieldValue);

                        }

                        else

                        {
                            _logger.Verbose($"{property.Key}: is null");
                        }

                    }
                    _logger.Debug("...");
                    _logger.Debug("Finished loading message to class");

                    //Process the message we just loaded. An integer is returned representing the AT Customer/Company ID.
                    // A return = -1 indicates no match was found. We only create an AT ticket on a successful match of the support distro.

                    Int64 coID = CheckSupportEmail(emailApiHelper);

                    //Write the coID that is associated with the Support Distro to the Email class. Later we will compare it with the 
                    //companyID returned from the AT ticket we pull.
                    SetAtCompanyId((int)coID);

                    if (coID == -1)
                    {
                        // No matching support distro was found. This message needs to be marked as Read so it is not reprocessed.
                        // This will happen when control is switched back to the calling method CallMSGraph().
                        // In fact the message will get marked as read even if there is a support group match as the branch logic is in CallMSGraph.
                        _logger.Debug($"CheckEmail.ProcessAddresses() received {coID} from ProcessEmail.CheckSupportEmail(), " +
                            $"indicating do not process. - Check ProcessEmail.CheckSupportEmail() for conditions");

                        return;
                    }
                    else

                    #region "C. R. E. A. T. E. AT Ticket"

                    {
                        if (coID > -1)
                        {
                            //OK we have a match on the Support Distro
                            //Check for a ticket number in the subject. If the ticket is active (not completed) update the ticket

                            //Get Subject from Email class and check for Auto Task Active ticket
                            string subject = GetField("Subject");

                            string ticketStatus = TicketHandler.GetTicketStatusFromEmailSubject(subject);




                            //Create AT ticket here.
                            if (ticketStatus == "CREATE")
                            {
                                //Create an Autotask Ticket
                                // The ticket id and the ticket number are saved in the Email class as AtTicketId and AtTicketNo

                                string resultValue = await _ticketHandler.CreateTicket(coID, emailApiHelper, configuration);


                                if (resultValue != "exception" && resultValue != null)
                                {

                                    //retrieve the AT Ticket Number that was saved to the Email class when the ticket was created in the step above
                                    string ticketNumber = GetField("AtTicketNo");
                                    _logger.Debug($"Ticket number created: {ticketNumber}");

                                    try
                                    {
                                        await DraftReplyAllRunAsync(emailApiHelper);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Warning($"\n Failed in MSGraph.EmailHelper.DraftReplyAllRunAsync() \n {ex}");
                                        _logger.Warning($"Ticket {ticketNumber} created but Draft Reply not sent\n");

                                        await MSGraphSendAdminErrorNotificationMail(ex.ToString(), emailApiHelper);
                                    }

                                }
                                else
                                {
                                    _logger.Error($"Result: {resultValue} No Ticket Created");
                                    await MSGraphMarkMessageRead(emailApiHelper);
                                    _logger.Error("Email Message Marked Read to prevent reprocessing after ticket creation failure");
                                }

                                return;
                            }

                            #endregion

                            //If the 'ticketStatus' == "!" then the Ticket number in the subject does not belong
                            //to the company matching the support distro so bail. A log entry was made
                            //in the "TicketHandler.GetTicketStatusFromEmailSubject" function.
                            if (ticketStatus == "!")
                            {
                                return;
                            }


                            #region "Add Ticket Note""

                            //If the ticket status returned != 'None' && != 'Completed'  add a ticket Note from the e-mail body
                            // We need the AutoTask ID to update/modify the ticket because the ticket number is just an attribute of the Ticket
                            // The ticket class should have the updated autoTaskId and updated status.
                            if (ticketStatus != "None" && ticketStatus != "Completed" && ticketStatus != "CREATE")
                            {
                                //Ticket Ref , Ticket number status already written to Email class object

                                TicketHandler.CreateTicketNote(emailApiHelper);
                                return;

                            }

                            #endregion

                        }

                    }

                }

            }

        }
        #endregion

        public static async Task<bool> SubjectExclusionKeyWords(string subject)
        {
            List<string> subjectExclusionKeyWordList = ManagementApiHelper.GetSubjectExclusionKeyWordsFromList().Result;
            RegexOptions options = RegexOptions.IgnoreCase;

            foreach (string item in subjectExclusionKeyWordList)
            {
                string pattern = "\\b" + item;

                Match match = Regex.Match(subject, pattern, options);

                // Use Regex.IsMatch to check if the list item is found in the email subject
                if (match.Success)
                {
                    return true;
                }

            }

            return false;
        }


        public static async Task<bool> SenderExclusionEmailAddresses(string from)
        {
            List<string> senderExclusionEmailList = ManagementApiHelper.GetSenderExclusionsFromList().Result;
            RegexOptions options = RegexOptions.IgnoreCase;

            foreach (string item in senderExclusionEmailList)
            {
                string pattern = "\\b" + item;

                Match match = Regex.Match(from, pattern, options);

                // Use Regex.IsMatch to check if the list item is found in the email subject
                if (match.Success)
                {
                    return true;
                }

            }

            return false;
        }


        /// <summary>
        /// If replied to e-mail from outlook returns text up to "From:"
        /// If e-mail sent to distro with ticket num in subject keeps the whole body of the message
        /// If replied from Gmail returns text upto "On:" where there is a pattern of "On:" + space + 3character + "," + space
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static String FindReturnMarkerPattern(string message)
        {
            string messageOriginal = message;
            List<string> patterns = new List<string> { "\\bFrom:", "\\bO\\s.{3}\\b\\,\\s" };

            RegexOptions options = RegexOptions.IgnoreCase;

            //string extractedText = string.Empty;

            foreach (string item in patterns)
            {
                Match match = Regex.Match(message, item, options);

                // Use Regex.IsMatch to check if the list item is found in the email subject
                if (match.Success)
                {
                    string subMessage = string.Empty;
                    int index = message.IndexOf(match.ToString());

                    if (index >= 0)
                    {
                        subMessage = message.Substring(0, index);

                    }

                    return subMessage;
                }

            }

            return messageOriginal;
        }


    }
}
