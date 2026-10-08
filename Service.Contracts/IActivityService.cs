using LMS.Shared.DTOs.ActivityDtos;

namespace Service.Contracts;

/// <summary>Business operations for activities, used by the activities controller.</summary>
public interface IActivityService
{
    /// <summary>Gets a module's activities, earliest start time first.</summary>
    Task<IEnumerable<ActivityDto>> GetByModuleIdAsync(Guid moduleId);
   
    /// <summary>Gets one activity. Throws a 404 (ActivityNotFoundException) if it does not exist.</summary>
    Task<ActivityDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates an activity. Throws a 400 (BadRequestException) if end is not after start,
    /// or a 409 (ConflictException) if it overlaps another activity in the module.
    /// </summary>
    Task<ActivityDto> CreateAsync(CreateActivityDto dto);

    /// <summary>Updates an activity. Throws a 404 if it does not exist, plus the same 400/409 as create.</summary>
    Task UpdateAsync(Guid id, UpdateActivityDto dto);

    /// <summary>Deletes an activity. Throws a 404 if it does not exist.</summary>
    Task DeleteAsync(Guid id);
}
