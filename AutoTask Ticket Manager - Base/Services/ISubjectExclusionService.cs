using AutoTaskTicketManager_Base.Dtos;

namespace AutoTaskTicketManager_Base.Services
{
    public interface ISubjectExclusionService
    {
        Task<List<SubjectExclusionKeywordDto>> GetAllAsync();
        Task<SubjectExclusionKeywordDto> AddAsync(SubjectExclusionKeywordDto dto);
        Task<bool> DeleteAsync(int id);
    }

}
