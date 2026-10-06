using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface ICourseUserRepository
{
    void Create(CourseUser courseUser);
    Task<IEnumerable<CourseUser>> GetByUserIdAsync(string userId);
}
