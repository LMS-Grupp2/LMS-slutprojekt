namespace Domain.Models.Entities;

public class CourseUser
{

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
