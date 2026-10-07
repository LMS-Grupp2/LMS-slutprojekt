using LMS.Shared.DTOs.CourseDtos;

namespace Service.Contracts;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetCourses();

    Task<CourseDto> CreateCourse(CreateCourseDto createCourseDto);

    Task<bool> UpdateCourse(UpdateCourseDto updateCourseDto);
}
