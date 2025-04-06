using PluginContracts;

namespace SchedulingPlugin
{
    public class SchedulingPlugin : ISchedulerPlugin
    {
        private CancellationTokenSource? _cts;
        private Task? _schedulerTask;
        private List<SchedulerJobConfig> _jobs = new();
        private ISchedulerResultReporter? _reporter;

        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);

        public string Name => "SchedulingPlugin";

        public void SetJobs(IEnumerable<SchedulerJobConfig> jobs)
        {
            _jobs = jobs.ToList();
        }

        public void SetResultReporter(ISchedulerResultReporter reporter)
        {
            _reporter = reporter;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _schedulerTask = Task.Run(() => RunSchedulerLoop(_cts.Token));
            Console.WriteLine($"[{Name}] Scheduler loop started.");
        }

        private async Task RunSchedulerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await RunScheduledJobsAsync(token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{Name}] Error in scheduler loop: {ex.Message}");
                }

                await Task.Delay(_pollInterval, token);
            }

            Console.WriteLine($"[{Name}] Scheduler loop exiting.");
        }

        public async Task RunScheduledJobsAsync(CancellationToken cancellationToken)
        {
            foreach (var job in _jobs)
            {
                if (!job.TaskActive || job.NextRunTime > DateTime.Now)
                    Console.WriteLine($"Dosen't seem any Jobs are being picked up... ");

                continue;

                Console.WriteLine($"[{Name}] Running job: {job.TaskName} at {DateTime.Now}");

                try
                {
                    await Task.Delay(500, cancellationToken); // Simulated work

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

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Not used for scheduled plugin execution
            return Task.CompletedTask;
        }

        public void Stop()
        {
            _cts?.Cancel();
            _schedulerTask?.Wait();
            Console.WriteLine($"[{Name}] Scheduler stopped.");
        }
    }

}
