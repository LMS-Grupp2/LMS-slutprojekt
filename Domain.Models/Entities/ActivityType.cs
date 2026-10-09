namespace Domain.Models.Entities;


/// <summary>
/// A table instead of an enum/string, so teachers can add new types without code changes.
/// </summary>
public class ActivityType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }


    // One type is used by many activities (one-to-many, configured in ApplicationDbContext).
    public ICollection<Activity> Activities { get; set; } = [];
}
