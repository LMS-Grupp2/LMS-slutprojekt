using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.ModuleDtos;

public record UpdateModuleDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = null!;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Required]
    public DateOnly? StartDate { get; init; }

    [Required]
    public DateOnly? EndDate { get; init; }
}