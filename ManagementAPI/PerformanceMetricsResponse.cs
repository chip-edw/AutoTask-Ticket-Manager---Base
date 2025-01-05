namespace ATTMWS_L.ManagementAPI
{
    public class PerformanceMetricsResponse
    {
        public double TotalCpuUsage { get; set; }
        public double ProcessCpuUsage { get; set; }
        public double TotalRamUsageMB { get; set; }
        public double ProcessRamUsageMB { get; set; }
    }
}