namespace AutoTaskTicketManager_Base.Common.Secrets
{
    public interface ISecretsProvider
    {
        Task<string?> GetApiKeyAsync(string keyName);
        Task<Dictionary<string, string>> GetAllApiKeysAsync();
    }
}
