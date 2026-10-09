namespace LMS.Shared.DTOs.ModuleDtos;

public sealed record ModuleDto(
    Guid Id,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid CourseId);
