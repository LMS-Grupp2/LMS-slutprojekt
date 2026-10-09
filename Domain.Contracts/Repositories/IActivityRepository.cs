using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

/// <summary>
/// Database access for activities. Nothing is saved at this level: Create/Update/Delete
/// only mark changes, and the service saves once through the unit of work.
/// </summary>
public interface IActivityRepository
{
    Task<IEnumerable<Activity>> GetByModuleIdAsync(Guid moduleId);
    Task<Activity?> GetByIdAsync(Guid id);

    /// <summary>True if an activity type with this id exists.</summary>
    Task<bool> ActivityTypeExistsAsync(Guid activityTypeId);
   
    void Create(Activity activity);
    void Update(Activity activity);
    void Delete(Activity activity);

    /// <summary>Returns all activity types, sorted by name. Read only.</summary>
    Task<IEnumerable<ActivityType>> GetActivityTypesAsync();

    /// <summary>
    /// Returns the first activity in the module that overlaps the given times, or null if the slot is free.
    /// On update, pass the activity's own id so it doesn't clash with itself. On create, pass null.
    /// </summary>
    Task<Activity?> FindOverlappingAsync(Guid moduleId, DateTime startTime, 
        DateTime endTime, Guid? excludeActivityId);
}
