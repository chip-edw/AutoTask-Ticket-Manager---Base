using AutoTaskTicketManager_Base.Models;
using Microsoft.EntityFrameworkCore;
using PluginContracts;
using Serilog;

namespace AutoTaskTicketManager_Base.Scheduler
{
    public class SchedulerJobLoader : ISchedulerJobLoader
    {
        private readonly ApplicationDbContext _dbContext;

        public SchedulerJobLoader(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

        }

        public async Task<IEnumerable<SchedulerJobConfig>> LoadJobsAsync(CancellationToken cancellationToken = default)
        {
            Log.Information("Loading active scheduler jobs from the database...");

            return await _dbContext.Schedulers
                .Where(s => s.TaskActive)
                .OrderBy(s => s.NextRunTime)
                .Select(s => new SchedulerJobConfig
                {
                    TaskID = s.TaskID,
                    TaskName = s.TaskName,
                    FrequencyMinutes = s.FrequencyMinutes,
                    TaskActive = s.TaskActive,
                    NextRunTime = s.NextRunTime
                })
                .ToListAsync(cancellationToken);
        }
    }
}
