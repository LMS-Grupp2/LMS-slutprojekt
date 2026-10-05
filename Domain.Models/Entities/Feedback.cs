namespace Domain.Models.Entities;

public class Feedback
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public Guid SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public string TeacherId { get; set; } = null!;
    public ApplicationUser Teacher { get; set; } = null!;
}
