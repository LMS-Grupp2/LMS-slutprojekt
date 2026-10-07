using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.CourseDtos;

public record CreateCourseDto
{
    [Required]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "{0} must be min {2} and max {1} character long.")]
    public required string Name { get; init; }

    [StringLength(30, ErrorMessage = "{0} must be max {1} character long.")]
    public string? Description { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    [Required]
    public DateOnly EndDate { get; init; }
}
