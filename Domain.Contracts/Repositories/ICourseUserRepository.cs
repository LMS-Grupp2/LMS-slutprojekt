using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface ICourseUserRepository
{
    void Create(CourseUser courseUser);
    Task<IEnumerable<CourseUser>> GetByUserIdAsync(string userId);

    Task<IEnumerable<CourseUser>> GetByCourseIdAsync(Guid courseId);

    /// <summary>
    /// Gets the members (with their user) of several courses in one query, read only.
    /// Used by the teacher dashboard so it does not need one query per course.
    /// </summary>
    Task<IEnumerable<CourseUser>> GetByCourseIdsAsync(IEnumerable<Guid> courseIds);
}