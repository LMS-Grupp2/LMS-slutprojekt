using LMS.Shared.DTOs.CourseDtos;

namespace Service.Contracts;

/// <summary>Business operations for courses, used by the course controller.</summary>
public interface ICourseService
{
    /// <summary>Gets one course. Throws a 404 (CourseNotFoundException) if it does not exist.</summary>
    Task<CourseDto?> GetCourseByIdAsync(Guid id);

    /// <summary>Gets all courses, earliest start date first.</summary>
    Task<IEnumerable<CourseDto>> GetCoursesAsync();

    /// <summary>Creates a course. Throws a 400 (BadRequestException) if the start date is after the end date.</summary>
    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto);

    /// <summary>Updates a course. Throws a 404 if it does not exist, or a 400 for invalid dates.</summary>
    Task UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto);
}