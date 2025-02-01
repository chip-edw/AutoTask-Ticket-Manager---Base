using RestSharp;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public interface IApiClient
    {
        Task<RestResponse> GetAsync(string resource);
        Task<RestResponse> PostAsync(string resource, object body);
        Task<RestResponse> PatchAsync(string resource, object body);
        Task<RestResponse> DeleteAsync(string resource);
    }
}
