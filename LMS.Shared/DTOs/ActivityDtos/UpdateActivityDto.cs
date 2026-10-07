using System.ComponentModel.DataAnnotations;


namespace LMS.Shared.DTOs.ActivityDtos;

public record UpdateActivityDto
{
    [Required]
    public string Type { get; init; } = null!;

    [Required]
    [MaxLength(50)]
    public string Name { get; init; } = null!;

    [MaxLength(400)]
    public string? Description { get; init; }

    [Required]
    public DateTime? StartTime { get; init; }

    [Required]
    public DateTime? EndTime { get; init; }
}
