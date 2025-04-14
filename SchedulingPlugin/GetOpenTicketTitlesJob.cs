using Microsoft.Extensions.DependencyInjection;
using PluginContracts;
using Serilog;

namespace SchedulingPlugin.Jobs
{
    public class GetOpenTicketTitlesJob : ISchedulerJob
    {
        public string JobName => "GetOpenTicketTitles";

        public async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            //var logger = serviceProvider.GetRequiredService<ILogger<GetOpenTicketTitlesJob>>();
            var openTicketService = serviceProvider.GetRequiredService<IOpenTicketService>();

            try
            {
                Log.Information("Executing job: {JobName}", JobName);
                openTicketService.LoadOpenTickets();
                Log.Information("Job {JobName} completed successfully", JobName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Job {JobName} failed.", JobName);
                throw;
            }

            await Task.CompletedTask;
        }
    }
}
