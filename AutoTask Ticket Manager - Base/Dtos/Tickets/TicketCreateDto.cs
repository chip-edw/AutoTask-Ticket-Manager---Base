namespace AutoTaskTicketManager_Base.Dtos.Tickets
{
    public class TicketCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public long? StatusId { get; set; }
        public long? QueueId { get; set; }
        public long? PriorityId { get; set; }
        public long? AssignedResourceId { get; set; }
    }

}
