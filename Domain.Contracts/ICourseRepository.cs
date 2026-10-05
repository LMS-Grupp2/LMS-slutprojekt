using Domain.Models.Entities;

namespace Domain.Contracts;

public interface ICourseRepository
{
    Task<Course?> GetCourseForUserAsync(string userId);
}