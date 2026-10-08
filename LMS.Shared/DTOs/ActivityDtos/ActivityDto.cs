namespace LMS.Shared.DTOs.ActivityDtos;

public sealed record ActivityDto(
    Guid Id,
    Guid ActivityTypeId,
    string ActivityTypeName,
    string Name,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    Guid ModuleId);

