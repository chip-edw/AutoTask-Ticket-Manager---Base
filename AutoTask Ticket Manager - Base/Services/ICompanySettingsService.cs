using AutoTaskTicketManager_Base.Models;

namespace AutoTaskTicketManager_Base.Services
{
    public interface ICompanySettingsService
    {
        /// <summary>
        /// Get all company settings from the SQL database.
        /// </summary>
        Task<IEnumerable<CustomerSettings>> GetAllCompanySettingsFromSqlAsync();

        /// <summary>
        /// Update a single company's settings.
        /// </summary>
        Task UpdateCompanySettingAsync(CustomerSettings company);

        /// <summary>
        /// Update multiple companies' settings at once (batch).
        /// </summary>
        Task UpdateCompanySettingsBatchAsync(List<CustomerSettings> companies);

        /// <summary>
        /// Refresh the in-memory cache of company settings.
        /// </summary>
        Task RefreshMemoryAsync();
    }
}
