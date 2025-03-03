using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.AutoTaskAPI.Utilities;
using AutoTaskTicketManager_Base.MSGraphAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Identity.Client;
using Serilog;

namespace AutoTaskTicketManager_Base
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var httpClientFactory = new MsalHttpClientFactory(configuration);



            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Attach global handlers for error handling
            AttachGlobalHandlers();

            try
            {
                Log.ForContext("SourceContext", "Microsoft.Hosting.Lifetime")
                    .Information("Testing Microsoft.Hosting.Lifetime log capture.");

                Log.Information($"Begining {nameof(StartupConfiguration)}\n");


                //Loads all the Protected Application Settings from the SQLite Db into the Dictionary StartupConfiguration.AppSettings
                //Must be loaded before the LoadMsGraphSettings as the MsGraph Settings are hear to start with and are just broken out for simplicity
                StartupConfiguration.LoadProtectedSettings();
                Log.Information($"{nameof(StartupConfiguration.LoadProtectedSettings)}");


                //Loads the necessary values for the MS Graph API. Includes values nessary to retrieve the Bearer Access Token
                //from the Azure Authentication Service
                //Must be loaded before initializing the EmailManager as these settings are involved in the MSGraph authentication
                StartupConfiguration.LoadMsGraphConfig();
                Log.Information($"{nameof(StartupConfiguration.LoadMsGraphConfig)}");

                EmailManager.Initialize(configuration, httpClientFactory);
                Log.Information($"Email Manager {nameof(EmailManager.Initialize)}");

                // Create the WebApplication builder
                var builder = WebApplication.CreateBuilder(args);

                // Explicitly configure Serilog for the WebApplication builder
                builder.Logging.ClearProviders(); // Clear default logging providers
                builder.Logging.AddConsole();    // Add console logging back
                builder.Host.UseSerilog();       // Use Serilog as the primary logging provider


                // Add the configuration to the builder
                builder.Configuration.AddConfiguration(configuration);

                // Register Services
                ConfigureServices(builder.Services, builder.Configuration);

                //Register Singletons
                builder.Services.AddSingleton<SecureEmailApiHelper>();
                builder.Services.AddSingleton<ConfidentialClientApp>();
                builder.Services.AddSingleton<IMsalHttpClientFactory, MsalHttpClientFactory>();
                builder.Services.AddTransient<EmailManager>();
                builder.Services.AddSingleton<IApiClient, ApiClient>();
                builder.Services.AddSingleton<IPicklistService, PicklistService>();
                builder.Services.AddSingleton<AutotaskAPIGet>();
                builder.Services.AddSingleton<AutoTaskResources>();
                builder.Services.AddSingleton<TicketHandler>();


                //Register Worker
                builder.Services.AddHttpClient<SecureEmailApiHelper>();
                builder.Services.AddSingleton<IWorkerService, Worker>();
                builder.Services.AddHostedService<Worker>();


                // Configure Kestrel for the internal maintenance API
                int managementApiPort = configuration.GetValue<int>("ManagementApiPort");
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(managementApiPort); // Bind to the specified port
                });

                // Build and run the application
                var app = builder.Build();

                // Map the maintenance API endpoints
                ConfigureEndpoints(app, managementApiPort);

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start.");
            }
            finally
            {
                Log.Information("Shutting down...");
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register the worker service
            services.AddHostedService<Worker>();

            // Register IConfiguration as a singleton for use throughout the application
            services.AddSingleton(configuration);

            // Additional services can be registered here
            services.AddScoped<IWorkerService, Worker>();
        }

        private static void ConfigureEndpoints(WebApplication app, int managementApiPort)
        {
            // Expose a default endpoint for testing the API
            app.MapGet("/", (IServiceProvider services) =>
            {
                var server = services.GetRequiredService<IServer>();
                var serverAddresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
                var listeningUrl = serverAddresses?.FirstOrDefault() ?? $"http://localhost:{managementApiPort}";
                Log.Information("Management API is UP and listening at: {listeningUrl}", listeningUrl);

                return $"Management API is UP and listening on: {listeningUrl}";
            });

            // Map additional endpoints from the ManagementAPI class
            ManagementAPI.ManagementAPI.Map(app);
        }

        private static void AttachGlobalHandlers()
        {
            // Handle Unhandled Exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                Log.Fatal($"Unhandled exception: {ex?.Message}", ex);
            };

            // Handle Task Unobserved Exceptions
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Fatal($"Unobserved task exception: {args.Exception?.Message}", args.Exception);
                args.SetObserved(); // Prevents the process from being terminated
            };

            // Handle Process Exit
            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                Log.Information("Process is exiting. Performing cleanup...");
            };
        }
    }
}
