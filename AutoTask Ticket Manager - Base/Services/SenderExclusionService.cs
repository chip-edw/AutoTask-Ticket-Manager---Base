using AutoTaskTicketManager_Base.Dtos;
using AutoTaskTicketManager_Base.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoTaskTicketManager_Base.Services
{

    public class SenderExclusionService : ISenderExclusionService
    {
        private readonly ApplicationDbContext _db;

        public SenderExclusionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<SenderExclusionDto>> GetAllAsync()
        {
            return await _db.SenderExclusions
                .OrderBy(e => e.Email)
                .Select(e => new SenderExclusionDto
                {
                    Id = e.Id,
                    Email = e.Email
                })
                .ToListAsync();
        }

        public async Task<SenderExclusionDto> AddAsync(SenderExclusionDto dto)
        {
            var entity = new SenderExclusion
            {
                Email = dto.Email.Trim(),
                CreatedOn = DateTime.UtcNow
            };

            _db.SenderExclusions.Add(entity);
            await _db.SaveChangesAsync();

            return new SenderExclusionDto
            {
                Id = entity.Id,
                Email = entity.Email
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.SenderExclusions.FindAsync(id);
            if (entity == null)
                return false;

            _db.SenderExclusions.Remove(entity);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}