using PluginContracts;
using System.Runtime.Loader;

namespace AutoTaskTicketManager_Base.Services
{
    public class PluginManager
    {
        private readonly string _pluginPath = Path.Combine(AppContext.BaseDirectory, "Plugins");

        private readonly ILogger<PluginManager> _logger;
        private readonly List<IPlugin> _plugins = new();

        public IReadOnlyList<IPlugin> Plugins => _plugins.AsReadOnly();

        public PluginManager(ILogger<PluginManager> logger)
        {
            _logger = logger;
        }

        public void LoadPlugins()
        {
            _plugins.Clear();

            if (!Directory.Exists(_pluginPath))
            {
                Directory.CreateDirectory(_pluginPath);
                _logger.LogWarning("Plugin directory did not exist, so it was created: {PluginPath}", _pluginPath);
            }

            var dllFiles = Directory.GetFiles(_pluginPath, "*.dll");

            foreach (var dll in dllFiles)
            {
                try
                {
                    _logger.LogInformation("Attempting to load plugin from {DllPath}", dll);

                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                    var pluginTypes = assembly.GetTypes().Where(t =>
                        typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in pluginTypes)
                    {
                        if (Activator.CreateInstance(type) is IPlugin plugin)
                        {
                            _plugins.Add(plugin);
                            _logger.LogInformation("Successfully loaded plugin: {PluginName}", plugin.Name);
                        }

                    }

                    if (!pluginTypes.Any())
                    {
                        _logger.LogWarning("No valid plugin types found in {DllPath}", dll);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load plugin from {DllPath}", dll);
                }
            }

            _logger.LogInformation("Plugin load complete. Total loaded: {Count}", _plugins.Count);

        }
    }
}
