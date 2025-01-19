using Azure.Identity;
using Microsoft.Graph;

namespace AutoTaskTicketManager_Base.MSGraphAPI
{
    public class GraphServiceClientSingleton
    {
        private static GraphServiceClient _instance;
        private static readonly object _lockObject = new object();
        public static AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

        private GraphServiceClientSingleton()
        {
            // Private constructor to prevent direct instantiation
        }

        public static GraphServiceClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = CreateGraphServiceClient();
                        }
                    }
                }
                return _instance;
            }
        }

        private static GraphServiceClient CreateGraphServiceClient()
        {
            var clientSecretCredential = new ClientSecretCredential(
                config.Tenant, config.ClientId, config.ClientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential);

            // Additional configuration if needed

            return graphClient;
        }

    }
}
