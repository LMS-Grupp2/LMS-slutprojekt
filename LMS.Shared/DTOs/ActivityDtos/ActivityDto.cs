namespace LMS.Shared.DTOs.ActivityDtos;

/// <summary>
/// Output for GET and POST. Only these fields leave the API, never the entity itself.
/// </summary>
public sealed record ActivityDto(
    Guid Id,
    Guid ActivityTypeId,
    string ActivityTypeName,
    string Name,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    Guid ModuleId);

