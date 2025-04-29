using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.Models;
using Serilog;

namespace AutoTaskTicketManager_Base.Services
{
    public class StartupLoaderService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AutotaskAPIGet _autotaskAPIGet;
        private readonly IPicklistService _picklistService;

        public StartupLoaderService(IServiceScopeFactory scopeFactory, AutotaskAPIGet autotaskAPIGet, IPicklistService picklistService)
        {
            _scopeFactory = scopeFactory;
            _picklistService = picklistService ?? throw new ArgumentNullException(nameof(picklistService));
            _autotaskAPIGet = autotaskAPIGet ?? throw new ArgumentNullException(nameof(autotaskAPIGet));
        }

        public async Task LoadAllStartupDataAsync()
        {
            Log.Information("Starting initial load of all startup configuration data...");

            //Loads the AutoTask Ticket Field List so we can be dynamic with Picklists / Drop down menus changes
            await _autotaskAPIGet.PicklistInformation(_picklistService);

            ////Loads all active Autotask Companies from the AutoTask API into Companies.companies Dictionary
            AutotaskAPIGet.GetAutoTaskCompanies();

            //Load Open tickets into Dictionary
            AutotaskAPIGet.GetNotCompletedTickets();

            //Load Active AutoTask Resources into Dictionary
            AutotaskAPIGet.GetAutoTaskActiveResources();

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Critical! Configurations
                StartupConfiguration.LoadSupportDistros(dbContext);
                StartupConfiguration.LoadAutoAssignCompanies(dbContext);
                StartupConfiguration.LoadAutoAssignSenders(dbContext);

                // Important! Exclusions not allowed to create or update a ticket
                StartupConfiguration.LoadSenderExclusionListFromSQL(dbContext);
                StartupConfiguration.LoadSubjectExclusionKeyWordsFromSQL(dbContext);

                // Important! Compares the SQL DB with companies loaded into memory and if any are missing in SQL they get added
                StartupConfiguration.UpdateDataBaseWithMissingCompanies(dbContext);
            }

            Log.Information("Startup data loaded successfully.");

        }
    }
}
