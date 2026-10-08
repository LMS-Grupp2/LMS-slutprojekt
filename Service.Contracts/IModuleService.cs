using LMS.Shared.DTOs.ModuleDtos;

namespace Service.Contracts;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetByCourseIdAsync(Guid courseId);
    Task<ModuleDto> GetByIdAsync(Guid id);
    Task<ModuleDto> CreateAsync(Guid courseId, CreateModuleDto dto);
    Task UpdateAsync(Guid id, UpdateModuleDto dto);
}
