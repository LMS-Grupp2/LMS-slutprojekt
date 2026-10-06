using LMS.Shared.DTOs.CourseDtos;

namespace Service.Contracts;

public interface IStudentCourseService
{
    Task<StudentCourseDto> GetMyCourseAsync(string userId);
}
