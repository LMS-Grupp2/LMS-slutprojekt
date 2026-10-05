using Domain.Contracts;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.CourseDtos;
using Service.Contracts;

namespace LMS.Services;

public class StudentCourseService(IUnitOfWork unitOfWork) : IStudentCourseService
{
    public async Task<StudentCourseDto> GetMyCourseAsync(string userId)
    {
        var course = await unitOfWork.Courses.GetCourseForUserAsync(userId)
            ?? throw new CourseNotFoundException();

        var participants = course.CourseUsers
            .Select(cu => new CourseParticipantDto(cu.User.Name))
            .OrderBy(p => p.Name)
            .ToList();

        return new StudentCourseDto(
            course.Id,
            course.Name,
            course.Description,
            course.StartDate,
            course.EndDate,
            participants);
    }
}
