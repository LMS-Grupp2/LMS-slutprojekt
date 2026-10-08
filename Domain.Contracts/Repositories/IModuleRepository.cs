using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface IModuleRepository
{
    Task<IEnumerable<Module>> GetByCourseIdAsync(Guid courseId);
    Task<Module?> GetByIdAsync(Guid id, bool trackChanges = false);
    void Create(Module module);
}
