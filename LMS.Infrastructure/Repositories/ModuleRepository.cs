using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class ModuleRepository(ApplicationDbContext context) : IModuleRepository
{
    public async Task<IEnumerable<Module>> GetByCourseIdAsync(Guid courseId) =>
        await context.Modules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.StartDate)
            .ToListAsync();

    public async Task<Module?> GetByIdAsync(Guid id, bool trackChanges = false)
    {
        var query = context.Modules.Where(m => m.Id == id);
        if (!trackChanges) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync();
    }

    public void Create(Module module) => context.Modules.Add(module);
}
