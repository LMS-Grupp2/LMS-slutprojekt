using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.Paging;

public sealed record UserListQuery : PagedQuery
{
    /// <summary>Matched against name and email (contains, case-insensitive in SQL Server).</summary>
    [MaxLength(100, ErrorMessage = "search can be at most 100 characters.")]
    public string? Search { get; init; }

    /// <summary>Exact role name, e.g. "Student" or "Teacher".</summary>
    public string? Role { get; init; }

    /// <summary>Only users linked to this course.</summary>
    public Guid? CourseId { get; init; }
}
