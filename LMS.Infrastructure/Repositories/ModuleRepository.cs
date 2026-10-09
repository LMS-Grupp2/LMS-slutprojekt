using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

/// <summary>
/// Database access for modules. The repository never saves changes,
/// saving is done once by the service through the unit of work.
/// </summary>
public class ModuleRepository(ApplicationDbContext context) : IModuleRepository
{
    /// <summary>Returns the modules of a course, earliest start date first. Read only.</summary>
    public async Task<IEnumerable<Module>> GetByCourseIdAsync(Guid courseId) =>
        await context.Modules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.StartDate)
            .ToListAsync();

    /// <summary>Returns a module by id. Tracking is only turned on when the module will be changed.</summary>
    public async Task<Module?> GetByIdAsync(Guid id, bool trackChanges = false)
    {
        var query = context.Modules.Where(m => m.Id == id);
        if (!trackChanges) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>Adds the module to the context. It is saved later by the unit of work.</summary>
    public void Create(Module module) => context.Modules.Add(module);

    /// <summary>
    /// Finds another module in the same course that overlaps the given period.
    /// Two periods overlap when each one starts on or before the day the other ends.
    /// The module being edited is excluded, so it is never compared with itself.
    /// </summary>
    public async Task<Module?> FindOverlappingAsync(
        Guid courseId, DateOnly startDate, DateOnly endDate, Guid? excludeModuleId) =>
        await context.Modules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId
                        && m.StartDate <= endDate
                        && m.EndDate >= startDate
                        && (excludeModuleId == null || m.Id != excludeModuleId))
            .OrderBy(m => m.StartDate)
            .FirstOrDefaultAsync();
}