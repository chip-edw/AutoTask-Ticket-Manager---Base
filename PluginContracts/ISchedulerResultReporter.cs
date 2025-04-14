namespace PluginContracts
{
    public interface ISchedulerResultReporter
    {
        Task ReportJobResultAsync(SchedulerJobExecutionResult result, CancellationToken cancellationToken = default);
    }

}

