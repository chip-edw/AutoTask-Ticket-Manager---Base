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
        private readonly Serilog.ILogger _logger;
        private volatile bool cancelTokenIssued = false;

        //private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


        private readonly ConfidentialClientApp _confidentialClientApp;
        private readonly EmailManager _emailManager;
        private readonly SecureEmailApiHelper _emailApiHelper;

        public Worker(ConfidentialClientApp confidentialClientApp, EmailManager emailManager, SecureEmailApiHelper emailApiHelper, ILogger<Worker> logger)
        {
            _confidentialClientApp = confidentialClientApp;
            _emailManager = emailManager;
            _logger = Log.ForContext<Worker>();
            _emailApiHelper = emailApiHelper ?? throw new ArgumentNullException(nameof(emailApiHelper));
        }


        public async void StopService()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _logger.Information("Internal Stop service Cancellation Token Received ...");
                try
                {
                    await _cancellationTokenSource.CancelAsync();
                }
                catch (OperationCanceledException)
                {
                    _logger.Warning("Operation was already canceled.");
                }
                finally
                {
                    _cancellationTokenSource.Cancel();
                }
            }
            cancelTokenIssued = true;
        }


        public override async Task StartAsync(CancellationToken cancellationToken)
        {

            //Get OS 
            var os = StartupConfiguration.DetermineOS();


            // ##########          Begin Application Startup and Prechecks          ##########

            _logger.Information("Preparing to start AutoTaskTicketManagement Service \n " +
                "Waiting 10 sec to ensure all Network Dependancies available. \n\n");
            Thread.Sleep(10 * 1000);

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

            _logger.Information(" ____________________________________________");
            _logger.Information("|                                            |");
            _logger.Information("|     AutoTask Ticket Management Service     |");
            _logger.Information("|                                            |");
            _logger.Information($"|        Application version: {assemblyVersion}        |");
            _logger.Information("|____________________________________________|");
            _logger.Information("\r\n\r\n");


            await base.StartAsync(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("Entering Main Worker Service ExecuteAsync Method\n");

            //Get the wait period at the bottom of each loop
            var tD = StartupConfiguration.GetProtectedSetting("TimeDelay");
            int timeDelay = Int32.Parse(tD) * 1000;



            try
            {
                string accessToken = await _confidentialClientApp.GetAccessToken();
                _logger.Debug("Acquired Access Token for Startup");

                //Send a test e-mail message using the refactored ProtectedApiCallHelper Class

                string subject = "ATTMS Worker Started Successfully";
                string body = $"The ATTMS Worker has started. at {System.DateTime.Now}";
                string adminEmail = "chip.edw@gmail.com";
                string url = "https://graph.microsoft.com/v1.0/users/attms@v7n2m.onmicrosoft.com/sendMail";

                //Comment or uncomment following line based on if you want an e-mail sent in startup
                //Later put config in Appsettings.json to control this

                //await _emailApiHelper.SendEmailAsync(url, accessToken, subject, body, adminEmail);

            }
            catch (Exception ex)
            {
                _logger.Error("Failed to retrieve the MSAL Bearer Token. Error: {ErrorMessage}", ex.Message);

                _logger.Error("Graph API call correlation ID: {CorrelationId}");
            }


            while (!stoppingToken.IsCancellationRequested && !cancelTokenIssued)
            {
                try
                {

                    if (Authenticate.GetExpiresOn() > DateTime.UtcNow)
                    {

                        Console.WriteLine("");

                        //#####################################################
                        //Check if any new e-mail has arrived in Inbox.
                        //await Task.Run(() => EmailManager.CheckEmail(_emailApiHelper), stoppingToken);
                        await EmailManager.CheckEmail(_emailApiHelper);
                        //#####################################################


                        int unreadCount = EmailManager.GetUnreadCount();



                        //#################################################################################
                        // Logic for managing Unread e-mails
                        // Remove the following resetting of the var unreadCount as functionality is added
                        //int unreadCount = 0;
                        //#################################################################################


                        if (unreadCount > 0)
                        {
                            _logger.Debug($"Messages to process: {unreadCount}.  0 count of messages are only recorded in verbose logging mode\n");

                            await Task.Delay(1000, stoppingToken);

                        }
                        else if (unreadCount == 0)
                        {
                            _logger.Verbose("...");
                            _logger.Verbose($"No Messages to Process pausing for {timeDelay} seconds. \nSet in SQL DB AppConfig Table under TimeDelay");
                            _logger.Verbose("Worker Loop - Pre Schedule");
                            await Task.Delay(timeDelay, stoppingToken); //Pausing for seconds set in SQL DB AppConfig Table under TimeDelay

                        }

                    }
                    else
                    {
                        if (stoppingToken.IsCancellationRequested || _cancellationTokenSource.IsCancellationRequested ||
                            cancelTokenIssued == true)
                        {
                            _logger.Information("\n ... Cancellation requested. Exiting worker service loop... \n");
                            break;
                        }

                        await Task.Delay(500, stoppingToken);
                        await _confidentialClientApp.GetAccessToken();
                        _logger.Debug("Aquired Bearer Token");
                    }

                }
                catch (OperationCanceledException)
                {
                    _logger.Information("Operation canceled. Exiting loop.");
                    break; // Exit the loop on cancellation
                }
                catch (Exception ex)
                {
                    _logger.Error($"Worker.cs Failed to acquire an Access Bearer Token. Check stuff... {ex}");
                }
            }

            _logger.Information("Exiting ExecuteAsync loop.");

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

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Worker stopping at:::::: {time}", DateTimeOffset.Now);

            if (_cancellationTokenSource != null)
            {
                _logger.Information("_cancellationTokenSource is not null");
                _cancellationTokenSource.Cancel();
            }

            _cancellationTokenSource?.Dispose();


            cancelTokenIssued = true;

            _logger.Information($"From StopAsync - cancelTokenIssued = {cancelTokenIssued}");

            //// Perform cleanup or additional tasks before stopping the worker.
            //_schedulerTimer?.Dispose();

            await base.StopAsync(cancellationToken);
        }
    }
}
