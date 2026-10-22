using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.NotificationDtos;

public record CreateNotificationDto
{
    public required string EventType { get; init; }  // Use NotificationEventTypes

    [Required]
    [MaxLength(100)]
    public required string Message { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    // Must have one of: RecipientId, CourseId, ModuleId, ActivityId
    public string? RecipientId { get; init; } = null!;
    public Guid? CourseId { get; init; }
    public Guid? ModuleId { get; init; }
    public Guid? ActivityId { get; init; }
}