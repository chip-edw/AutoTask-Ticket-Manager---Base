using RestSharp;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public class ApiClient : IApiClient
    {
        private readonly RestClient _client;
        private readonly string _apiIntegrationCode;
        private readonly string _userName;
        private readonly string _secret;

        public ApiClient(string baseUrl, string apiIntegrationCode, string userName, string secret)
        {
            var options = new RestClientOptions
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 60000 // Timeout in milliseconds (e.g., 60000 ms = 1 minute)
            };

            _client = new RestClient(options);

            _apiIntegrationCode = apiIntegrationCode;
            _userName = userName;
            _secret = secret;
        }

        public RestResponse Get(string resource)
        {
            var request = new RestRequest(resource, Method.Get);
            request.AddHeader("ApiIntegrationCode", _apiIntegrationCode);
            request.AddHeader("UserName", _userName);
            request.AddHeader("Secret", _secret);
            request.AddHeader("Content-Type", "application/json");

            var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            return _client.Execute(request);
        }
    }
}
