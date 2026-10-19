using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.NotificationDtos;

public record CreateNotifications
{
    //public Guid Id { get; init; }
    public required string EventType { get; init; }  // TODO - EventType: Feedback/Document

    [Required]
    [MaxLength(100)]
    public required string Message { get; init; }
    //public bool IsRead { get; init; }
    public DateTime CreatedAd { get; init; } = DateTime.Now;
    public required string RecipientId { get; init; } = null!;
    //public Domain.Models.Entities.ApplicationUser Recipient { get; init; } = null!;
}

