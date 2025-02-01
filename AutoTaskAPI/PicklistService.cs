using Newtonsoft.Json.Linq;
using Serilog;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public class PicklistService : IPicklistService
    {
        private readonly IApiClient _apiClient;

        public PicklistService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task GetPicklistInformationAsync()
        {
            string resource = "Tickets/entityinformation/fields";
            try
            {
                var response = await _apiClient.GetAsync(resource);

                if (response.IsSuccessStatusCode)
                {
                    string rawResponse = response.Content!;
                    Log.Verbose("The web response is:  " + rawResponse);

                    var unescapedResponse = JObject.Parse(rawResponse);

                    JArray fields = (JArray)unescapedResponse["fields"];

                    //Get each picklist out of the entity field information returned from API
                    foreach (JObject field in fields)
                    {
                        if (field.GetValue("isPickList").ToString().ToLower() == "true")
                        {
                            string name = field.GetValue("name").ToString();
                            string picklistValues = field.GetValue("picklistValues").ToString();
                            Array plv = JArray.Parse(picklistValues).ToArray();

                            Ticket.SetPickLists(name, plv);
                        }

                    }
                    Log.Debug($"Auto Task PickLists Loaded. {Ticket.pickLists.Count} Items in Array \n");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error retrieving AutpTask Picklists - PicklistService.GetPicklistInformation(): - {ex}");
            }
        }
    }
}
