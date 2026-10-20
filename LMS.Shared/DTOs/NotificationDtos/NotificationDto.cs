namespace LMS.Shared.DTOs.NotificationDtos;

public record NotificationDto
{
    public Guid Id { get; init; }
    public required string EventType { get; init; }
    public required string Message { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAd { get; init; }
}