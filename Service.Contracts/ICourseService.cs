using LMS.Shared.DTOs.CourseDtos;

namespace Service.Contracts;

public interface ICourseService
{
    Task<CourseDto?> GetCourseById(Guid id);

    Task <IEnumerable<CourseDto>> GetCourses();

    Task<CourseDto> CreateCourse(CreateCourseDto createCourseDto);

    Task<bool> UpdateCourse(Guid id, UpdateCourseDto updateCourseDto);
}
