namespace PluginContracts
{
    public class SchedulerJobExecutionResult
    {
        public int TaskID { get; set; }
        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }
        public string Status { get; set; } = "Unknown";
    }
}

