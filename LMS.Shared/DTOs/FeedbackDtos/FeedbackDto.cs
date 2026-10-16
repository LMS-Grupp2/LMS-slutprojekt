namespace LMS.Shared.DTOs.FeedbackDtos;

public record FeedbackDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public string TeacherId { get; init; } = null!;
    public string TeacherName { get; init; } = null!;
}
