using Domain.Contracts;
using Service.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ActivityDtos;


namespace LMS.Services;

/// <summary>
/// Business rules for activities: end must be after start (400), inside the module's dates (400)
/// and no overlap within the same module (409). Controllers call this service through the interface,
/// and it talks to the database only through the unit of work.
/// </summary>
public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ActivityDto>> GetByModuleIdAsync(Guid moduleId)
    {
        var activities = await _unitOfWork.Activities.GetByModuleIdAsync(moduleId);
        return activities.Select(MapToDto).ToList();
    }

    public async Task<ActivityDto> GetByIdAsync(Guid id)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(id)
            ?? throw new ActivityNotFoundException(id);

        return MapToDto(activity);
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityDto dto)
    {
        await ValidateActivityAsync(dto.ModuleId!.Value, dto.StartTime!.Value, 
            dto.EndTime!.Value, null );

        var activity = new Activity
        {
            ActivityTypeId = dto.ActivityTypeId!.Value,
            Name = dto.Name,
            Description = dto.Description,
            StartTime = dto.StartTime!.Value,
            EndTime = dto.EndTime!.Value,
            ModuleId = dto.ModuleId!.Value
        };

        _unitOfWork.Activities.Create(activity);
        await _unitOfWork.SaveChangesAsync();

        // Reload instead of MapToDto(activity): the new entity only has ActivityTypeId,
        // its ActivityType navigation is null. GetByIdAsync includes it for the type name.
        return await GetByIdAsync(activity.Id);
    }

    public async Task UpdateAsync(Guid id, UpdateActivityDto dto)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(id)
            ?? throw new ActivityNotFoundException(id);

        await ValidateActivityAsync(activity.ModuleId, dto.StartTime!.Value, dto.EndTime!.Value, 
            activity.Id);

        activity.ActivityTypeId = dto.ActivityTypeId!.Value;
        activity.Name = dto.Name;
        activity.Description = dto.Description;
        activity.StartTime = dto.StartTime!.Value;
        activity.EndTime = dto.EndTime!.Value;

        // Tracked by the context, so changing the properties and saving is enough. No Update call needed.
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(id)
            ?? throw new ActivityNotFoundException(id);

        _unitOfWork.Activities.Delete(activity);
        await _unitOfWork.SaveChangesAsync();
    }


    /* Helper */

    /// <summary>
    /// Single place that maps an Activity to its DTO. Requires ActivityType to be loaded (Include).
    /// </summary>
    private static ActivityDto MapToDto(Activity activity)
    {
        return new ActivityDto(
            activity.Id,
            activity.ActivityTypeId,
            activity.ActivityType.Name,
            activity.Name,
            activity.Description,
            activity.StartTime,
            activity.EndTime,
            activity.ModuleId
        );
    }

    /// <summary>
    /// Runs the activity rules before create/update: 400 if end is not after start,
    /// 404 if the module doesn't exist, 400 if the activity is outside the module's dates,
    /// 409 if it overlaps another activity in the module. excludeActivityId = own id on update, null on create.
    /// </summary>
    private async Task ValidateActivityAsync(Guid moduleId, DateTime startTime, DateTime endTime,
        Guid? excludeActivityId)
    {
        if (endTime <= startTime)
            throw new BadRequestException("End time must be after start time.");

        var module = await _unitOfWork.Modules.GetByIdAsync(moduleId)
            ?? throw new ModuleNotFoundException(moduleId);

        // Module dates are DateOnly and activity times are DateTime, so compare on the date part only.
        // That way an activity at 16:00 on the module's last day still counts as inside.
        var activityStart = DateOnly.FromDateTime(startTime);
        var activityEnd = DateOnly.FromDateTime(endTime);

        if (activityStart < module.StartDate || activityEnd > module.EndDate)
            throw new BadRequestException("The activity must be within the module's dates.");

        var overlapping = await _unitOfWork.Activities.FindOverlappingAsync(moduleId, startTime,
            endTime, excludeActivityId);

        if (overlapping is not null)
            throw new ConflictException($"The activity overlaps with '{overlapping.Name}' " +
                $"({overlapping.StartTime:g} – {overlapping.EndTime:g}).");
    }
}
