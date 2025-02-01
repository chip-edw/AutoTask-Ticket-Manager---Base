using AutoTaskTicketManager_Base.Common.Utilities;
using AutoTaskTicketManager_Base.MSGraphAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public static class TicketHandler
    {
        /// <summary>
        /// Creates an AT ticket for the AT Company ID passed by MSGraphAPI.CheckEmail
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public static async Task<string> CreateTicket(Int64 companyId, SecureEmailApiHelper emailApiHelper)
        {
            // Do some cleanup of the e-mail subject before creating the ticket title
            string rawSubject = EmailManager.GetField("Subject");
            string title = ContentProcessor.EmailSubjectCleanup(rawSubject);

            //check if Auto-Assign Flag set for AT Company
            bool autoAssign = StartupConfiguration.autoAssignCompanies.TryGetValue(companyId, out bool value);
            Log.Debug($"Auto Assign = {autoAssign} for CompanyID {companyId} \n ");

            //setup Rest Response
            RestResponse response;

            var baseUrl = StartupConfiguration.GetConfig("RestfulBaseURL");

            var resource = "/Tickets/";
            Log.Debug("...");
            Log.Information($"Create AT Ticket API request to: " + baseUrl + resource);

            // Create Ticket and Get Ticket ID

            //Get API Credentials for inclusion in the Http headers
            var apiIntegrationCode = StartupConfiguration.GetConfig("APITrackingID");
            var userName = StartupConfiguration.GetConfig("APIUsername");
            var secret = StartupConfiguration.GetConfig("APIPassword");
            string coId = (string)companyId.ToString();


            //Update the E-mail subject in the Email class so it matches the cleaned up ticket title.
            //(Will be accessed later when creating the reply All e-mail)
            EmailManager.SetField("Subject", title);

            //Get subject to make sure it is correct.
            string test = EmailManager.GetField("Subject");

            string issueType = StartupConfiguration.GetConfig("IssueType");
            string subIssueType = StartupConfiguration.GetConfig("SubIssueType");

            string descriptionText = "";

            //string description = EmailManager.GetField("BodyPreview");
            //Let's use the body of the e-mail and just make sure it is less than 2000 characters
            //which is the max length of the AT Ticket Description.
            string descriptionRaw = EmailManager.GetField("Body");


            #region Formats and cleanup


            try
            {

                //Convert Escaped HTML to Text and Normalize - New
                descriptionText = await ContentProcessor.ConvertHtmlToTextAndNormalize(descriptionRaw, emailApiHelper);

            }
            catch (Exception ex)
            {
                Log.Error("...");
                Log.Error("TicketHandler.CreateTicket: " + ex);
                Log.Error("AT Ticket not Created..");

                return "exception";

            }


            //Clean up some mess
            string description = null;
            var removalList = new List<string> { "&lt", "&gt" };
            var substitutionList = new List<string> { "\"", "\'" };

            //deal with quotes in title
            foreach (string c in substitutionList)
            {
                title = title.Replace(c.ToString(), "\\" + c);
            }


            foreach (string c in removalList)
            {
                description = descriptionText.Replace(c.ToString(), " ");
            }

            //Autotask is not escaping a single " \ ". So we need to
            var replacementList = new List<string> { @" \ ", " !@# " };

            foreach (string b in replacementList)
            {
                description = description.Replace(b.ToString(), " / ");
            }


            // Make sure we do not exceed the AT Ticket allowed characters for the Description 
            if (description.Length > 1500)
            {
                description = description.Substring(0, 1500);
            }

            #endregion


            #region Example Use of the SecureEmailApiHelper Singleton
            static async Task HandleTicketAsync(string ticketId, SecureEmailApiHelper emailApiHelper)
            {
                try
                {
                    // Your ticket handling logic
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Log.Error($"An error occurred while handling ticket {ticketId}: {ex}");

                    // Send an administrative email
                    await SendAdministrativeEmailAsync(ex, ticketId, emailApiHelper);
                }
            }

            static async Task SendAdministrativeEmailAsync(Exception exception, string ticketId, SecureEmailApiHelper emailApiHelper)
            {
                try
                {
                    string subject = $"Error in Ticket Handler - Ticket ID: {ticketId}";
                    string body = $"An error occurred while processing ticket {ticketId}.\n\n" +
                                  $"Exception Details:\n{exception}";

                    string adminEmail = "admin@example.com"; // Replace with actual admin email
                    string url = "https://graph.microsoft.com/v1.0/me/sendMail"; // MS Graph endpoint
                    string accessToken = "ACCESS_TOKEN"; // Replace with actual token retrieval logic

                    await emailApiHelper.SendEmailAsync(url, accessToken, subject, body, adminEmail);
                }
                catch (Exception emailException)
                {
                    Log.Error($"Failed to send administrative email: {emailException}");
                }
            }

            #endregion

            string messageId = EmailManager.GetField("Id");

            var options = new RestClientOptions
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 60000 * 2 // Timeout in milliseconds (e.g., 60000 ms = 1 minute)
            };


            var client = new RestClient(options);


            //Get Auto assignments and assign accordingly
            if (autoAssign == true)
            {
                var assignments = StartupConfiguration.GetAutoAssignMembers(companyId);

                string leadFunctionalId = assignments[0].ToString();
                string leadFunctionalName = assignments[1].ToString();
                string leadFunctionalEmail = assignments[2].ToString();

                string leadTechnicalId = assignments[3].ToString();
                string leadTechnicalName = assignments[4].ToString();
                string leadTechnicalEmail = assignments[5].ToString();


                string jsonFromAddress = EmailManager.GetField("From");
                JObject jsonFrom = JObject.Parse(jsonFromAddress);

                string fromAddress = jsonFrom["emailAddress"]["address"].ToString();

                //Get "FunctionalResourceRoleId" for Functional Resources
                var functionalResourceRoleID = StartupConfiguration.GetConfig("FunctionalResourceRoleId");

                //Get "TechnicalResourceRoleId" for Technical Resources
                var technicalResourceRoleId = StartupConfiguration.GetConfig("TechnicalResourceRoleId");

                //resourceRoleID is set to either the Technical or Functional resource role ID based on Assignment logic
                int resourceRoleID = 123;
                int assignedResource = 123;

                //Autoassigmnent logic
                if (assignments != null && fromAddress != null)
                {
                    if (string.Equals(fromAddress.Trim(), leadFunctionalEmail.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Debug($"Assign ticket to Lead Technical: {leadTechnicalName} \n");
                        resourceRoleID = Convert.ToInt32(technicalResourceRoleId);
                        assignedResource = Convert.ToInt32(leadTechnicalId);
                    }
                    else if (string.Equals(fromAddress.Trim(), leadTechnicalEmail.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Debug($"Assign ticket to Lead Technical: {leadTechnicalName} \n");
                        resourceRoleID = Convert.ToInt32(technicalResourceRoleId);
                        assignedResource = Convert.ToInt32(leadTechnicalId);
                    }
                    else
                    {
                        Log.Debug($"Assign ticket to Lead Functional = {leadFunctionalName} \n");
                        resourceRoleID = Convert.ToInt32(functionalResourceRoleID);
                        assignedResource = Convert.ToInt32(leadFunctionalId);
                    }
                }

                // Create Ticket And AutoAssign

                var requestAutoAssign = new RestRequest(resource, Method.Post);
                requestAutoAssign.AddHeader("ApiIntegrationCode", apiIntegrationCode);
                requestAutoAssign.AddHeader("UserName", userName);
                requestAutoAssign.AddHeader("Secret", secret);
                requestAutoAssign.AddHeader("Content-Type", "application/json");
                var autoAssignBody = @"{" + "\n" +

                @"    ""companyID"":" + (string)coId + "," + "\n" +
                @"    ""QueueID"": 29683488," + "\n" +
                @"    ""dueDateTime"": ""2030-03-21T00:00:00""," + "\n" +
                @"    ""priority"": 3," + "\n" +
                @"    ""status"": 1," + "\n" +
                @"    ""source"": 8," + "\n" +
                @"    ""issueType"":" + issueType + "," + "\n" +
                @"    ""subIssueType"":" + subIssueType + "," + "\n" +
                @"    ""title"":" + '"' + (string)title + '"' + "," + "\n" +
                @"    ""description"":" + '"' + (string)description + '"' + "," + "\n" +
                @"    ""assignedResourceID"" :" + assignedResource + "," + "\n" +
                @"    ""assignedResourceRoleID"" :" + resourceRoleID + "," + "\n" +
                @"" + "\n" +
                @"}";
                requestAutoAssign.AddParameter("application/json", autoAssignBody, ParameterType.RequestBody);



                //add response validation and create ticket on success and deal with failure
                //##########################################################################
                response = client.Execute(requestAutoAssign);


                try
                {
                    if (response.IsSuccessful == true)
                    {
                        //Deserialise response.Content returned from ticket creation
                        string data = response.Content;
                        Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                        string ticketRef = values["itemId"];

                        Log.Information($"The Ticket Reference is: {ticketRef}");

                        //Update Email class with the ticketId or ref
                        EmailManager.SetField("AtTicketId", ticketRef);

                        //get the ticket number we just created and update Email Class with Ticket Number
                        var ticketNumber = GetTicketField(ticketRef, "ticketNumber");
                        EmailManager.SetField("AtTicketNo", ticketNumber);

                        //##########################################################################

                        //Add the new Ticket Clean and raw title and Ticket number to the Dictionary 'StartupConfiguration.companiesTicketsNotCompleted' for dup prevention
                        StartupConfiguration.companiesTicketsNotCompleted.Add(rawSubject, ticketNumber);
                        if (rawSubject != title)
                        {
                            StartupConfiguration.companiesTicketsNotCompleted.Add(title, ticketNumber);
                        }


                        Log.Debug($" ... Added Ticket raw and cleaned Title to Tracker Dictionary ... ");


                        //Update the subject of Email class with AT Ticket Number
                        string Subject = EmailManager.GetField("Subject");
                        Subject = "[" + ticketNumber + "]" + " " + Subject;
                        EmailManager.SetField("Subject", Subject);

                        Log.Debug($"The updated subject is now: {EmailManager.GetField("Subject")}");


                        Log.Verbose($"The Ticket Reference ID is: {ticketRef}");
                        Log.Verbose($"The Ticket Number is: {ticketNumber}");

                        //Get the Base64 string representation of the e-mail we want to attach to the AT ticket.
                        var base64String = await EmailManager.MSGraphGetCompleteMessageById(messageId);
                        string base64Data = base64String.ToString();

                        //Pass information about the MS Graph E-mail Message to the method that will
                        //manage the process of attaching the e-mail that generated the AT ticket to the ticket.
                        CreateTicketEmailAttachment(ticketRef, Subject, base64Data);




                        return ticketRef;
                    }
                    else
                    {
                        //Deserialise response.Content returned from ticket creation
                        string data = response.Content;
                        Log.Error($"TicketHandler.cs.CreateTicket - AutoAssign - AutoTask API Error: {data} \n");
                    }

                }
                catch (Exception ex)
                {
                    Log.Information($"Ticket Handler http call issue: {ex}");
                    return "exception";
                }

            }


            // This is where we check if the sender should be assigned as the Primary resource of the Ticket
            //The way it works is that early on in the MSGraphAPI.ProcessEmail class we check if the sender's e-mail is in the StartupConfiguration AutoAssignSenders Dictionary
            //if it is in the dictionary and the resource is 'Active' then we log it and update the Email class and make the SenderAssignment 'True'
            //If it is True then we ise the below else if statement to assign to the sender.
            //see the bool senderAssignmentExists in the 
            else if (EmailManager.GetBool("SenderAssignment") == true)
            {
                string resourceRole = string.Empty;
                int resourceRoleID = 123;
                int assignedResource = 123;
                bool resourceActive = false;

                //Cleanup From (Sender) e-mail address for futhur processing
                var senderAddress = ContentProcessor.SenderCleanup(EmailManager.GetField("From"), 0);

                var assignments = StartupConfiguration.autoAssignSenders;

                // Iterate through dictionary and get resource Id that corresponds with the Sender From address
                foreach (var entry in assignments)
                {
                    // Check if the email exists in the object[] values
                    if (Array.Exists(entry.Value, element => element.ToString() == senderAddress))
                    {
                        assignedResource = Convert.ToInt32(entry.Key); // Return the key as soon as the email is found


                        //Get the resource role
                        resourceRole = entry.Value[2].ToString();

                        //Use the correct default ResourceRoldId based on resource role
                        if (resourceRole == "Technical")
                        {
                            resourceRoleID = Convert.ToInt32(StartupConfiguration.GetConfig("TechnicalResourceRoleId"));

                        }
                        if (resourceRole == "Functional")
                        {
                            resourceRoleID = Convert.ToInt32(StartupConfiguration.GetConfig("FunctionalResourceRoleId"));
                        }

                        break;
                    }
                }



                var request = new RestRequest(resource, Method.Post);
                request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
                request.AddHeader("UserName", userName);
                request.AddHeader("Secret", secret);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{" + "\n" +

                @"    ""companyID"":" + (string)coId + "," + "\n" +
                @"    ""QueueID"": 29683488," + "\n" +
                @"    ""dueDateTime"": ""2030-03-21T00:00:00""," + "\n" +
                @"    ""priority"": 3," + "\n" +
                @"    ""status"": 1," + "\n" +
                @"    ""source"": 8," + "\n" +
                @"    ""issueType"":" + issueType + "," + "\n" +
                @"    ""subIssueType"":" + subIssueType + "," + "\n" +
                @"    ""title"":" + '"' + (string)title + '"' + "," + "\n" +
                @"    ""description"":" + '"' + (string)description + '"' + "," + "\n" +
                @"    ""assignedResourceID"" :" + assignedResource + "," + "\n" +
                @"    ""assignedResourceRoleID"" :" + resourceRoleID + "," + "\n" +
                @"" + "\n" +
                @"}";
                request.AddParameter("application/json", body, ParameterType.RequestBody);

                var apiRequestBody = request.ToString();

                response = client.Execute(request);

                //add response validation and create ticket on success and deal with failure
                //##########################################################################
                try
                {
                    if (response.IsSuccessful == true)
                    {
                        //Deserialise response.Content returned from ticket creation
                        string data = response.Content;
                        Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                        string ticketRef = values["itemId"];

                        Log.Information($"The Ticket Reference is: {ticketRef}");

                        //Update Email class with the ticketId or ref
                        EmailManager.SetField("AtTicketId", ticketRef);

                        //get the ticket number we just created and update Email Class with Ticket Number
                        var ticketNumber = GetTicketField(ticketRef, "ticketNumber");
                        EmailManager.SetField("AtTicketNo", ticketNumber);


                        //##########################################################################

                        //Add the new Ticket Clean and raw title and Ticket number to the Dictionary 'StartupConfiguration.companiesTicketsNotCompleted' for dup prevention
                        StartupConfiguration.companiesTicketsNotCompleted.Add(rawSubject, ticketNumber);
                        if (rawSubject != title)
                        {
                            StartupConfiguration.companiesTicketsNotCompleted.Add(title, ticketNumber);
                        }


                        Log.Debug($" ... Added Ticket raw and cleaned Title to Tracker Dictionary... ");


                        //Update the subject of Email class with AT Ticket Number
                        string subject = EmailManager.GetField("Subject");
                        subject = "[" + ticketNumber + "]" + " " + subject;
                        EmailManager.SetField("Subject", subject);

                        Log.Debug($"The updated subject is now: {EmailManager.GetField("Subject")}");


                        Log.Verbose($"The Ticket Reference ID is: {ticketRef}");
                        Log.Verbose($"The Ticket Number is: {ticketNumber}");

                        //Get the Base64 string representation of the e-mail we want to attach to the AT ticket.
                        var base64String = await EmailManager.MSGraphGetCompleteMessageById(messageId);
                        string base64Data = base64String.ToString();

                        //Pass information about the MS Graph E-mail Message to the method that will
                        //manage the process of attaching the e-mail that generated the AT ticket to the ticket.
                        CreateTicketEmailAttachment(ticketRef, subject, base64Data);


                        return ticketRef;
                    }
                    else
                    {
                        //Deserialise response.Content returned from ticket creation
                        string data = response.Content;
                        Log.Error($"TicketHandler.cs.CreateTicket - AutoTask API Error: {data} \n");
                    }

                }
                catch (Exception ex)
                {
                    Log.Information($"Ticket Handler http call issue: {ex}");
                    return "exception";
                }



            }

            else
            {
                var request = new RestRequest(resource, Method.Post);
                request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
                request.AddHeader("UserName", userName);
                request.AddHeader("Secret", secret);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{" + "\n" +

                @"    ""companyID"":" + (string)coId + "," + "\n" +
                @"    ""QueueID"": 29683488," + "\n" +
                @"    ""dueDateTime"": ""2030-03-21T00:00:00""," + "\n" +
                @"    ""priority"": 3," + "\n" +
                @"    ""status"": 1," + "\n" +
                @"    ""source"": 8," + "\n" +
                @"    ""issueType"":" + issueType + "," + "\n" +
                @"    ""subIssueType"":" + subIssueType + "," + "\n" +
                @"    ""title"":" + '"' + (string)title + '"' + "," + "\n" +
                @"    ""description"":" + '"' + (string)description + '"' + "," + "\n" +
                @"" + "\n" +
                @"}";
                request.AddParameter("application/json", body, ParameterType.RequestBody);

                response = client.Execute(request);

                //add response validation and create ticket on success and deal with failure
                //##########################################################################
                try
                {
                    if (response.IsSuccessful == true)
                    {
                        //Deserialise response.Content returned from ticket creation
                        string data = response.Content;
                        Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                        string ticketRef = values["itemId"];

                        Log.Information($"The Ticket Reference is: {ticketRef}");

                        //Update Email class with the ticketId or ref
                        EmailManager.SetField("AtTicketId", ticketRef);

                        //get the ticket number we just created and update Email Class with Ticket Number
                        var ticketNumber = GetTicketField(ticketRef, "ticketNumber");
                        EmailManager.SetField("AtTicketNo", ticketNumber);


                        //##########################################################################

                        //Add the new Ticket Clean and raw title and Ticket number to the Dictionary 'StartupConfiguration.companiesTicketsNotCompleted' for dup prevention
                        StartupConfiguration.companiesTicketsNotCompleted.Add(rawSubject, ticketNumber);
                        if (rawSubject != title)
                        {
                            StartupConfiguration.companiesTicketsNotCompleted.Add(title, ticketNumber);
                        }


                        Log.Debug($" ... Added Ticket raw and cleaned Title to Tracker Dictionary... ");


                        //Update the subject of Email class with AT Ticket Number
                        string subject = EmailManager.GetField("Subject");
                        subject = "[" + ticketNumber + "]" + " " + subject;
                        EmailManager.SetField("Subject", subject);

                        Log.Debug($"The updated subject is now: {EmailManager.GetField("Subject")}");


                        Log.Verbose($"The Ticket Reference ID is: {ticketRef}");
                        Log.Verbose($"The Ticket Number is: {ticketNumber}");

                        //Get the Base64 string representation of the e-mail we want to attach to the AT ticket.
                        var base64String = await EmailManager.MSGraphGetCompleteMessageById(messageId);
                        string base64Data = base64String.ToString();

                        //Pass information about the MS Graph E-mail Message to the method that will
                        //manage the process of attaching the e-mail that generated the AT ticket to the ticket.
                        CreateTicketEmailAttachment(ticketRef, subject, base64Data);


                        return ticketRef;
                    }
                    else
                    {
                        //Deserialise response.Content returned from ticket creation
                        string data = response.Content;
                        Log.Error($"TicketHandler.cs.CreateTicket - AutoTask API Error: {data} \n");
                    }

                }
                catch (Exception ex)
                {
                    Log.Information($"Ticket Handler http call issue: {ex}");
                    return "exception";
                }

            }

            return "exception";
        }


        /// <summary>
        /// Manages the creation of an AT ticket attachment of the E-mail prefixed with the AT ticket number
        /// of the the e-mail that raised the AT ticket.
        /// </summary>
        /// <param name="atId"></param> this is the AT ticket Reference number that contains all the information about the ticket
        /// it is not the ticket number.
        /// <param name="title"></param> this is going to be the subject of the e-mail we are attaching prefixed with the AT ticket number.
        /// <param name="data"></param> this is the Base64 string representation of the e-mail.
        public static async void CreateTicketEmailAttachment(string atId, string title, string data)
        {
            //make sure we name the email to attach with the .eml format
            string filename = title + ".eml";

            var baseUrl = StartupConfiguration.GetConfig("RestfulBaseURL");

            var resource = "Tickets/" + atId + "/Attachments";

            Log.Debug($"\n Create AT Ticket Email Attachment API request to: " + baseUrl + resource);

            //Get API Credentials for inclusion in the Http headers
            var apiIntegrationCode = StartupConfiguration.GetConfig("APITrackingID");
            var userName = StartupConfiguration.GetConfig("APIUsername");
            var secret = StartupConfiguration.GetConfig("APIPassword");


            var options = new RestClientOptions
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 60000 * 2 // Timeout in milliseconds (e.g., 60000 ms = 1 minute)
            };

            var client = new RestClient(options);

            var request = new RestRequest(resource, Method.Post);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);

            request.AddHeader("Content-Type", "application/json");
            var body = @"{" + "\n" +
            @"    ""id"": 0," + "\n" +
            @"    ""attachDate"": ""2020-10-05T10:59:57.537""," + "\n" +
            @"    ""attachedByContactID"": null," + "\n" +
            @"    ""attachedByResourceID"": null," + "\n" +
            @"    ""attachmentType"": ""FILE_ATTACHMENT""," + "\n" +
            @"    ""fullPath"":" + '"' + filename + '"' + "," + "\n" +
            @"    ""publish"": 1," + "\n" +
            @"    ""title"":" + '"' + title + '"' + "," + "\n" +
            @"    ""data"":" + '"' + data + '"' + "\n" +
            @"}";

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            // Need some error handling here.
            if (response.IsSuccessStatusCode != true || response.IsSuccessful != true)
            {
                string errMsg = (string)response.Content;
                Log.Error($"TicketHandler.CreateTicketEmailAttachment - {errMsg}");
            }

        }

        /// <summary>
        /// Returns the value of the ticket field requested
        /// ticketRef is the autoTaskId which contains the ticket information including the ticket number
        /// </summary>
        /// <param name="ticketRef"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetTicketField(string ticketRef, string fieldName)
        {
            var baseUrl = StartupConfiguration.GetConfig("RestfulBaseURL");

            var resource = "/Tickets/" + ticketRef + "/";
            Log.Debug("...");
            Log.Information($"Get autoTaskId API request to: " + baseUrl + resource);
            Log.Debug("...");

            //Get API Credentials for inclusion in the Http headers
            var apiIntegrationCode = StartupConfiguration.GetConfig("APITrackingID");
            var userName = StartupConfiguration.GetConfig("APIUsername");
            var secret = StartupConfiguration.GetConfig("APIPassword");

            var options = new RestClientOptions
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 60000 * 2 // Timeout in milliseconds (e.g., 60000 ms = 1 minute)
            };


            var client = new RestClient(options);


            var request = new RestRequest(resource, Method.Get);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);
            request.AddHeader("Content-Type", "application/json");

            var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            //add response validation and retrieve ticket Number on success and deal with failure
            //##########################################################################

            //Log the response which should be a full Json representation of the ticket
            Log.Verbose(response.Content);

            try
            {
                if (response.IsSuccessful == true)
                {
                    //Deserialise response.Content returned from ticket creation

                    JObject rslt = JObject.Parse(response.Content);
                    var item = rslt["item"];

                    var value = item[fieldName].ToString();

                    return value;
                }

            }
            catch (Exception ex)
            {
                Log.Information($"Ticket Handler http call issue: {ex}");
                return "exception";
            }

            return "exception";
        }


        #region "public static string GetTicketByNumberUpdateTicketClass(string ticketNum)"
        /// <summary>
        /// Retrieves the AutoTask ticket from Autotask based on the Ticket Number. Loads the ticket into the Ticket class.
        /// returns ticket "status" if ticket exists and ticket load successful
        /// returns "None" if ticket does not exist
        /// </summary>
        /// <param name="ticketNum"></param>
        /// <returns></returns>
        public static string GetTicketByNumberUpdateTicketClass(string ticketNum, bool byPassAtCompanyId = false)
        {
            var baseUrl = StartupConfiguration.GetConfig("RestfulBaseURL");

            var resource = "/Tickets/query?search={ \"filter\": [ {\"op\" : \"eq\", " +
                "\"field\" : \"ticketNumber\", \"value\":" + '"' + ticketNum + '"' + " }]}";

            Log.Debug($"\n Get AT Ticket by Ticket Number API request to: " + baseUrl + resource);
            Log.Debug("...");

            //Get API Credentials for inclusion in the Http headers
            var apiIntegrationCode = StartupConfiguration.GetConfig("APITrackingID");
            var userName = StartupConfiguration.GetConfig("APIUsername");
            var secret = StartupConfiguration.GetConfig("APIPassword");

            var options = new RestClientOptions
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 60000 * 2// Timeout in milliseconds (e.g., 60000 ms = 1 minute)
            };

            var client = new RestClient(options);

            var request = new RestRequest(resource, Method.Get);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);
            request.AddHeader("Content-Type", "application/json");

            var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            //add response validation and retrieve ticket Number on success and deal with failure
            //##########################################################################

            //Log the response which should be a full Json representation of the ticket
            Log.Verbose("");
            Log.Verbose("Ticket: JSON Representation of returned ticket: {response.Content}");
            Log.Verbose("");

            try
            {
                if (response.IsSuccessful == true)
                {
                    //Deserialise 'response.Content' returned from ticket creation. This is the Ticket in JSON.
                    JObject rslt = JObject.Parse(response.Content);

                    int count = (int)rslt["pageDetails"]["count"];

                    //Check for a ticket count of 1. This indicates an existing ticket.
                    if (count > 0)
                    {
                        // Get ticket status, companyID and Autotask ID of ticket
                        try
                        {
                            string statusLabel = "";

                            //companyId on ticket must match 'AtCompanyId' we saved in Email class when validating Support Distro.
                            int companyId = (int)rslt["items"][0]["companyID"];
                            int AtCompanyId = EmailManager.GetAtCompanyId();

                            if (companyId != AtCompanyId && byPassAtCompanyId == false)
                            {
                                Log.Error($"TicketHandler.GetTicketByNumberUpdateTicketClass: The companyID of: {companyId} returned" +
                                    $"from the ticket does not match" + $"the expected value of: {AtCompanyId}" +
                                    $" returned from EmailManager.GetAtCompanyId() which was derived from " +
                                    $"the SupportDistro matching AT company: .");

                                return "!";
                            }
                            string status = (string)rslt["items"][0]["status"];


                            // Need to convert the representation of the INT  returned to the picklist display name.
                            string autotaskId = (string)rslt["items"][0]["id"];

                            Array picklistLabel = Ticket.GetPickLists("status");
                            foreach (var item in picklistLabel)
                            {
                                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ToString());
                                if (values["value"] == status)
                                {
                                    statusLabel = (string)values["label"];
                                    break;
                                }

                            }

                            // Update the Ticket class with autoTaskId and status
                            TicketHandler.SetField("autoTaskId", autotaskId);
                            TicketHandler.SetField("status", statusLabel);

                            return statusLabel;
                        }
                        catch (Exception ex)
                        {
                            Log.Warning($"There has been an exception TicketHandler.cs updating the Ticket Class{ex.ToString()}");
                            //Send Admin Email here

                        }


                    }

                    //If no ticket return "None"
                    return "None";
                }

            }
            catch (Exception ex)
            {
                Log.Warning($"Ticket Handler http call issue: {ex}");
                return "None";
            }

            return "None";
        }
        #endregion

        /// <summary>
        /// Takes email subject and parses looking for an AutoTask Ticket Number
        /// If Ticket Number found queries Autotask to see if the ticket exists and returns the ticket status
        /// The Status for not found is 'None'
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static string GetTicketStatusFromEmailSubject(string subject)
        {
            //pattern match for ticket in subject
            string pattern = "\\bT20\\d{6}[.]{1}\\d{4}|\\bT20\\d{6}[.]{1}\\d{3}";
            Match match = Regex.Match(subject, pattern);
            if (match.Success)
            {
                string ticketNum = match.Value.Trim();
                Log.Debug($"Found Ticket Number Pattern in Subject of Email: {ticketNum}");

                string ticketStatus = GetTicketByNumberUpdateTicketClass(ticketNum);
                //Ticketnum and ID were already written to the Ticket Class Object

                return ticketStatus;
            }
            else
            {
                return "CREATE";
            }

        }

        /// <summary>
        /// Creates a Ticket note from the body of the e-mail message stored in the Email class
        /// </summary>
        /// <param name="ticketRef"></param>
        /// <returns></returns>
        public static async Task CreateTicketNote(SecureEmailApiHelper emailApiHelper)
        {
            //Get Body of Email Message
            // How many rows do we want from the body of the e-mail? Will need to decide. For now will pick 1500 characters

            string messageBody = EmailManager.GetField("Body");
            string description = await ContentProcessor.ConvertHtmlToTextAndNormalize(messageBody, emailApiHelper);

            string sender = EmailManager.GetField("Sender");

            Thread.Sleep(500); //sleep 500 ms because I was to lazy to go back and  change the EmailManager.GetField() method
            //to awaitable because there were 27 references I would have had to update. Will do later

            var title = ContentProcessor.SenderCleanup(sender);

            Console.WriteLine(title);

            var removalList = new List<string> { "&lt", "&gt" };
            foreach (string c in removalList)
            {
                description = description.Replace(c.ToString(), " ");
            }


            //Get Autotask Id we saved to Ticket Class Object
            string atId = TicketHandler.GetClassField("autoTaskId");

            var baseUrl = StartupConfiguration.GetConfig("RestfulBaseURL");

            var resource = "Tickets/" + atId + "/Notes";

            Log.Debug($"\n Create AT Ticket Note API request to: " + baseUrl + resource);
            Log.Debug("...");

            //Get API Credentials for inclusion in the Http headers
            var apiIntegrationCode = StartupConfiguration.GetConfig("APITrackingID");
            var userName = StartupConfiguration.GetConfig("APIUsername");
            var secret = StartupConfiguration.GetConfig("APIPassword");


            var options = new RestClientOptions
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 60000 * 2 // Timeout in milliseconds (e.g., 60000 ms = 1 minute)
            };

            var client = new RestClient(options);

            var request = new RestRequest(resource, Method.Post);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);

            request.AddHeader("Content-Type", "application/json");
            var body = @"{" + "\n" +

            @"	""TicketID"":" + (string)atId + "," + "\n" +
            @"    ""Description"":" + '"' + (string)description + '"' + "," + "\n" +
            @"    ""NoteType"": 1," + "\n" +
            @"    ""Publish"": 1," + "\n" +
            @"	  ""Title"":" + '"' + (string)title.ToString() + '"' + "\n" +
            @"" + "\n" +
            @"}";

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            // Need some error handling here.
            if (response.IsSuccessStatusCode != true || response.IsSuccessful != true)
            {
                string errMsg = (string)response.Content;
                Log.Error($"TicketHandler.CreateTicketNote - {errMsg}");
            }
            else
            {
                Log.Debug("Ticket Note Added... \n\n");
            }

        }

        public static string SetField(string fieldName, string fieldValue)
        {

            if (fieldName.First().ToString() != "@") //We need to omit dealing with "@" at this time and we aren't using it anyway.

            {
                //Try to populate the class field
                try
                {
                    var field = typeof(Ticket).GetField(fieldName,
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
                    Log.Debug($"{fieldName} does not exist as a class field");

                }

            }

            return ($"{fieldName} does not exist as a class field");

        }

        public static string GetClassField(string fieldId)
        {
            var field = typeof(Ticket).GetField(fieldId,
                BindingFlags.Static | BindingFlags.NonPublic);

            var retValue = field!.GetValue(fieldId);

            return (string)retValue!;
        }
    }
}
