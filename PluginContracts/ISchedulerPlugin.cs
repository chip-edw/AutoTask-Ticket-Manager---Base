namespace PluginContracts
{
    /// <summary>
    /// Interface for scheduler plugins that manage and execute scheduled jobs on their own internal loop.
    /// </summary>
    public interface ISchedulerPlugin : IPlugin
    {
        /// <summary>
        /// Starts the scheduler plugin. Typically creates an internal thread or loop to monitor and run jobs.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the scheduler plugin loop gracefully.
        /// </summary>
        void Stop();

        /// <summary>
        /// Provides the scheduler plugin with a list of configured jobs from the database.
        /// </summary>
        /// <param name="jobs">A collection of scheduler job configuration entries.</param>
        void SetJobs(IEnumerable<SchedulerJobConfig> jobs);

        /// <summary>
        /// Provides the result reporter implementation used for persisting job execution results.
        /// </summary>
        /// <param name="reporter">An implementation of ISchedulerResultReporter.</param>
        void SetResultReporter(ISchedulerResultReporter reporter);
    }
}
