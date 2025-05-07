using AutoTaskTicketManager_Base.Dtos;

namespace AutoTaskTicketManager_Base.Services
{
    public interface ISenderExclusionService
    {
        Task<List<SenderExclusionDto>> GetAllAsync();
        Task<SenderExclusionDto> AddAsync(SenderExclusionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
