namespace AutoTaskTicketManager_Base.Dtos.Tickets
{
    public class TicketUpdateDto
    {
        /// <summary>
        /// The AutoTask ticket ID. Required when updating a ticket.
        /// This value is typically passed via the route and echoed here for API forwarding.
        /// </summary>
        public long? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? StatusId { get; set; }
        public int? QueueId { get; set; }
        public int? PriorityId { get; set; }
        public long? AssignedResourceId { get; set; }  // Make sure this is long?, not int?
        public long? CompanyId { get; set; }
    }
}

