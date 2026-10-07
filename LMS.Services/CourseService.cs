using Domain.Contracts;
using LMS.Shared.DTOs.CourseDtos;
using Service.Contracts;

namespace LMS.Services;

public class CourseService(IUnitOfWork unitOfWork) : ICourseService
{
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
}
