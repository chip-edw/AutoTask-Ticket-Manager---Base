using AutoTaskTicketManager_Base.AutoTaskAPI;
using PluginContracts;

namespace AutoTaskTicketManager_Base.Services
{
    public class OpenTicketService : IOpenTicketService
    {
        public void LoadOpenTickets()
        {
            AutotaskAPIGet.GetNotCompletedTickets(); // existing logic
        }
    }
}
