using Microsoft.Extensions.DependencyInjection;
using PluginContracts;




namespace SchedulingPlugin
{
    public class SchedulingPlugin : ISchedulerPlugin
    {
        private IEnumerable<SchedulerJobConfig> _jobs = new List<SchedulerJobConfig>();
        private ISchedulerResultReporter? _reporter;
        private Thread? _schedulerThread;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly string _name = "SchedulingPlugin";

        public string Name => _name;

        public void SetJobs(IEnumerable<SchedulerJobConfig> jobs)
        {
            _jobs = jobs;
        }

        public void SetResultReporter(ISchedulerResultReporter reporter)
        {
            _reporter = reporter;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _schedulerThread = new Thread(() => RunSchedulerLoop(_cancellationTokenSource.Token))
            {
                IsBackground = true,
                Name = "SchedulingPluginThread"
            };
            _schedulerThread.Start();
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void RunSchedulerLoop(CancellationToken token)
        {
            var pollInterval = TimeSpan.FromSeconds(30); // Can be config-driven

            while (!token.IsCancellationRequested)
            {
                try
                {
                    foreach (var jobConfig in _jobs)
                    {
                        if (!jobConfig.TaskActive || jobConfig.NextRunTime > DateTime.Now)
                        {
                            Console.WriteLine($"[SchedulerPlugin] Job '{jobConfig.TaskName}' not due yet or inactive.");
                            continue;
                        }

                        Console.WriteLine($"[SchedulerPlugin] Creating DI scope for job: {jobConfig.TaskName}");

                        using var scope = PluginContracts.ServiceActivator.ServiceProvider?.CreateScope();
                        var serviceProvider = scope.ServiceProvider;

                        var jobType = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .FirstOrDefault(t =>
                                typeof(ISchedulerJob).IsAssignableFrom(t) &&
                                !t.IsAbstract &&
                                Activator.CreateInstance(t) is ISchedulerJob instance &&
                                instance.JobName.Equals(jobConfig.TaskName, StringComparison.OrdinalIgnoreCase));

                        if (jobType == null)
                        {
                            Console.WriteLine($"[SchedulerPlugin] No job class found matching: {jobConfig.TaskName}");
                            continue;
                        }

                        if (Activator.CreateInstance(jobType) is ISchedulerJob jobInstance)
                        {
                            Console.WriteLine($"[SchedulerPlugin] Executing job: {jobInstance.JobName}");

                            var result = new SchedulerJobExecutionResult
                            {
                                TaskID = jobConfig.TaskID,
                                LastRunTime = DateTime.Now,
                                NextRunTime = DateTime.Now.AddMinutes(jobConfig.FrequencyMinutes),
                                Status = "Success"
                            };

                            try
                            {
                                jobInstance.ExecuteAsync(scope.ServiceProvider, token).GetAwaiter().GetResult();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[SchedulerPlugin] Job {jobInstance.JobName} failed: {ex.Message}");
                                result.Status = "Failed";
                            }

                            try
                            {
                                var reporter = serviceProvider.GetRequiredService<ISchedulerResultReporter>();
                                reporter.ReportJobResultAsync(result, token).GetAwaiter().GetResult();
                                jobConfig.NextRunTime = result.NextRunTime; // update in memory

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[SchedulerPlugin] Failed to report result for job {jobInstance.JobName}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SchedulerPlugin] Error in scheduler loop: {ex.Message}");
                }

                Thread.Sleep(pollInterval);
            }

            Console.WriteLine("[SchedulerPlugin] Scheduler loop exited.");
        }





        public Task ExecuteAsync(CancellationToken token)
        {
            // This scheduler runs on its own thread, so this method is intentionally a no-op.
            return Task.CompletedTask;
        }


    }
}
