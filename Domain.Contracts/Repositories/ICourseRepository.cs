using Domain.Models.Entities;

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

    /// <summary>Gets all courses.</summary>
    Task<IEnumerable<Course>> GetAllCoursesAsync();
}