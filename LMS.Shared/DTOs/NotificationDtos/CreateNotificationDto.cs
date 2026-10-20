using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.NotificationDtos;

public record CreateNotificationDto
{
    public required string EventType { get; init; }  // Use constant 

    [Required]
    [MaxLength(100)]
    public required string Message { get; init; }
    //public bool IsRead { get; init; }
    public DateTime CreatedAd { get; init; } = DateTime.Now;

    // Must have RecipientId, CourseId, ModuleId or ActivityId
    public string? RecipientId { get; init; } = null!;
    public Guid? CourseId { get; init; }
    public Guid? ModuleId { get; init; }
    public Guid? ActivityId { get; init; }
}

// Document - alla i kurs
// Feedback - RecipientId
// CourseId, studentId (inte till sig själv för eget dokument)
