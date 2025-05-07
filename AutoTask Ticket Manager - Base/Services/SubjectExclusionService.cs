using AutoTaskTicketManager_Base.Dtos;
using AutoTaskTicketManager_Base.Models;
using Microsoft.EntityFrameworkCore;


namespace AutoTaskTicketManager_Base.Services
{
    public class SubjectExclusionService : ISubjectExclusionService
    {
        private readonly ApplicationDbContext _db;

        public SubjectExclusionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<SubjectExclusionKeywordDto>> GetAllAsync()
        {
            return await _db.SubjectExclusionKeywords
                .OrderBy(e => e.Keyword)
                .Select(e => new SubjectExclusionKeywordDto
                {
                    Id = e.Id,
                    Keyword = e.Keyword,
                    CreatedOn = e.CreatedOn
                })
                .ToListAsync();
        }

        public async Task<SubjectExclusionKeywordDto> AddAsync(SubjectExclusionKeywordDto dto)
        {
            var entity = new SubjectExclusionKeyword
            {
                Keyword = dto.Keyword.Trim(),
                CreatedOn = DateTime.UtcNow
            };

            _db.SubjectExclusionKeywords.Add(entity);
            await _db.SaveChangesAsync();

            return new SubjectExclusionKeywordDto
            {
                Id = entity.Id,
                Keyword = entity.Keyword,
                CreatedOn = entity.CreatedOn
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.SubjectExclusionKeywords.FindAsync(id);
            if (entity == null)
                return false;

            _db.SubjectExclusionKeywords.Remove(entity);
            return await _db.SaveChangesAsync() > 0;
        }
    }

}
