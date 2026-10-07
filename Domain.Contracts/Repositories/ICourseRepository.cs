using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface ICourseRepository
{
    Task CreateCourseAsync(Course course);

    Task<Course?> GetCourseByIdAsync(Guid Id);

    Task<Course?> GetCourseForUserAsync(string userId);

    Task<IEnumerable<Course>> GetAllCoursesAsync();

    Task<bool> UpdateCourse(Course course);
}