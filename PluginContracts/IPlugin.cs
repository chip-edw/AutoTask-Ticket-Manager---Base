namespace PluginContracts
{
    public interface IPlugin
    {
        string Name { get; }

        /// <summary>
        /// Main entry point for plugin logic.
        /// Some plugins may override this for one-time tasks.
        /// </summary>
        Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Called when the plugin is being stopped or disposed.
        /// </summary>
        void Stop();
    }

}
