namespace LMS.Shared.DTOs.CourseDtos;

public record UpdateCourseDto : CreateCourseDto
{
    public required Guid Id { get; init; }
}
