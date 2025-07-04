﻿using PluginContracts;
using Serilog;
using System.Runtime.Loader;

namespace AutoTaskTicketManager_Base.Services
{
    public class PluginManager
    {
        // Plugin folder path
        private readonly string _pluginPath = Path.Combine(AppContext.BaseDirectory, "Plugins");

        // This will hold all ISchedulerJob implementations discovered in plugin DLLs.
        public List<ISchedulerJob> Jobs { get; } = new List<ISchedulerJob>();

        //private readonly ILogger<PluginManager> _logger;
        private readonly List<IPlugin> _plugins = new();

        // Legacy plugin list exposed as read-only
        public IReadOnlyList<IPlugin> Plugins => _plugins.AsReadOnly();

        //public PluginManager(ILogger<PluginManager> logger)
        //{
        //    _logger = logger;
        //}

        public void LoadPlugins()
        {
            // Clear legacy plugins list first.
            _plugins.Clear();
            // Also clear scheduler jobs list to ensure a fresh load.
            Jobs.Clear();

            if (!Directory.Exists(_pluginPath))
            {
                Directory.CreateDirectory(_pluginPath);
                Log.Warning("Plugin directory did not exist, so it was created: {PluginPath}", _pluginPath);
            }

            var dllFiles = Directory.GetFiles(_pluginPath, "*.dll");

            foreach (var dll in dllFiles)
            {
                try
                {
                    Log.Information("Attempting to load plugin from {DllPath}", dll);

                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                    // Enumerate all types for debugging purposes:
                    var allTypes = assembly.GetTypes();
                    Log.Information("Enumerating {Count} types in assembly: {DllPath}", allTypes.Length, dll);
                    foreach (var type in allTypes)
                    {
                        Log.Information(" - Found type: {FullName}", type.FullName);
                    }

                    // Legacy: Load plugins that implement IPlugin
                    var pluginTypes = allTypes.Where(t =>
                        typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var type in pluginTypes)
                    {
                        if (Activator.CreateInstance(type) is IPlugin plugin)
                        {
                            _plugins.Add(plugin);
                            Log.Information("Successfully loaded plugin: {PluginName}", plugin.Name);
                        }
                    }
                    if (!pluginTypes.Any())
                    {
                        Log.Warning("No valid IPlugin types found in {DllPath}", dll);
                    }

                    // NEW: Discover ISchedulerJob implementations from the same assembly
                    try
                    {
                        var schedulerJobTypes = allTypes.Where(t =>
                            typeof(ISchedulerJob).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                            .ToList();

                        Log.Information("Found {Count} ISchedulerJob candidate(s) in {DllPath}", schedulerJobTypes.Count, dll);
                        foreach (var jobType in schedulerJobTypes)
                        {
                            try
                            {
                                Log.Information("Attempting to create instance of: {Type}", jobType.FullName);

                                if (Activator.CreateInstance(jobType) is ISchedulerJob job)
                                {
                                    Jobs.Add(job);
                                    Log.Information("OK Discovered scheduler job: {JobName}", job.JobName);
                                }
                                else
                                {
                                    Log.Warning("Failed Could not cast {Type} to ISchedulerJob", jobType.FullName);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Failed to instantiate ISchedulerJob from type: {Type}", jobType.FullName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to discover ISchedulerJob implementations in {DllPath}", dll);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load plugin from {DllPath}", dll);
                }
            }

            Log.Information($"Plugin load complete. Total plugins loaded: {_plugins.Count}, Total scheduler jobs loaded: {Jobs.Count}");
        }

        // Legacy method for assigning scheduler jobs (if any plugins implement ISchedulerPlugin).
        // This method is maintained for backward compatibility with parts of the startup that may use it.
        public void AssignSchedulerJobs(IEnumerable<SchedulerJobConfig> jobs, ISchedulerResultReporter reporter)
        {
            foreach (var plugin in _plugins.OfType<ISchedulerPlugin>())
            {
                var pluginJobs = jobs
                    .Where(j => j.TaskName.Length > 1) // Future filtering by plugin name could be applied here.
                    .ToList();

                plugin.SetJobs(pluginJobs);
                plugin.SetResultReporter(reporter);

                Log.Information("Assigned {Count} jobs to plugin {PluginName}", pluginJobs.Count, plugin.Name);
            }
        }
    }
}
