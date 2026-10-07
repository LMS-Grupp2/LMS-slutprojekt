namespace Domain.Models.Entities;

public class ActivityType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; } 

    public ICollection<Activity> Activities { get; set; } = [];
}
