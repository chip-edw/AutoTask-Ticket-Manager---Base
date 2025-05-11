using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public class AutotaskAPIGet
    {

        // Centralized API credentials, misc settings, and options
        private static readonly string apiIntegrationCode = StartupConfiguration.GetConfig("APITrackingID");
        private static readonly string userName = StartupConfiguration.GetConfig("APIUsername");
        private static readonly string secret = StartupConfiguration.GetConfig("APIPassword");
        private static readonly string baseUrl = StartupConfiguration.GetConfig("RestfulBaseURL");


        //Create Centralized RestClient
        private static readonly RestClient client = new RestClient(new RestClientOptions
        {
            BaseUrl = new Uri(StartupConfiguration.GetConfig("RestfulBaseURL")),
            MaxTimeout = 240000 // Set timeout to 4 minutes (240000 ms)
        });

        private static IApiClient _apiClient;

        public static void Initialize(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        private readonly IPicklistService _picklistService;

        public AutotaskAPIGet(IPicklistService picklistService)
        {
            _picklistService = picklistService ?? throw new ArgumentNullException(nameof(picklistService));
        }


        // Retrieves the AutoTask Zone Information for the API user that is using this service
        // This method does not require authentication
        public static IDictionary<string, string> ZoneInformation()
        {
            string URLSuffix = "zoneinformation"; // ZoneInformation URL Suffix

            Log.Debug(" ZoneInformation reached & ATUser = " + userName);
            Log.Debug(" ZoneInformation reached & BaseUrl = " + baseUrl);
            Log.Debug(" ZoneInformation reached & URLSuffix = " + URLSuffix);

            var request = new RestRequest(URLSuffix + "?user=" + userName);
            var response = client.Execute(request)!;

            if (response.IsSuccessStatusCode == true)
            {
                string rawResponse = response.Content!;
                Log.Debug("The web response is:  " + rawResponse);
                Dictionary<string, string> rslt = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawResponse)!;

                Log.Debug("zoneName: " + rslt["zoneName"]);
                Log.Debug("url: " + rslt["url"]);
                Log.Debug("webUrl: " + rslt["webUrl"]);
                Log.Debug("ci: " + rslt["ci"]);

                return rslt;
            }

            else
            {
                Log.Warning("Issue retrieving web response. Success Status Code was not 'OK' for for GET Method ZoneInformation");

                IDictionary<string, string> rslt = new Dictionary<string, string>();
                rslt.Add(new KeyValuePair<string, string>("Failed", "Failed"));

                client.Dispose();

                return rslt;
            }


        }

        /// <summary>
        /// Gets all the Picklists from the field entity information for an AutoTask Ticket.
        /// Allows us to get all the current information for menu dropdowns like status into a dictionary at load since they can change as items are deleted or added.
        /// </summary>
        /// <returns></returns>
        public Task PicklistInformation(IPicklistService picklistService)
        {
            if (picklistService == null)
            {
                throw new ArgumentNullException(nameof(picklistService), "picklistService is null in PicklistInformation.");
            }

            // Loads the Ticket Entity Picklists
            picklistService.GetPicklistInformationAsync().Wait();

            return Task.CompletedTask;
        }



        /// <summary>
        /// Loads the information for AT companies of Type 'Customer' and updates the SQL DB with any newly added companies
        /// Will be used for multiple purposes. One of the primary purposes is to compare to the local persistance to ensure we add new companies
        /// to the Data Base once they have been added to AutoTask.
        /// </summary>
        public static string GetAutoTaskCompanies()
        {
            string CompanyTypeCustomer = "";

            try
            {
                // Get CompanyTypeCustomer
                CompanyTypeCustomer = StartupConfiguration.GetConfig("CompanyTypeCustomer");
                Log.Debug($"CompanyTypeCustomer: {CompanyTypeCustomer}");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving CompanyTypeCustomer from config.");
                return "Failed - Missing config key";
            }



            string resource = "/Companies/query?search=" +
                "{ \"filter\":[{\"op\" : \"eq\", \"field\":\"CompanyType\",\"value\":" + CompanyTypeCustomer + " }]}";

            var client = new RestClient(baseUrl);

            var request = new RestRequest(resource, Method.Get);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);
            request.AddHeader("Content-Type", "application/json");


            var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            if (response.IsSuccessStatusCode == true)

            {
                string rawResponse = response.Content!;
                Log.Verbose("The web response is:  " + rawResponse);

                var unescapedResponse = JObject.Parse(rawResponse);

                JArray a = (JArray)unescapedResponse["items"];

                //Drop all Entries from company dictionary for a fresh fill.
                Companies.companies.Clear();

                //Get each active company out of the entity field information returned from API
                foreach (JObject item in a)
                {
                    if (item.GetValue("id") != null)
                    {
                        Int64 id = (Int64)item.GetValue("id");
                        string companyName = item.GetValue("companyName").ToString();
                        string isActive = item.GetValue("isActive").ToString();
                        string[] plv = { companyName, isActive };

                        Companies.SetCompanies(id, plv);
                    }

                }

                Log.Debug($"Retrieved Autotask Companies of Company Type Customer. Count: {Companies.companies.Count}\n");
                return "Success - GetAutoTaskCompanies";
            }

            else
            {
                Log.Warning("Issue retrieving web response. Success Status Code was not 'OK' for for GET Method GetAutoTaskCompanies");

                return "Failed - Get Autotask Companies";
            }

        }


        /// <summary>
        /// This Method will return the count of AT companies with the Company Type of 'Customer'
        /// </summary>
        /// <returns></returns>
        public static string GetCompanyCount()
        {
            // Get CompanyTypeCustomer
            string CompanyTypeCustomer = StartupConfiguration.GetConfig("CompanyTypeCustomer");

            string resource = "/Companies/query/count?search=" +
                "{ \"filter\":[{\"op\" : \"eq\", \"field\" : \"CompanyType\", \"value\":" + CompanyTypeCustomer + "}]}";

            var request = new RestRequest(resource, Method.Get);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);
            request.AddHeader("Content-Type", "application/json");

            var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            if (response.IsSuccessStatusCode == true)

            {
                var unescapedContent = JObject.Parse(response.Content);

                string count = unescapedContent["queryCount"].ToString();

                Log.Verbose($"The Company Count of Type 'Customer' returned from the AT API is: {count} ");

                return count;
            }

            else
            {
                Log.Warning("Issue retrieving web response. Success Status Code was not 'OK' for for GET Method GetCompanyCount");

                return "Failed - Get Company Count";
            }

        }


        public static void GetCriticalTickets()
        {
            string resource = "/Companies/query/count?search={ \"filter\":[{\"op\" : \"exist\", \"field\" : \"id\" }]}";


            var request = new RestRequest(resource, Method.Get);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);
            request.AddHeader("Content-Type", "application/json");


            var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            if (response.IsSuccessStatusCode == true)

            {
                var unescapedContent = JObject.Parse(response.Content);

                string count = unescapedContent["queryCount"].ToString();

                Log.Verbose($"The Company Count Returned from the AT API is: {count} ");

                return;
            }

            else
            {
                Log.Warning("Issue retrieving web response. Success Status Code was not 'OK' for for GET Method GetCriticalTickets()");

                return;
            }

        }


        public static void GetNotCompletedTickets()
        {
            string resource = "/tickets/query";

            var request = new RestRequest(resource, Method.Post);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);
            request.AddHeader("Content-Type", "application/json");


            #region Request Body
            // The Status Value in the Body is the Value for the Status of 'Complete'
            // The queueID value is the value for 'Flintfox Application Support'
            var body = @"{
                " + "\n" +
                @"  ""MaxRecords"": 500,
                " + "\n" +
                @"  ""IncludeFields"": [
                " + "\n" +
                @"    ""ticketNumber"",
                " + "\n" +
                @"    ""title""
                " + "\n" +
                @"  ],
                " + "\n" +
                @"  ""Filter"": [
                " + "\n" +
                @"    {
                " + "\n" +
                @"        ""op"":""eq"",
                " + "\n" +
                @"        ""field"":""queueID"",
                " + "\n" +
                @"        ""value"": ""29683488""
                " + "\n" +
                @"    },
                " + "\n" +
                @"    {
                " + "\n" +
                @"        ""op"":""noteq"",
                " + "\n" +
                @"        ""field"":""Status"",
                " + "\n" +
                @"        ""value"":5
                " + "\n" +
                @"    }
                " + "\n" +
                @"  ]
                " + "\n" +
                @"}";

            #endregion

            Log.Debug("Getting the list of Open Tickets OKA Not Completed Tickets");

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            if (response.IsSuccessStatusCode == true)

            {
                Log.Debug($"AutoTaskApiGet.GetNotCompletedTickets() - Succesfully returned list of non Completed tickets from AT");

                JObject json = JObject.Parse(response.Content);
                JArray items = (JArray)json["items"];

                //Clear the StartupConfiguration.companiesTicketsNotCompleted Dictionary and load it with the TicketNumber and title of tickets returned 
                StartupConfiguration.companiesTicketsNotCompleted.Clear();

                foreach (JObject item in items)
                {
                    // Using the title as the key and the ticketNumber as the value
                    string ticketNumber = (string)item["ticketNumber"];
                    string title = (string)item["title"];

                    // There could already exist duplicates so just do not add another to the dictionary. Just grab the 1st one and log the rest.
                    // Duplicate Open tickets are automatically excluded from the Dictionaty as the Title of the Ticket is the Dictionary Key
                    try
                    {
                        StartupConfiguration.companiesTicketsNotCompleted.Add(title, ticketNumber);
                    }
                    catch
                    {
                        Log.Debug($"Detected a potential duplicate ticket title so not loading it into the dup checker Dictionary - StartupConfiguration.companiesTicketsNotCompleted");
                        Log.Debug($"Ticket Title: {title}\n");
                    }
                }

                Log.Verbose($"The count of Tickets 'not completed' Returned from the AT API is: {StartupConfiguration.companiesTicketsNotCompleted.Count} ");

                return;
            }

            else
            {
                Log.Warning("Issue retrieving web response. Success Status Code was not 'OK' for for GET Method GetCriticalTickets()");

                return;
            }

        }


        /// <summary>
        /// Loads the AppConfig.cs autoTaskResources dictionary with the active resources
        /// </summary>
        public static string GetAutoTaskActiveResources()
        {
            Log.Debug("Getting list of AutoTask Active Resources");

            // Filter for returning all the Active AutoTask Resources
            string resource = "/Resources/query?search=" +
                "{ \"filter\":[{ \"op\" : \"exist\", \"field\" : \"id\" }, { \"op\" : \"eq\", \"field\" :  \"isActive\", \"value\" : true}]}";


            var client = new RestClient(baseUrl);

            var request = new RestRequest(resource, Method.Get);
            request.AddHeader("ApiIntegrationCode", apiIntegrationCode);
            request.AddHeader("UserName", userName);
            request.AddHeader("Secret", secret);
            request.AddHeader("Content-Type", "application/json");


            var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            if (response.IsSuccessStatusCode == true)

            {
                string rawResponse = response.Content!;
                Log.Verbose("The web response is:  " + rawResponse);

                var unescapedResponse = JObject.Parse(rawResponse);

                JArray a = (JArray)unescapedResponse["items"];

                //Drop all Entries from autoTaskresources dictionary for a fresh fill.
                StartupConfiguration.autoTaskResources.Clear();

                //Get each active Resource out of the entity field information returned from API
                foreach (JObject item in a)
                {
                    if (item.GetValue("id") != null)
                    {
                        Int64 id = (Int64)item.GetValue("id");
                        string firstName = item.GetValue("firstName").ToString();
                        string lastName = item.GetValue("lastName").ToString();
                        string email = item.GetValue("email").ToString();
                        string[] plv = { firstName, lastName, email };

                        StartupConfiguration.autoTaskResources.Add(id, plv);
                    }
                }

                Log.Debug($"Active Autotask Resources loaded. Count: {StartupConfiguration.autoTaskResources.Count}\n");
                return "Success - GetAutoTaskResources";
            }

            else
            {
                Log.Warning("Issue retrieving web response. Success Status Code was not 'OK' for for GET Method GetAutoTaskActiveResources()");

                return "Failed - Get Autotask Resources";
            }

        }

    }
}
