using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.CourseDtos;
using Service.Contracts;

namespace LMS.Services;

/// <summary>
/// Business rules for courses. Controllers call this service,
/// and it talks to the database only through the unit of work.
/// </summary>
public class CourseService(IUnitOfWork unitOfWork) : ICourseService
{
    /// <summary>Creates a course after validating that the dates make sense.</summary>
    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        // Rule: a course cannot start after it ends (returns 400).
        EnsureValidDates(createCourseDto.StartDate, createCourseDto.EndDate);

        var course = new Course
        {
            Name = createCourseDto.Name.Trim(),
            Description = createCourseDto.Description?.Trim(),
            StartDate = createCourseDto.StartDate,
            EndDate = createCourseDto.EndDate
        };

        await unitOfWork.Courses.CreateCourseAsync(course);

        // The repository only adds the entity. Saving happens once, here.
        await unitOfWork.SaveChangesAsync();

        return ToDto(course);
    }

    /// <summary>Returns all courses, earliest start date first.</summary>
    public async Task<IEnumerable<CourseDto>> GetCoursesAsync()
    {
        var courses = await unitOfWork.Courses.GetAllCoursesAsync();

        // The mapping runs over the loaded courses (not an empty list).
        return courses
            .OrderBy(c => c.StartDate)
            .Select(ToDto)
            .ToList();
    }

    /// <summary>Returns one course, or throws a 404 if it does not exist.</summary>
    public async Task<CourseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await unitOfWork.Courses.GetCourseByIdAsync(id)
            ?? throw new CourseNotFoundException($"Course with id '{id}' was not found.", "Course not found");

        return ToDto(course);
    }

    /// <summary>Updates an existing course, or throws a 404 if it does not exist.</summary>
    public async Task UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto)
    {
        EnsureValidDates(updateCourseDto.StartDate, updateCourseDto.EndDate);

        var course = await unitOfWork.Courses.GetCourseByIdAsync(id)
            ?? throw new CourseNotFoundException($"Course with id '{id}' was not found.", "Course not found");

        // The course is tracked by the context, so changing the properties
        // and saving is enough. No explicit Update call is needed.
        course.Name = updateCourseDto.Name.Trim();
        course.Description = updateCourseDto.Description?.Trim();
        course.StartDate = updateCourseDto.StartDate;
        course.EndDate = updateCourseDto.EndDate;

        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>Throws a 400 (BadRequestException) if the start date is after the end date.</summary>
    private static void EnsureValidDates(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
        {
            throw new BadRequestException("Start date can not be after end date.");
        }
    }

    /// <summary>Single place that maps a Course entity to its DTO.</summary>
    private static CourseDto ToDto(Course course) => new()
    {
        Id = course.Id,
        Name = course.Name,
        Description = course.Description,
        StartDate = course.StartDate,
        EndDate = course.EndDate
    };
}