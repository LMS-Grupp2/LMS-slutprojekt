using Domain.Models.Enums;

namespace Domain.Models.Entities;

public class Submission
{
    public Guid Id { get; set; }
    public DateTime SubmittedAt { get; set; }
    public Grade? Grade { get; set; }
    public bool Approved { get; set; }
    public bool NeedChanges { get; set; }

    public Guid ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;

    public string StudentId { get; set; } = null!;
    public ApplicationUser Student { get; set; } = null!;

    public ICollection<Feedback> Feedbacks { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];
}
