using Domain.Contracts;
using Service.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ActivityDtos;
using System.Xml;

namespace LMS.Services;

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

        _unitOfWork.Activities.Update(activity);
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

    private async Task ValidateActivityAsync(Guid moduleId, DateTime startTime, DateTime endTime,
        Guid? excludeActivityId)
    {
        if (endTime <= startTime)
            throw new BadRequestException("End time must be after start time.");

        var overlapping = await _unitOfWork.Activities.FindOverlappingAsync(moduleId, startTime,
            endTime, excludeActivityId);

        if (overlapping is not null)
            throw new ConflictException($"The activity overlaps with '{overlapping.Name}' " +
                $"({overlapping.StartTime:g} – {overlapping.EndTime:g}).");
    }
}
