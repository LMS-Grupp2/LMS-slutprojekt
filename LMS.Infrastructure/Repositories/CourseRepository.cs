using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
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

    /// <summary>Returns all courses. Read only, so tracking is turned off.</summary>
    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await context.Courses
            .AsNoTracking()
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