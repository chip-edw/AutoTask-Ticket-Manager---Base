namespace AutoTaskTicketManager_Base.Dtos.Tickets
{
    public class TicketMetadataDto
    {
        public List<PicklistOptionDto> Statuses { get; set; } = new();
        public List<PicklistOptionDto> Queues { get; set; } = new();
        public List<PicklistOptionDto> Priorities { get; set; } = new();
        public List<PicklistOptionDto> Resources { get; set; } = new();
    }

    public class PicklistOptionDto
    {
        public string Value { get; set; } = "";
        public string Label { get; set; } = "";
    }
}

