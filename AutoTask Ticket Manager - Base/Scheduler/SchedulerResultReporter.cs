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

        public async Task ReportJobResultAsync(SchedulerJobExecutionResult result)
        {
            var job = await _dbContext.Schedulers.SingleOrDefaultAsync(j => j.TaskID == result.TaskID);
            if (job != null)
            {
                job.LastRunTime = result.LastRunTime;
                job.NextRunTime = result.NextRunTime;
                job.Status = result.Status;

                await _dbContext.SaveChangesAsync();
            }
        }
    }


}

