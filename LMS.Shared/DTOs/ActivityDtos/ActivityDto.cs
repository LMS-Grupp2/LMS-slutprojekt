namespace LMS.Shared.DTOs.ActivityDtos;

public sealed record ActivityDto(
    Guid Id,
    string Type,
    string Name,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    Guid ModuleId);

