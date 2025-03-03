using AutoTaskTicketManager_Base.AutoTaskAPI.Payloads;
using Newtonsoft.Json.Linq;
using Serilog;

namespace AutoTaskTicketManager_Base.AutoTaskAPI.Utilities
{
    public class AutoTaskResources
    {
        private readonly IApiClient _apiClient;

        public AutoTaskResources(IApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            Log.Debug($"AutoTaskResources initialized with IApiClient: {_apiClient != null}");
        }

        public async Task<int> RetrieveDefaultServiceDeskRoleID(int atUserId)
        {
            string resource = "Resources/query";

            try
            {
                var body = PayloadSerializer.Serialize(1, atUserId);
                var response = await _apiClient.PostAsync(resource, body);

                if (response == null)
                {
                    Log.Error("API response is null.");
                    return -1;
                }

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"API call failed with status code: {response.StatusCode}");
                    return -1;
                }

                //string rawResponse = await response.Content.ReadAsStringAsync();
                string rawResponse = response.Content;

                Log.Verbose("The web response is: " + rawResponse);

                var jsonResponse = JObject.Parse(rawResponse);

                // Assuming the response structure contains "items" array with the needed ID
                if (jsonResponse["items"] is JArray items && items.Count > 0)
                {
                    var firstItem = items[0];

                    if (firstItem["defaultServiceDeskRoleID"] != null &&
                        int.TryParse(firstItem["defaultServiceDeskRoleID"]?.ToString(), out int roleId))
                    {
                        return roleId;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error retrieving AutoTask Role ID - RetrieveDefaultServiceDeskRoleID(): {ex}");
            }

            return -1; // Default return value if nothing was found
        }
    }
}
