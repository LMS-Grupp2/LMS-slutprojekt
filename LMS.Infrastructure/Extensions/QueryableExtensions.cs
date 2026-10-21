using LMS.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Runs two queries: a COUNT of the whole filtered list, then SELECT ... OFFSET/FETCH for the page.
    /// Call it last, after Where and OrderBy. The query must be ordered, or SQL Server returns
    /// pages in an undefined order and the same row can appear on two pages.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
