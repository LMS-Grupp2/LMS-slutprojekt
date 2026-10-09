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

    // Read only (the schedule list), so tracking is turned off.          
    // Include is needed so MapToDto can read ActivityType.Name. 
    public async Task<IEnumerable<Activity>> GetByModuleIdAsync(Guid moduleId)
    {
        return await _context.Activities
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Where(a => a.ModuleId == moduleId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    // Tracked on purpose: UpdateAsync and DeleteAsync change the entity and save it.  
    // Include is needed so MapToDto can read ActivityType.Name. 
    public async Task<Activity?> GetByIdAsync(Guid id)
    {
        return await _context.Activities
            .Include(a => a.ActivityType)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    // Overlap = starts before the other ends AND ends after the other starts.
    // Strict < and > so back-to-back activities (10:00 end, 10:00 start) are allowed.
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

    // Read only (the dropdown list), so tracking is turned off
    public async Task<IEnumerable<ActivityType>> GetActivityTypesAsync()
    {
        return await _context.ActivityTypes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public void Create(Activity activity) => _context.Activities.Add(activity);
    public void Update(Activity activity) => _context.Activities.Update(activity);
    public void Delete(Activity activity) => _context.Activities.Remove(activity);
}
