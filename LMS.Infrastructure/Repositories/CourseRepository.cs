using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.Constants;
using LMS.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

/// <summary>
/// Database access for courses. The repository never saves changes,
/// saving is done once by the service through the unit of work.
/// </summary>
public class CourseRepository(ApplicationDbContext context) : ICourseRepository
{
    /// <summary>Adds a new course to the context. It is saved later by the unit of work.</summary>
    public async Task CreateCourseAsync(Course course)
    {
        await context.Courses.AddAsync(course);
    }

    /// <summary>
    /// Returns the courses that match the filters, in one SQL query (US15). Read only, so tracking is turned off.
    /// The status is not a column: it is a date comparison, so each status becomes a WHERE on the dates.
    /// </summary>
    public async Task<IEnumerable<Course>> GetAllCoursesAsync(CourseListQuery query, DateOnly today)
    {
        var courses = context.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            courses = courses.Where(c => c.Name.Contains(search));
        }

        courses = query.Status switch
        {
            CourseStatus.Upcoming => courses.Where(c => c.StartDate > today),
            CourseStatus.Active => courses.Where(c => c.StartDate <= today && c.EndDate >= today),
            CourseStatus.Finished => courses.Where(c => c.EndDate < today),
            _ => courses // null or unknown: the service has already rejected unknown values
        };

        return await courses
            .OrderBy(c => c.StartDate)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Returns one course. It is tracked on purpose, so the service can change it
    /// and save without calling Update.
    /// </summary>
    public async Task<Course?> GetCourseByIdAsync(Guid Id)
    {
        return await context.Courses.FirstOrDefaultAsync(c => c.Id == Id);
    }

    /// <summary>Returns the course a student belongs to, including the participants.</summary>
    public async Task<Course?> GetCourseForUserAsync(string userId) =>
        await context.Courses
            .AsNoTracking()
            .Include(c => c.CourseUsers)
                .ThenInclude(cu => cu.User)
            .FirstOrDefaultAsync(c => c.CourseUsers.Any(cu => cu.UserId == userId));
}