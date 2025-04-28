using AutoTaskTicketManager_Base.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AutoTaskTicketManager_Base.ManagementAPI
{
    public class ManagementService : IManagementService
    {
        private readonly ApplicationDbContext _dbContext;

        public ManagementService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetCompanyCountAsync()
        {
            try
            {
                Log.Debug("Attempting to load the Company Count from DB.");

                int count = await _dbContext.CustomerSettings.CountAsync();

                Log.Debug("Retrieved Count of Companies from SQL successfully.");

                return count;
            }
            catch (Exception ex)
            {
                Log.Error("Unable to retrieve the Company Count from DB. Exception: {ExceptionMessage}", ex.Message);
                throw;  // Let the caller handle the error (e.g., return 500)
            }
        }

        public async Task<List<string>> GetSubjectExclusionKeywordsAsync()
        {
            try
            {
                var keywords = await _dbContext.SubjectExclusionKeywords
                    .Select(x => x.SubjectKeyWord)
                    .ToListAsync();

                return keywords;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to retrieve Subject Exclusion Keywords. Exception: {ExceptionMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<string>> GetSenderExclusionsAsync()
        {
            try
            {
                var exclusions = await _dbContext.SenderExclusions
                    .Select(x => x.SenderAddress)
                    .ToListAsync();

                return exclusions;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to retrieve Sender Exclusions. Exception: {ExceptionMessage}", ex.Message);
                throw;
            }
        }


    }
}
