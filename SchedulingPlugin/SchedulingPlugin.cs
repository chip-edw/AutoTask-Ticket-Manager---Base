using PluginContracts;

namespace SchedulingPlugin
{
    public class SchedulingPlugin : IPlugin
    {
        public string Name => "SchedulingPlugin";

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"[{Name}] Executed at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
