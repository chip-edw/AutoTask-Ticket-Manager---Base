namespace PluginContracts
{
    public interface ISchedulerJobLoader
    {
        Task<IEnumerable<SchedulerJobConfig>> LoadJobsAsync(CancellationToken cancellationToken = default);
    }
}

