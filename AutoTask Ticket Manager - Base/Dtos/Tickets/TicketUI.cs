namespace AutoTaskTicketManager_Base.Dtos.Tickets
{
    public class TicketUI
    {
        public long Id { get; set; }
        public string TicketNumber { get; set; } = "";
        public string Title { get; set; } = "";
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

