namespace LMS.Shared.DTOs.CourseDtos;

public sealed record StudentCourseDto(
    Guid Id,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<CourseParticipantDto> Participants);