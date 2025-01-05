using Serilog;
using System.Diagnostics;


namespace ATTMWS_L.ManagementAPI
{

    public class ManagementApiHelper
    {

        public ManagementApiHelper()
        {
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
    }
}
