using LMS.Shared.DTOs.ActivityDtos;

namespace Service.Contracts;

/// <summary>Business operations for activities, used by the activities controller.</summary>
public interface IActivityService
{
    /// <summary>Gets a module's activities, earliest start time first.</summary>
    Task<IEnumerable<ActivityDto>> GetByModuleIdAsync(Guid moduleId);
   
    /// <summary>Gets one activity. Throws a 404 (ActivityNotFoundException) if it does not exist.</summary>
    Task<ActivityDto> GetByIdAsync(Guid id);

    /// <summary>Gets all activity types, sorted by name. Used for the type dropdown.</summary>
    Task<IEnumerable<ActivityTypeDto>> GetActivityTypesAsync();

    /// <summary>
    /// Creates an activity. Throws a 400 (BadRequestException) if end is not after start or the activity
    /// is outside the module's dates, a 404 if the module doesn't exist, or a 409 (ConflictException)
    /// if it overlaps another activity in the module.
    /// </summary>
    Task<ActivityDto> CreateAsync(CreateActivityDto dto);

    /// <summary>Updates an activity. Throws a 404 if it does not exist, plus the same 400/409 as create.</summary>
    Task UpdateAsync(Guid id, UpdateActivityDto dto);

    /// <summary>Deletes an activity. Throws a 404 if it does not exist.</summary>
    Task DeleteAsync(Guid id);
}
