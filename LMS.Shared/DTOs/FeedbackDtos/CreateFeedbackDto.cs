using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.FeedbackDtos;

public record CreateFeedbackDto
{
    [Required]
    [MaxLength(1000)]
    public string Text { get; init; } = null!;

    [Required]
    public bool? IsApproved { get; init; }
}
