namespace Domain.Models.Entities;

public class Activity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public Guid ActivityTypeId { get; set; }
    public ActivityType ActivityType { get; set; } = null!;

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public ICollection<Submission> Submissions { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];
}
