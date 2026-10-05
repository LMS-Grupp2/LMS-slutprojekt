using Domain.Models.Enums;

namespace Domain.Models.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public FileType FileType { get; set; }
    public DateTime UploadedAt { get; set; }

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }

    public Guid? ModuleId { get; set; }
    public Module? Module { get; set; }

    public Guid? ActivityId { get; set; }
    public Activity? Activity { get; set; }

    public Guid? SubmissionId { get; set; }
    public Submission? Submission { get; set; }
}
