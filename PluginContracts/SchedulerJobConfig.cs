namespace PluginContracts
{
    public class SchedulerJobConfig
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public int FrequencyMinutes { get; set; }
        public bool TaskActive { get; set; }
        public DateTime NextRunTime { get; set; }
    }
}

