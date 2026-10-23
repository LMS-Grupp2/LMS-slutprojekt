using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.Paging;

public record PagedQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>1-based page number. ?page=0 or a negative value is a 400.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "page must be 1 or greater.")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Items per page. Too large is rejected (400), not clamped, so a client that asks for 1000
    /// learns the limit instead of silently getting 100.
    /// </summary>
    [Range(1, MaxPageSize, ErrorMessage = "pageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = DefaultPageSize;
}
