using Domain.Models.Entities;
using LMS.Shared.Paging;

namespace Domain.Contracts.Repositories;

/// <summary>Database operations for courses. Saving is handled by the unit of work.</summary>
public interface ICourseRepository
{
    /// <summary>Adds a new course. It is saved by the unit of work.</summary>
    Task CreateCourseAsync(Course course);

    /// <summary>Gets a tracked course by id, or null if it does not exist.</summary>
    Task<Course?> GetCourseByIdAsync(Guid Id);

    /// <summary>Gets the course a user belongs to, including participants.</summary>
    Task<Course?> GetCourseForUserAsync(string userId);

    /// <summary>
    /// Gets all courses that match the optional filters (name search, status by today's date),
    /// earliest start date first (US15). An empty query returns every course.
    /// </summary>
    Task<IEnumerable<Course>> GetAllCoursesAsync(CourseListQuery query, DateOnly today);
}