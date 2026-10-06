using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetCourseForUserAsync(string userId);
}