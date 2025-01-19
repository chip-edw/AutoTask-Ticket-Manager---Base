using AutoTaskTicketManager_Base.MSGraphAPI;
using Serilog;

namespace AutoTaskTicketManager_Base
{
    //Used for executing methods within the worker class from the Management API
    public interface IWorkerService
    {
        Task DoWorkAsync(CancellationToken cancellationToken);
        void StopService();
    }

    public class Worker : BackgroundService, IWorkerService
    {
        private volatile bool cancelTokenIssued = false;

        //private CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly ConfidentialClientApp _confidentialClientApp;

        public Worker(ConfidentialClientApp confidentialClientApp, ILogger<Worker> logger)
        {
            _confidentialClientApp = confidentialClientApp;
            _logger = logger;
        }


        private readonly ILogger<Worker> _logger;


        public async void StopService()
        {
            Log.Information("Internal Stop service Cancellation Token Received ...");
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Cancel();
            cancelTokenIssued = true;
        }


        public override async Task StartAsync(CancellationToken cancellationToken)
        {

            //Get OS 
            var os = StartupConfiguration.DetermineOS();
            // ##########          Begin Application Startup and Prechecks          ##########

            Log.Information("Preparing to start AutoTaskTicketManagement Service \n " +
                "Just need to wait 10 sec for all Network Dependancies to finish \n\n");
            Thread.Sleep(10 * 1000);

            //Loads the necessary values for the MS Graph API. Includes values nessary to retrieve the Bearer Access Token
            //from the Azure Authentication Service
            StartupConfiguration.LoadMsGraphConfig();

            //Loads the supportDistros Dictionary       
            StartupConfiguration.LoadSupportDistros();

            ////Loads the AutoTask Ticket Field List so we can be dynamic with Picklists / Drop down menus changes
            //AutotaskAPIGet.PicklistInformation();

            ////Loads all active Autotask Companies from the AutoTask API into Companies.companies Dictionary
            //AutotaskAPIGet.GetAutoTaskCompanies();

            ////Loads all the SubjectExclusionKeyWords from the Database
            //StartupConfiguration.LoadSubjectExclusionKeyWordsFromSQL();

            ////Loads all the SenderExclusions from the Database
            //StartupConfiguration.LoadSenderExclusionListFromSQL();

            ////Compares the SQL DB with what was loaded into memory and if anything is missing in SQL it gets added
            //StartupConfiguration.UpdateDataBaseWithMissingCompanies();

            //Loads all the AT Companies into a Dictionary that have the auto assign flag set
            StartupConfiguration.LoadAutoAssignCompanies();

            ////Dictionary that holds all the AutoAssign members for AT companies that have the AutoAssign flag set in the local database CustomerSettings table
            //StartupConfiguration.LoadAutoAssignMembers();

            ////Dictionary that holds the Flintfox senders who should be directly assigned to any ticket they raise.
            //StartupConfiguration.LoadAutoAssignSenders();

            ////Load open tickets into Dictionary
            //AutotaskAPIGet.GetNotCompletedTickets();

            ////Load Active AutoTask Resources into Dictionary
            //AutotaskAPIGet.GetAutoTaskActiveResources();



            //// Start Seperate Thread to run Scheduler Timer in the background
            //_schedulerTimer = new Timer(DoScheduler, null, schedulerRunDelay, Timeout.Infinite);

            //Log.Information("Scheduler Thread Started");


            //Get Assembly Version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;


            // ##########          Completes Application Startup and Prechecks          ##########


            Log.Information("");
            Log.Information(" _______________________________________________________________________");
            Log.Information("|                                                                       |");
            Log.Information("|              AutoTaskTicketManagement Service Started                 |");
            Log.Information("|   Protected App Settings Retrieved from SQL and loaded into Memory    |");
            Log.Information("|                                                                       |");
            Log.Information($"                   Application version: {assemblyVersion}");
            Log.Information("|_______________________________________________________________________|");
            Log.Information("\r\n\r\n");



            // When done testing put return back in front of base
            //return base.StartAsync(cancellationToken);
            await base.StartAsync(cancellationToken);

        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("\n\n");
            Log.Information("Entering Main Worker Service ExecuteAsync Method");
            Log.Information("\n\n");

            //Get the wait period at the bottom of each loop
            var tD = StartupConfiguration.GetProtectedSetting("TimeDelay");
            int timeDelay = Int32.Parse(tD) * 1000;

            var accessToken = await _confidentialClientApp.GetAccessToken();
            Log.Debug("Acquired Access Token for Startup");

            while (!stoppingToken.IsCancellationRequested || !cancelTokenIssued == true ||
                           !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {

                    if (Authenticate.GetExpiresOn() > DateTime.UtcNow)
                    {
                        // Check for cancelation
                        if (stoppingToken.IsCancellationRequested || _cancellationTokenSource.IsCancellationRequested ||
                            cancelTokenIssued == true || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            Log.Information("Cancellation requested. Exiting worker service loop. \n");
                            break;
                        }

                        Console.WriteLine("");

                        ////#####################################################
                        ////Check if any new e-mail has arrived in Inbox.
                        //await Task.Run(EmailHelper.CheckEmail, stoppingToken);
                        ////#####################################################


                        // Check for cancelation
                        if (stoppingToken.IsCancellationRequested || _cancellationTokenSource.IsCancellationRequested ||
                            cancelTokenIssued == true)
                        {
                            Log.Information("Cancellation requested. Exiting worker service loop. \n");
                            break;
                        }

                        //int unreadCount = EmailHelper.GetUnreadCount();



                        // Check for cancelation
                        if (stoppingToken.IsCancellationRequested || _cancellationTokenSource.IsCancellationRequested ||
                            cancelTokenIssued == true)
                        {
                            Log.Information("Cancellation requested. Exiting worker service loop. \n");
                            break;
                        }


                        //#################################################################################
                        // Logic for managing Unread e-mails
                        // Remove the following resetting of the var unreadCount as functionality is added
                        int unreadCount = 0;
                        //#################################################################################


                        if (unreadCount > 0)
                        {
                            Log.Debug($"Messages to process: {unreadCount}.  0 count of messages are only recorded in verbose logging mode\n");

                            await Task.Delay(1000, stoppingToken);

                            // Check for cancelation
                            if (stoppingToken.IsCancellationRequested || _cancellationTokenSource.IsCancellationRequested ||
                                cancelTokenIssued == true)
                            {
                                Log.Information("Cancellation requested. Exiting worker service loop. \n");
                                break;
                            }
                        }
                        else if (unreadCount == 0)
                        {
                            Log.Verbose("...");
                            Log.Verbose($"No Messages to Process pausing for {timeDelay} seconds. \nSet in SQL DB AppConfig Table under TimeDelay");
                            Log.Verbose("Worker Loop - Pre Schedule");
                            await Task.Delay(timeDelay, stoppingToken); //Pausing for seconds set in SQL DB AppConfig Table under TimeDelay

                        }

                    }
                    else
                    {
                        if (stoppingToken.IsCancellationRequested || _cancellationTokenSource.IsCancellationRequested ||
                            cancelTokenIssued == true)
                        {
                            Log.Information("\n Cancellation requested. Exiting worker service loop. \n");
                            break;
                        }
                        await Task.Delay(500, stoppingToken);
                        accessToken = await _confidentialClientApp.GetAccessToken();
                        Log.Debug("Aquired Bearer Token");
                    }

                }
                catch (Exception ex)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        Log.Error($"Worker Service recieved stoppingToken Cancellation request. Stopping ATTMWS");
                        break;
                    }
                    else
                    {
                        Log.Error($"Worker.cs Failed to acquire an Access Bearer Token. Check stuff... {ex}");
                    }

                    return;

                }

            }
        }


        /// <summary>
        /// Holds the Logic for calling methods or performing background work. Currently just being used to helo manage the cancellatioin Token.
        /// It will look ecery 1 second for a cancellationToken to be issued.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            // Implement your logic for DoWorkAsync here
            // For example, you might want to call methods or perform some background work

            // Your scheduled task code here
            Console.WriteLine($"Task executed at: {DateTime.Now}");

            await StopAsync(cancellationToken).ConfigureAwait(false);
            await Task.Delay(1000, cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Worker stopping at:::::: {time}", DateTimeOffset.Now);

            //// Perform cleanup or additional tasks before stopping the worker.
            //_schedulerTimer?.Dispose();

            await base.StopAsync(cancellationToken);
        }
    }
}
