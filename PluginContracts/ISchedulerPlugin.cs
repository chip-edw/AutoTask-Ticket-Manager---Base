namespace PluginContracts
{
    public interface ISchedulerPlugin : IPlugin
    {
        /// <summary>
        /// Starts the scheduler loop or timer.
        /// </summary>
        void Start();

        /// <summary>
        /// Executes scheduled jobs manually (e.g. for testing or boot-time kickstart).
        /// </summary>
        Task RunScheduledJobsAsync(CancellationToken token);

        /// <summary>
        /// Provides the scheduler plugin with its assigned jobs.
        /// </summary>
        void SetJobs(IEnumerable<SchedulerJobConfig> jobs);

        /// <summary>
        /// Sets the result reporter used to return job results.
        /// </summary>
        void SetResultReporter(ISchedulerResultReporter reporter);
    }

}
