using AutoTaskTicketManager_Base.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoTaskTicketManager_Base.Services
{
    public class CompanySettingsService : ICompanySettingsService
    {
        private readonly ApplicationDbContext _dbContext;
        private List<CustomerSettings> _companySettingsMemoryCache; // Optional in-memory cache

        public CompanySettingsService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _companySettingsMemoryCache = new List<CustomerSettings>();
        }

        /// <summary>
        /// Get all company settings directly from SQL.
        /// </summary>
        public async Task<IEnumerable<CustomerSettings>> GetAllCompanySettingsFromSqlAsync()
        {
            return await _dbContext.CustomerSettings
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Update a single company's settings.
        /// </summary>
        public async Task UpdateCompanySettingAsync(CustomerSettings company)
        {
            var existingCompany = await _dbContext.CustomerSettings
                .FirstOrDefaultAsync(c => c.AutotaskId == company.AutotaskId);

            if (existingCompany != null)
            {
                existingCompany.AccountName = company.AccountName;
                existingCompany.SupportEmail = company.SupportEmail;
                existingCompany.Enabled = company.Enabled;
                existingCompany.EnableEmail = company.EnableEmail;
                existingCompany.AutoAssign = company.AutoAssign;

                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Update multiple companies' settings at once.
        /// Will only update what has changed
        /// </summary>
        public async Task UpdateCompanySettingsBatchAsync(List<CustomerSettings> companies)
        {
            foreach (var company in companies)
            {
                var existing = await _dbContext.CustomerSettings
                    .FirstOrDefaultAsync(c => c.AutotaskId == company.AutotaskId);

                if (existing != null)
                {
                    bool changed = false;

                    if (existing.AccountName != company.AccountName)
                    {
                        existing.AccountName = company.AccountName;
                        changed = true;
                    }

                    if (existing.SupportEmail != company.SupportEmail)
                    {
                        existing.SupportEmail = company.SupportEmail;
                        changed = true;
                    }

                    if (existing.Enabled != company.Enabled)
                    {
                        existing.Enabled = company.Enabled;
                        changed = true;
                    }

                    if (existing.EnableEmail != company.EnableEmail)
                    {
                        existing.EnableEmail = company.EnableEmail;
                        changed = true;
                    }

                    if (existing.AutoAssign != company.AutoAssign)
                    {
                        existing.AutoAssign = company.AutoAssign;
                        changed = true;
                    }

                    if (changed)
                    {
                        // Explicitly mark entity as modified (optional with EF Core)
                        _dbContext.Entry(existing).State = EntityState.Modified;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// Refresh the in-memory cache from SQL database.
        /// </summary>
        public async Task RefreshMemoryAsync()
        {
            _companySettingsMemoryCache = await _dbContext.CustomerSettings
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Get the current in-memory company settings (optional).
        /// </summary>
        public IEnumerable<CustomerSettings> GetCompanySettingsFromMemory()
        {
            return _companySettingsMemoryCache;
        }
    }
}
