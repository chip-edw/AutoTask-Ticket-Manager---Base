namespace AutoTaskTicketManager_Base.Services
{
    public interface IManagementService
    {
        Task<int> GetCompanyCountAsync();
        Task<List<string>> GetSubjectExclusionKeywordsAsync();
        Task<List<string>> GetSenderExclusionsAsync();
    }
}
