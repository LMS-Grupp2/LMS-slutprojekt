using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.CourseDtos;
using Service.Contracts;

namespace LMS.Services;

public class CourseService(IUnitOfWork unitOfWork) : ICourseService
{
    public async Task<CourseDto> CreateCourse(CreateCourseDto createCourseDto)
    {
        if (createCourseDto.StartDate > createCourseDto.EndDate)
        {
            throw new BadRequestException("Start date can not be after end date");
        }

        var course = new Course
        {
            Name = createCourseDto.Name.Trim(),
            Description = createCourseDto.Description?.Trim(),
            StartDate = createCourseDto.StartDate,
            EndDate = createCourseDto.EndDate
        };

        await unitOfWork.Courses.CreateCourseAsync(course);
        await unitOfWork.SaveChangesAsync();

        var dto = new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            StartDate = course.StartDate,
            EndDate = course.EndDate
        };

        return dto;
    }

    public async Task<IEnumerable<CourseDto>> GetCourses()
    {
        var courses = unitOfWork.Courses.GetAllCoursesAsync();

        var courseList = new List<CourseDto>();

        if (courses != null)
        {   
            foreach (var item in courseList)
            {
                courseList.Add(
                   new CourseDto
                   {
                       Id = item.Id,
                       Name = item.Name,
                       Description = item.Description,
                       StartDate = item.StartDate,
                       EndDate = item.EndDate

                       // TODO - add collections to GetCourses()
                       // ICollection<Modules> Modules { get; init; } = [];
                       // ICollection<CourseUsers> CourseUsers { get; init; } = [];
                       // ICollection<Documents> Documents { get; set; } = [];
                   });
            }
        }

        return courseList;
    }

    public async Task<bool> UpdateCourse(UpdateCourseDto updateCourseDto)
    {
        if (updateCourseDto.StartDate > updateCourseDto.EndDate)
        {
            throw new BadRequestException("Failed to create new course");
        }

        var course = new Course
        {
            Name = updateCourseDto.Name.Trim(),
            Description = updateCourseDto.Description?.Trim(),
            StartDate = updateCourseDto.StartDate,
            EndDate = updateCourseDto.EndDate
        };

        await unitOfWork.Courses.UpdateCourse(course);
        await unitOfWork.CompleteAsync();

        return true;
    }
}
