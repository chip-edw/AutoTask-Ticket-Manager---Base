namespace PluginContracts
{
    /// <summary>
    /// This interface defines the contract for scheduler jobs. 
    /// Each job should expose a unique JobName and the execution logic in ExecuteAsync.
    /// </summary>
    public interface ISchedulerJob
    {
        /// <summary>
        /// Gets the job name. This should match the TaskName stored in the Scheduler table.
        /// </summary>
        string JobName { get; }

        /// <summary>
        /// Executes the job logic. The serviceProvider provides access to any registered services.
        /// </summary>
        /// <param name="serviceProvider">The dependency injection container.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
