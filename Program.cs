using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;

namespace AutoTaskTicketManager_Base
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog for logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting the application...");

                // Create the WebApplication builder
                var builder = WebApplication.CreateBuilder(args);

                // Load configuration from appsettings.json
                builder.Configuration
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var configuration = builder.Configuration;

                // Get the port for the management API from appsettings.json
                int managementApiPort = configuration.GetValue<int>("ManagementApiPort");

                // Configure Kestrel to listen on the specified port
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(managementApiPort); // Bind to specified port
                });

                // Add services for the worker service
                builder.Services.AddHostedService<Worker>(); // Background worker
                builder.Services.AddScoped<IWorkerService, Worker>();

                var app = builder.Build();

                // Map management API root endpoint
                //app.MapGet("/", async context =>
                //{
                //    var serverAddresses = app.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
                //    var listeningUrl = serverAddresses?.FirstOrDefault();
                //    await context.Response.WriteAsync($"Management API is running and listening on: {listeningUrl}");
                //    Log.Information("Management API is UP and listening on port {managementApiPort}", managementApiPort);
                //});

                // Access ServerFeatures via app.Services
                app.MapGet("/", (IServiceProvider services) =>
                {
                    var server = services.GetRequiredService<IServer>();
                    var serverAddresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
                    var listeningUrl = serverAddresses?.FirstOrDefault() ?? $"http://localhost:{managementApiPort}";

                    Log.Information("Management API is UP and listening at: {listeningUrl}", listeningUrl);
                    return $"Management API is UP and listening on: {listeningUrl}";
                });



                // Map additional endpoints in the ManagementAPI class
                ManagementAPI.ManagementAPI.Map(app);

                app.Run(); // Run the application
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
