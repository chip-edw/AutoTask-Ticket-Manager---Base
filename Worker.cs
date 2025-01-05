using Serilog;

namespace AutoTaskTicketManager_Base
{
    //Used for executing methods within the worker class from the Management API
    public interface IWorkerService
    {
        Task DoWorkAsync(CancellationToken cancellationToken);
        void StopService();
    }


    public class Worker : BackgroundService, IWorkerService
    {
        private volatile bool cancelTokenIssued = false;

        //private CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationTokenSource _cancellationTokenSource;


        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }


        public async void StopService()
        {
            Log.Information("Internal Stop service Cancellation Token Received ...");
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Cancel();
            cancelTokenIssued = true;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(5000, stoppingToken);
            }
        }


        /// <summary>
        /// Holds the Logic for calling methods or performing background work. Currently just being used to helo manage the cancellatioin Token.
        /// It will look ecery 1 second for a cancellationToken to be issued.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            // Implement your logic for DoWorkAsync here
            // For example, you might want to call methods or perform some background work

            // Your scheduled task code here
            Console.WriteLine($"Task executed at: {DateTime.Now}");

            await StopAsync(cancellationToken).ConfigureAwait(false);
            await Task.Delay(1000, cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Worker stopping at:::::: {time}", DateTimeOffset.Now);

            //// Perform cleanup or additional tasks before stopping the worker.
            //_schedulerTimer?.Dispose();

            await base.StopAsync(cancellationToken);
        }
    }
}
