using LMS.Shared.DTOs.CourseDtos;
using System.Runtime.CompilerServices;

namespace Service.Contracts;

public interface ICourseService
{
    Task<CourseDto?> GetCourseByIdAsync(Guid id);

    Task <IEnumerable<CourseDto>> GetCoursesAsync();

    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto);

    Task UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto);
}
