namespace LMS.Shared.DTOs.CourseDtos;

public record CourseDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }

    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }

    // TODO - add collections to CourseDto
    //public ICollection<Modules> Modules { get; init; } = [];
    //public ICollection<CourseUsers> CourseUsers { get; init; } = [];
    //public ICollection<Documents> Documents { get; set; } = [];
}
