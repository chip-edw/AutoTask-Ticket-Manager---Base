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

            // Slight delay to bulletproof the first-write timing and smooth out any SQLite weirdness
            Task.Delay(50);


            if (job != null)
            {
                job.LastRunTime = result.LastRunTime;
                job.NextRunTime = result.NextRunTime;
                job.Status = result.Status;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
