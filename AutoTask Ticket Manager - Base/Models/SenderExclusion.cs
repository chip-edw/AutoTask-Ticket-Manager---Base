namespace AutoTaskTicketManager_Base.Models
{
    public class SenderExclusion
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        // Timestamp to track when a sender was blocked
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
