using Microsoft.EntityFrameworkCore;
using Domain.Contracts.Repositories;
using LMS.Infrastructure.Data;
using Domain.Models.Entities;

namespace LMS.Infrastructure.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Activity>> GetByModuleIdAsync(Guid moduleId)
    {
        return await _context.Activities
            .Include(a => a.ActivityType)
            .Where(a => a.ModuleId == moduleId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<Activity?> GetByIdAsync(Guid id)
    {
        return await _context.Activities
            .Include(a => a.ActivityType)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Activity?> FindOverlappingAsync(Guid moduleId, 
        DateTime startTime, 
        DateTime endTime,
        Guid? excludeActivityId)
    {
        return await _context.Activities
            .Where(a => a.ModuleId == moduleId)
            .Where(a => excludeActivityId == null || a.Id != excludeActivityId)
            .Where(a => startTime < a.EndTime && endTime > a.StartTime)
            .FirstOrDefaultAsync();
    }

    public void Create(Activity activity) => _context.Activities.Add(activity);
    public void Update(Activity activity) => _context.Activities.Update(activity);
    public void Delete(Activity activity) => _context.Activities.Remove(activity);
}
