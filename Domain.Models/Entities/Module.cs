namespace Domain.Models.Entities;

public class Module
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<Activity> Activities { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];
}
