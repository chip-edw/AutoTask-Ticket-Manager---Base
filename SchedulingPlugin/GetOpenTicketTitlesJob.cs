using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginContracts;

namespace SchedulingPlugin.Jobs
{
    public class GetOpenTicketTitlesJob : ISchedulerJob
    {
        public string JobName => "GetOpenTicketTitles";

        public async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<GetOpenTicketTitlesJob>>();
            var openTicketService = serviceProvider.GetRequiredService<IOpenTicketService>();

            try
            {
                logger.LogInformation("Executing job: {JobName}", JobName);
                openTicketService.LoadOpenTickets();
                logger.LogInformation("Job {JobName} completed successfully", JobName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job {JobName} failed.", JobName);
                throw;
            }

            await Task.CompletedTask;
        }
    }
}
