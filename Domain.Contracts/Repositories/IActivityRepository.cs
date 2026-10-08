using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface IActivityRepository
{
    Task<IEnumerable<Activity>> GetByModuleIdAsync(Guid moduleId);
    Task<Activity?> GetByIdAsync(Guid id);
    void Create(Activity activity);
    void Update(Activity activity);
    void Delete(Activity activity);

    Task<Activity?> FindOverlappingAsync(Guid moduleId, DateTime startTime, 
        DateTime endTime, Guid? excludeActivityId);
}
