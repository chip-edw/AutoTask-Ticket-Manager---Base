using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.AutoTaskAPI.Utilities;
using AutoTaskTicketManager_Base.Common.Secrets;
using AutoTaskTicketManager_Base.Models;
using AutoTaskTicketManager_Base.MSGraphAPI;
using AutoTaskTicketManager_Base.Scheduler;
using AutoTaskTicketManager_Base.Services;
using AutoTaskTicketManager_Base.Services.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PluginContracts;


namespace AutoTaskTicketManager_Base.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            #region Core Configuration
            services.AddSingleton<ISecretsProvider, LocalSecretsProvider>();
            #endregion

            #region MS Graph Integration
            services.AddSingleton<SecureEmailApiHelper>();
            services.AddSingleton<ConfidentialClientApp>();
            services.AddSingleton<IMsalHttpClientFactory, MsalHttpClientFactory>();
            #endregion

            #region HTTP Clients
            services.AddHttpClient<SecureEmailApiHelper>();
            #endregion

            #region AutoTask and Plugin API
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<AutotaskAPIGet>();
            services.AddSingleton<AutoTaskResources>();
            services.AddSingleton<TicketHandler>();
            services.AddSingleton<IPicklistService, PicklistService>();
            services.AddSingleton<PluginManager>();
            #endregion

            #region Business Services and Internal API
            services.AddScoped<IManagementService, ManagementService>();
            services.AddScoped<ICompanySettingsService, CompanySettingsService>();
            services.AddSingleton<StartupLoaderService>();
            services.AddScoped<EmailManager>();
            services.AddScoped<ISenderExclusionService, SenderExclusionService>();
            services.AddScoped<ISubjectExclusionService, SubjectExclusionService>();
            services.AddScoped<ITicketUIService, TicketUIService>();


            #endregion

            #region Scheduler
            services.AddScoped<ISchedulerJobLoader, SchedulerJobLoader>();
            services.AddSingleton<IOpenTicketService, OpenTicketService>();
            services.AddScoped<ISchedulerResultReporter>(provider =>
            {
                var dbContext = provider.GetRequiredService<ApplicationDbContext>();
                return new SchedulerResultReporter(dbContext);
            });
            #endregion

            #region Entity Framework / DB
            //Register ApplicationDbContext needed so we can create new DbContext instances to use across threads
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=ATTMS.db"));
            #endregion

            #region Worker and Hosted Services
            services.AddScoped<IWorkerService, Worker>();
            services.AddHostedService<ScopedWorkerHostedService>();
            #endregion

            return services;
        }
    }
}
