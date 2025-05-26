using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.Dtos.Tickets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace AutoTaskTicketManager_Base.Services.Tickets
{
    public class TicketUIService : ITicketUIService
    {
        private readonly IApiClient _apiClient;

        public TicketUIService(IApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<List<TicketUI>> GetOpenTicketsAsync(int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;

            var query = new
            {
                filter = new[]
                {
                    new { op = "noteq", field = "status", value = 5 } // Exclude completed tickets (status = 5)
                },
                includeFields = new[]
                {
                    "id", "ticketNumber", "title", "status", "priority",
                    "queueID", "companyID", "createDate", "lastActivityDate"
                },
                maxRecords = pageSize,
                startRecord = offset
            };

            var response = await _apiClient.PostAsync("/Tickets/query", query);

            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                Log.Warning($"[TicketUIService] Failed to retrieve open tickets: {response.StatusCode} - {response.Content}");
                return new List<TicketUI>();
            }

            var json = JObject.Parse(response.Content);
            var items = (JArray?)json["items"];
            if (items == null) return new List<TicketUI>();

            var results = new List<TicketUI>();

            foreach (var item in items)
            {
                var companyId = item.Value<long?>("companyID") ?? 0;

                var dto = new TicketUI
                {
                    Id = item.Value<long?>("id") ?? 0,
                    TicketNumber = item.Value<string>("ticketNumber") ?? "",
                    Title = item.Value<string>("title") ?? "",
                    Status = MapPicklistLabel("status", item.Value<string>("status")),
                    Priority = MapPicklistLabel("priority", item.Value<string>("priority")),
                    QueueName = MapPicklistLabel("queueID", item.Value<string>("queueID")),
                    CompanyName = Companies.companies.TryGetValue(companyId, out var values) && values.Length > 0
                        ? values[0]?.ToString() ?? $"CompanyID: {companyId}"
                        : $"CompanyID: {companyId}",

                    CreatedDate = item.Value<DateTime?>("createDate") ?? DateTime.MinValue,
                    LastUpdated = item.Value<DateTime?>("lastActivityDate") ?? DateTime.MinValue
                };


                results.Add(dto);
            }

            return results;
        }


        public async Task<TicketUI?> GetTicketByNumberAsync(string ticketNumber)
        {
            var query = new
            {
                filter = new[]
                {
            new { op = "eq", field = "ticketNumber", value = ticketNumber }
        },
                includeFields = new[]
                {
            "id", "ticketNumber", "title", "status", "priority",
            "queueID", "companyID", "createDate", "lastActivityDate"
        },
                maxRecords = 1
            };

            var response = await _apiClient.PostAsync("/Tickets/query", query);

            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                Log.Warning($"[TicketUIService] Failed to retrieve ticket #{ticketNumber}: {response.StatusCode}");
                return null;
            }

            var json = JObject.Parse(response.Content);
            var item = ((JArray?)json["items"])?.FirstOrDefault();
            if (item == null) return null;

            var companyId = item.Value<long?>("companyID") ?? 0;

            return new TicketUI
            {
                Id = item.Value<long?>("id") ?? 0,
                TicketNumber = item.Value<string>("ticketNumber") ?? "",
                Title = item.Value<string>("title") ?? "",
                Status = MapPicklistLabel("status", item.Value<string>("status")),
                Priority = MapPicklistLabel("priority", item.Value<string>("priority")),
                QueueName = MapPicklistLabel("queueID", item.Value<string>("queueID")),

                CompanyName = Companies.companies.TryGetValue(companyId, out var values) && values.Length > 0
                    ? values[0]?.ToString() ?? $"CompanyID: {companyId}"
                    : $"CompanyID: {companyId}",

                CreatedDate = item.Value<DateTime?>("createDate") ?? DateTime.MinValue,
                LastUpdated = item.Value<DateTime?>("lastActivityDate") ?? DateTime.MinValue
            };
        }



        private static string MapPicklistLabel(string field, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";

            try
            {
                var picklist = Ticket.GetPickLists(field);
                foreach (var item in picklist)
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ToString());
                    if (dict != null && dict.TryGetValue("value", out var v) && v == value)
                        return dict.TryGetValue("label", out var label) ? label : value;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[TicketUIService] Error mapping picklist label for {field}:{value} - {ex.Message}");
            }

            return value; // fallback to raw value if mapping fails
        }
    }
}
