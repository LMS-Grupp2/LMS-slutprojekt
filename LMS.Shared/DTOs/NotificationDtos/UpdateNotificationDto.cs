namespace LMS.Shared.DTOs.NotificationDtos;

public record UpdateNotificationDto
{
    public Guid Id { get; init; }
    public bool? IsRead { get; init; }
}
