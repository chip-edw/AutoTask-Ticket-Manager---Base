using AutoTaskTicketManager_Base.Models;
using Microsoft.EntityFrameworkCore;
using PluginContracts;

namespace AutoTaskTicketManager_Base.Scheduler
{
    public class SchedulerResultReporter : ISchedulerResultReporter
    {
        private readonly ApplicationDbContext _dbContext;

        public SchedulerResultReporter(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ReportJobResultAsync(SchedulerJobExecutionResult result, CancellationToken cancellationToken = default)
        {
            var job = await _dbContext.Schedulers
                .FirstOrDefaultAsync(s => s.TaskID == result.TaskID, cancellationToken);

            if (job != null)
            {
                job.LastRunTime = result.LastRunTime;
                job.NextRunTime = result.NextRunTime;

                // Optional: If you want to track last result/status in DB, add a column like `LastResultStatus`
                // job.LastResultStatus = result.Status;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
