using Asp.Versioning;
using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.AutoTaskAPI.Utilities;
using AutoTaskTicketManager_Base.Common.Secrets;
using AutoTaskTicketManager_Base.ManagementAPI;
using AutoTaskTicketManager_Base.Models;
using AutoTaskTicketManager_Base.MSGraphAPI;
using AutoTaskTicketManager_Base.Scheduler;
using AutoTaskTicketManager_Base.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PluginContracts;
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

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Attach global handlers for error handling
            AttachGlobalHandlers();

            try
            {

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

                // Add Controllers (required for MapControllers to work)
                builder.Services.AddControllers();

                //Went for the more robust versioning for the maintenance API
                builder.Services.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = ApiVersionReader.Combine(
                        new UrlSegmentApiVersionReader(),
                        new HeaderApiVersionReader("X-Api-Version"),
                        new QueryStringApiVersionReader("api-version")
                    );
                });

                //Register Singletons
                builder.Services.AddSingleton<SecureEmailApiHelper>();
                builder.Services.AddSingleton<ConfidentialClientApp>();
                builder.Services.AddSingleton<IMsalHttpClientFactory, MsalHttpClientFactory>();
                builder.Services.AddScoped<EmailManager>();
                builder.Services.AddSingleton<IApiClient, ApiClient>();
                builder.Services.AddSingleton<IPicklistService, PicklistService>();
                builder.Services.AddSingleton<AutotaskAPIGet>();
                builder.Services.AddSingleton<AutoTaskResources>();
                builder.Services.AddSingleton<TicketHandler>();
                builder.Services.AddSingleton<PluginManager>();
                builder.Services.AddScoped<ISchedulerJobLoader, SchedulerJobLoader>();
                builder.Services.AddSingleton<IOpenTicketService, OpenTicketService>();
                builder.Services.AddSingleton<ISecretsProvider, LocalSecretsProvider>();
                builder.Services.AddScoped<IManagementService, ManagementService>();


                //Register Scoped services
                builder.Services.AddScoped<ISchedulerResultReporter>(provider =>
                {
                    var dbContext = provider.GetRequiredService<ApplicationDbContext>();
                    return new SchedulerResultReporter(dbContext);
                });


                //Register ApplicationDbContext needed so we can create new DbContext instances to use across threads
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=ATTMS.db"));


                //Register Worker
                builder.Services.AddHttpClient<SecureEmailApiHelper>();
                builder.Services.AddScoped<IWorkerService, Worker>();
                builder.Services.AddHostedService<ScopedWorkerHostedService>();


                // Configure Kestrel for the internal maintenance API
                int managementApiPort = configuration.GetValue<int>("ManagementApiPort");
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(managementApiPort); // Bind to the specified port
                });

                // Build and run the application
                var app = builder.Build();

                //Enable plugins to resolve scoped services
                PluginContracts.ServiceActivator.ServiceProvider = app.Services;


                // Create a scope to access scoped services
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    Log.Information($"Beginning {nameof(StartupConfiguration)}\n");

                    // Load settings using the scoped dbContext
                    StartupConfiguration.LoadProtectedSettings(dbContext);
                    Log.Information($"{nameof(StartupConfiguration.LoadProtectedSettings)}");


                    //Loads the necessary values for the MS Graph API. Includes values nessary to retrieve the Bearer Access Token
                    //from the Azure Authentication Service
                    //Must be loaded before initializing the EmailManager as these settings are involved in the MSGraph authentication
                    StartupConfiguration.LoadMsGraphConfig();
                    Log.Information($"{nameof(StartupConfiguration.LoadMsGraphConfig)}");

                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IMsalHttpClientFactory>();
                    EmailManager.Initialize(config, httpClientFactory);
                    Log.Information($"Email Manager {nameof(EmailManager.Initialize)}");
                }

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
            //Register IConfiguration as singleton
            services.AddSingleton(configuration);

            //Register Worker as a scoped dependency
            services.AddScoped<IWorkerService, Worker>();

            //Register the safe wrapper that uses a scoped Worker
            services.AddHostedService<ScopedWorkerHostedService>();
        }


        private static void ConfigureEndpoints(WebApplication app, int managementApiPort)
        {
            // GLOBAL ROUTING
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Protect only /api/setup with localhost-only middleware
            app.MapWhen(context => context.Request.Path.StartsWithSegments("/api/setup"), setupApp =>
            {
                setupApp.UseMiddleware<AutoTaskTicketManager_Base.Common.Middleware.LocalhostOnlyMiddleware>();
            });

            // default root endpoint
            app.MapGet("/", (IServiceProvider services) =>
            {
                var server = services.GetRequiredService<IServer>();
                var serverAddresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
                var listeningUrl = serverAddresses?.FirstOrDefault() ?? $"http://localhost:{managementApiPort}";
                Log.Information("Management API is UP and listening at: {listeningUrl}", listeningUrl);

                return $"Management API is UP and listening on: {listeningUrl}";
            });

            // Map legacy endpoints
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
