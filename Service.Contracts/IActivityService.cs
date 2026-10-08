using LMS.Shared.DTOs.ActivityDtos;

namespace Service.Contracts;

public interface IActivityService
{
    Task<IEnumerable<ActivityDto>> GetByModuleIdAsync(Guid moduleId);
    Task<ActivityDto> GetByIdAsync(Guid id);
    Task<ActivityDto> CreateAsync(CreateActivityDto dto);
    Task UpdateAsync(Guid id, UpdateActivityDto dto);
    Task DeleteAsync(Guid id);
}
