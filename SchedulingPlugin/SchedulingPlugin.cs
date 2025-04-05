using PluginContracts;

namespace SchedulingPlugin
{
    public class SchedulingPlugin : ISchedulerPlugin
    {
        //Set the timer and time span between polling of the Schedule DB
        private Timer? _schedulerTimer;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30); // adjust as needed

        private List<SchedulerJobConfig> _jobs = new();
        private ISchedulerResultReporter? _reporter;

        public string Name => "SchedulingPlugin";

        public void SetJobs(IEnumerable<SchedulerJobConfig> jobs)
        {
            _jobs = jobs.ToList();
        }

        public void SetResultReporter(ISchedulerResultReporter reporter)
        {
            _reporter = reporter;
        }

        public async Task RunScheduledJobsAsync(CancellationToken cancellationToken)
        {
            foreach (var job in _jobs)
            {
                if (!job.TaskActive || job.NextRunTime > DateTime.Now)
                    continue;

                Console.WriteLine($"[{Name}] Running job: {job.TaskName} at {DateTime.Now}");

                try
                {
                    // Example: you can switch on job.TaskName here if needed
                    await Task.Delay(500, cancellationToken); // Simulate job work

                    if (_reporter != null)
                    {
                        await _reporter.ReportJobResultAsync(new SchedulerJobExecutionResult
                        {
                            TaskID = job.TaskID,
                            LastRunTime = DateTime.Now,
                            NextRunTime = DateTime.Now.AddMinutes(job.FrequencyMinutes),
                            Status = "Success"
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{Name}] Failed to run job {job.TaskName}: {ex.Message}");

                    if (_reporter != null)
                    {
                        await _reporter.ReportJobResultAsync(new SchedulerJobExecutionResult
                        {
                            TaskID = job.TaskID,
                            LastRunTime = DateTime.Now,
                            NextRunTime = DateTime.Now.AddMinutes(job.FrequencyMinutes),
                            Status = "Failed"
                        });
                    }
                }
            }
        }

        // Not used in scheduled mode, but required by IPlugin
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Start()
        {
            _schedulerTimer = new Timer(async _ =>
            {
                try
                {
                    await RunScheduledJobsAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{Name}] Scheduler timer error: {ex.Message}");
                }
            }, null, TimeSpan.Zero, _pollInterval);

            Console.WriteLine($"[{Name}] Scheduler background timer started (every {_pollInterval.TotalSeconds} sec)");
        }

        public void Stop()
        {
            _schedulerTimer?.Dispose();
            Console.WriteLine($"[{Name}] Scheduler stopped.");
        }



    }
}
