using AutoTaskTicketManager_Base.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;


namespace AutoTaskTicketManager_Base.ManagementAPI
{

    public class ManagementApiHelper
    {
        private static DbContextOptions<ApplicationDbContext> _dbOptions;

        public ManagementApiHelper(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        // Architecture Note:
        // =============================================================
        // ManagementApiHelper operates as a static helper class
        // for occasional lightweight database access (e.g., internal
        // Admin UI operations like company counts).
        //
        // It uses a static DbContextOptions<ApplicationDbContext> (_dbOptions)
        // which must be initialized ONCE at application startup.
        //
        // _dbOptions must be assigned from a root-level DI service (not scoped),
        // because scoped services are disposed after their lifetime ends,
        // causing ObjectDisposedException if reused later.
        //
        // ManagementApiHelper is NOT used inside background threads or schedulers,
        // so static access to a root-level DbContextOptions is acceptable.
        //
        // Future Improvement:
        // Consider migrating to an injected scoped service or using a scoped DbContext factory
        // if Management API operations become heavier or multithreaded.
        // =============================================================


        public static void Initialize(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }


        public static async void TestManagementAPI()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            int port = configuration.GetValue<int>("ManagementApiPort");

            // Internal test of Maintenance API. Should log Port and IP Address
            // Does not work well with Linux is internal poer access has not had permissions granted to the application
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"http://localhost:{port}/");

                if (response.IsSuccessStatusCode)
                {
                    var listeningUrl = await response.Content.ReadAsStringAsync();
                    Log.Debug("Listening on: {0}", listeningUrl);
                }
                else
                {
                    Console.WriteLine($"Failed to access the API. Status Code: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Error($"Error response: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception occurred while testing the Management API: {ex.Message}");
            }
        }


        public static async Task<string> RunCurlCommand()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            int port = configuration.GetValue<int>("ManagementApiPort");
            string url = "http://localhost:" + port;

            Log.Debug($"curl command: {url}\n");

            using (var process = new Process())
            {
                process.StartInfo.FileName = "curl";
                process.StartInfo.Arguments = url;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string result = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"Error running curl: {error}");
                }

                return result;
            }
        }

        public static async Task<List<string>> GetSubjectExclusionKeyWordsFromList()
        {
            //Return the StartupConfiguration public List subjectExclusionKeyWordList
            //return StartupConfiguration.subjectExclusionKeyWordList;
            List<string> fakeList = new List<string> { "1", "2", "3", "4", "5" };
            return fakeList;

        }

        public static async Task<List<string>> GetSenderExclusionsFromList()
        {
            //Return the StartupConfiguration public List subjectExclusionKeyWordList
            return StartupConfiguration.senderExclusionsList;

        }

        public static async Task<string> GetCompanyCountFromSql()
        {
            try
            {
                using (var context = new ApplicationDbContext(_dbOptions)) // DbContext name from Models Class
                {
                    Log.Debug("Attempting to load the Company Count from DB.");

                    // Query to count all entries in CustomerSettings
                    int count = await context.CustomerSettings.CountAsync();

                    Log.Debug("Retrieved Count of Companies from SQL successfully...");

                    return count.ToString();
                }
            }
            catch (Exception ex) // Catching a more general exception as Entity Framework might throw different exceptions
            {
                Log.Error("Unable to retrieve the Company Count from DB. Exception: ", ex);
                return "failure";
            }
        }
    }
}
