using LMS.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Extensions;

/// <summary>
/// Helper for paging any EF Core query (US15).
/// It lives in LMS.Infrastructure because CountAsync and ToListAsync come from EF Core,
/// and LMS.Shared (where PagedResult lives) must not depend on database packages.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Takes a query that is already filtered and ordered, and returns one page of it
    /// together with the total number of rows.
    ///
    /// How it works: an IQueryable is only a description of a SQL query. Nothing has been sent to
    /// the database yet when this method is called. This method sends two queries:
    ///   1. CountAsync  → SELECT COUNT(*) over the whole filtered list, so the client can show "of 134".
    ///   2. Skip + Take → SELECT ... ORDER BY ... OFFSET x ROWS FETCH NEXT y ROWS ONLY, which returns only
    ///      the rows for the asked page. The other rows are never loaded into memory.
    ///
    /// Rules for the caller:
    ///   - Call this last, after every Where (filter) and OrderBy (sort). Anything added after it has no effect.
    ///   - The query must be ordered, and the order must be unique (for example OrderBy Name, ThenBy Id).
    ///     Without an order, SQL Server may return the rows in a different order for each request, so the
    ///     same user could appear on page 1 and again on page 2, while another user is never shown.
    ///   - page is 1-based: page 1 is the first page. pageSize is the number of rows per page.
    ///     Both are validated earlier by PagedQuery ([Range] attributes), so they are never 0 or negative here.
    /// </summary>
    /// <typeparam name="T">The entity type of the query, for example ApplicationUser.</typeparam>
    /// <param name="query">The filtered and ordered query to page.</param>
    /// <param name="page">The 1-based page number to return.</param>
    /// <param name="pageSize">How many rows one page holds.</param>
    /// <returns>The rows of that page plus the paging metadata (page, page size, total count).</returns>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize)
    {
        // Query 1: how many rows match the filters in total. Needed for "Page 2 of 7" and "134 users".
        // This runs the COUNT in the database; it does not load any rows.
        var totalCount = await query.CountAsync();

        // Query 2: the rows of the asked page only.
        // Skip jumps over the rows of the earlier pages. Page 1 skips 0 rows, page 2 skips one page, and so on.
        // Take limits the result to one page. EF Core translates Skip/Take into OFFSET/FETCH in SQL Server.
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Pack the rows and the numbers into one object. PagedResult calculates TotalPages,
        // HasPrevious and HasNext from these values, so they are never stored twice.
        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
