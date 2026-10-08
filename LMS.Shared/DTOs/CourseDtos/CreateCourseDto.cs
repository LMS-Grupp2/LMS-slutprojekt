using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.CourseDtos;

/// <summary>
/// Input for creating a course. Also used as the base for updating a course.
/// [ApiController] returns 400 automatically when these rules are broken.
/// </summary>
public record CreateCourseDto
{
    // Real course names can be long, e.g. "Fullstack .NET Systemutvecklare" (31 characters).
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters long.")]
    public required string Name { get; init; }

    // The description is a free text, so it needs room for a few sentences.
    [StringLength(1000, ErrorMessage = "{0} must be at most {1} characters long.")]
    public string? Description { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    // The rule "start date cannot be after end date" is checked in CourseService.
    [Required]
    public DateOnly EndDate { get; init; }
}