using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

/// <summary>Database operations for modules. Saving is handled by the unit of work.</summary>
public interface IModuleRepository
{
    /// <summary>Gets the modules of a course, ordered by start date. Read only.</summary>
    Task<IEnumerable<Module>> GetByCourseIdAsync(Guid courseId);

    /// <summary>Gets a module by id. Set trackChanges to true when the module will be updated.</summary>
    Task<Module?> GetByIdAsync(Guid id, bool trackChanges = false);

    /// <summary>Adds a new module. It is saved by the unit of work.</summary>
    void Create(Module module);

    /// <summary>
    /// Finds another module in the same course whose dates overlap the given period,
    /// or null if there is none. Pass the module's own id when updating,
    /// so a module is never compared with itself.
    /// </summary>
    Task<Module?> FindOverlappingAsync(Guid courseId, DateOnly startDate, DateOnly endDate, Guid? excludeModuleId);
}