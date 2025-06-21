using RestSharp;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public class ApiClient : IApiClient
    {
        private readonly RestClient _client;
        private readonly string _apiIntegrationCode;
        private readonly string _userName;
        private readonly string _secret;

        public ApiClient(IConfiguration configuration)
        {
            _apiIntegrationCode = StartupConfiguration.GetProtectedSetting("APITrackingID");
            _userName = StartupConfiguration.GetProtectedSetting("APIUsername");
            _secret = StartupConfiguration.GetProtectedSetting("APIPassword");

            var baseUrl = StartupConfiguration.GetProtectedSetting("RestfulBaseURL");
            _client = new RestClient(new RestClientOptions
            {
                BaseUrl = new Uri(baseUrl),
                MaxTimeout = 240000 // 4 minutes timeout
            });
        }

        private void AddHeaders(RestRequest request)
        {
            request.AddHeader("ApiIntegrationCode", _apiIntegrationCode);
            request.AddHeader("UserName", _userName);
            request.AddHeader("Secret", _secret);
            request.AddHeader("Content-Type", "application/json");
        }


        public async Task<RestResponse> GetAsync(string resource)
        {
            var request = new RestRequest(resource, Method.Get);
            AddHeaders(request);
            return await _client.ExecuteAsync(request);
        }

        public async Task<RestResponse> PostAsync(string resource, object body)
        {
            var request = new RestRequest(resource, Method.Post);
            AddHeaders(request);
            request.AddJsonBody(body);
            return await _client.ExecuteAsync(request);
        }

        public async Task<RestResponse> PatchAsync(string resource, object body)
        {
            var request = new RestRequest(resource, Method.Patch);
            AddHeaders(request);
            request.AddJsonBody(body);
            return await _client.ExecuteAsync(request);
        }

        public async Task<RestResponse> PutAsync(string resource, object body)
        {
            var request = new RestRequest(resource, Method.Put);
            request.AddJsonBody(body);
            return await _client.ExecuteAsync(request);
        }

        public async Task<RestResponse> DeleteAsync(string resource)
        {
            var request = new RestRequest(resource, Method.Delete);
            AddHeaders(request);
            return await _client.ExecuteAsync(request);
        }

    }
}
