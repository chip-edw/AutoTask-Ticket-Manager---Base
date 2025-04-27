namespace AutoTaskTicketManager_Base.ManagementAPI
{
    public interface IManagementService
    {
        Task<int> GetCompanyCountAsync();
        Task<List<string>> GetSubjectExclusionKeywordsAsync();
        Task<List<string>> GetSenderExclusionsAsync();
    }
}
