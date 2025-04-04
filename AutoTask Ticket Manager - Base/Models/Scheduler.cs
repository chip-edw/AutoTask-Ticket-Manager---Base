namespace AutoTaskTicketManager_Base.Models
{
    public class Scheduler
    {
        public int Id { get; set; }
        public int TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public int FrequencyMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }
        public bool TaskActive { get; set; }
    }
}
