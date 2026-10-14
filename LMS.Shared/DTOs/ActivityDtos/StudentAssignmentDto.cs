namespace LMS.Shared.DTOs.ActivityDtos;

public record StudentAssignmentDto
{
    // Activity
    public Guid ActivityId { get; init; }
    public required string ActivityName { get; init; }
    public string? ActivityDescription { get; init; }
    public DateTime EndTime { get; init; }

    // Module
    public required Guid ModuleId { get; init; }
    public required string ModuleName { get; init; }

    // Course
    public Guid CourseId { get; init; }
    public required string CourseName { get; init; }

    // Submission
    public bool Submitted { get; init; }
    public Guid? SubmissionId { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public string? Status { get; init; }
    public string? Grade { get; init; }
}
