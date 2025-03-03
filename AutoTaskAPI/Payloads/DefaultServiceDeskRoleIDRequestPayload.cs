using Newtonsoft.Json;

namespace AutoTaskTicketManager_Base.AutoTaskAPI.Payloads
{
    internal class DefaultServiceDeskRoleIDRequestPayload
    {
        public int MaxRecords { get; set; }
        public List<string> IncludeFields { get; set; } = new();
        public List<FilterItem> Filter { get; set; } = new();
    }

    internal class FilterItem
    {
        public string op { get; set; }
        public string field { get; set; }
        public int value { get; set; }
        public bool udf { get; set; }
    }

    internal static class PayloadSerializer
    {
        public static string Serialize(int maxRecords, int filterValue)
        {
            var payload = new DefaultServiceDeskRoleIDRequestPayload
            {
                MaxRecords = maxRecords,
                IncludeFields = new List<string> { "defaultServiceDeskRoleID" },
                Filter = new List<FilterItem>
                {
                    new FilterItem
                    {
                        op = "eq",
                        field = "id",
                        value = filterValue,
                        udf = false
                    }
                }
            };
            var jsonString = JsonConvert.SerializeObject(payload, Formatting.Indented);

            return JsonConvert.SerializeObject(payload, Formatting.Indented);
        }
    }
}
