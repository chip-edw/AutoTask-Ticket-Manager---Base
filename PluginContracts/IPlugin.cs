namespace PluginContracts
{
    public interface IPlugin
    {
        string Name { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
