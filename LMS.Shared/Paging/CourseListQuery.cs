using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.Paging;


public sealed record CourseListQuery
{
    /// <summary>Matched against the course name (contains).</summary>
    [MaxLength(100, ErrorMessage = "search can be at most 100 characters.")]
    public string? Search { get; init; }

    /// <summary>One of the names in CourseStatus: Upcoming, Active or Finished. Compared against today's date.</summary>
    public string? Status { get; init; }
}