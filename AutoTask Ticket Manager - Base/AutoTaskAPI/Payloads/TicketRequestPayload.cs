using Newtonsoft.Json;


namespace AutoTaskTicketManager_Base.AutoTaskAPI.Payloads
{
    internal class TicketRequestPayload
    {
        public string CompanyID { get; set; }
        public int QueueID { get; set; }
        public DateTime DueDateTime { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public int Source { get; set; }
        public int IssueType { get; set; }
        public int SubIssueType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }


        [JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? AssignedResourceID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? AssignedResourceRoleID { get; set; }


        //Serialize the DTO
        public static string Serialize(
            long companyId,
            int queueID,
            DateTime dueDateTime,
            int priority,
            int status,
            int source,
            int issueType,
            int subIssueType,
            string title,
            string description,
            int? assignedResourceID = null,
            int? assignedResourceRoleID = null)
        {

            var payload = new TicketRequestPayload
            {
                CompanyID = companyId.ToString(),
                QueueID = queueID,
                DueDateTime = dueDateTime,
                Priority = priority,
                Status = status,
                Source = source,
                IssueType = issueType,
                SubIssueType = subIssueType,
                Title = title,
                Description = description,
                AssignedResourceID = assignedResourceID,
                AssignedResourceRoleID = assignedResourceRoleID
            };

            // Serialize Duedate using ISO 8601 format for dates.
            var jsonSettings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd'T'HH:mm:ss"
            };

            return JsonConvert.SerializeObject(payload, jsonSettings);
        }
    }
}
