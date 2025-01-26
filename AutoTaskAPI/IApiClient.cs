using RestSharp;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public interface IApiClient
    {
        RestResponse Get(string resource);
    }
}
