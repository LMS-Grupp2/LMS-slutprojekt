namespace Domain.Models.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public ICollection<Module> Modules { get; set; } = [];
    public ICollection<CourseUser> CourseUsers { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];
}
