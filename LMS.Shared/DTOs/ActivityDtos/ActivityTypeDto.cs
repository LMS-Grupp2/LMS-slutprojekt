namespace LMS.Shared.DTOs.ActivityDtos;


/// <summary>Output for GET api/activity-types. Used by the activity form's type dropdown.</summary>
public sealed record ActivityTypeDto(Guid Id, string Name, string? Description);

