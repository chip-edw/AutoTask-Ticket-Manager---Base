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

        public void GetPicklistInformation()
        {
            string resource = "Tickets/entityinformation/fields";
            try
            {
                var response = _apiClient.Get(resource);

                if (response.IsSuccessStatusCode)
                {
                    string rawResponse = response.Content!;
                    Log.Verbose("The web response is:  " + rawResponse);

                    var unescapedResponse = JObject.Parse(rawResponse);

                    JArray a = (JArray)unescapedResponse["fields"];

                    //Get each picklist out of the entity field information returned from API
                    foreach (JObject item in a)
                    {
                        if (item.GetValue("isPickList").ToString().ToLower() == "true")
                        {
                            string isPickList = item.GetValue("isPickList").ToString().ToLower();
                            string name = item.GetValue("name").ToString();
                            string picklistValues = item.GetValue("picklistValues").ToString();
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
