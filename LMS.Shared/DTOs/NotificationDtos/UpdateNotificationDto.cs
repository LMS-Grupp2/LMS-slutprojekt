namespace LMS.Shared.DTOs.NotificationDtos;

public record UpdateNotificationDto
{
    public required Guid Id { get; init; }
    public bool? IsRead { get; init; }
}
