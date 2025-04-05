namespace PluginContracts
{
    public interface ISchedulerPlugin : IPlugin
    {
        void SetJobs(IEnumerable<SchedulerJobConfig> jobs);
        void SetResultReporter(ISchedulerResultReporter reporter);
        Task RunScheduledJobsAsync(CancellationToken cancellationToken);
    }
}
