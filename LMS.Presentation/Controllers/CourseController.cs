using LMS.Shared.Constants;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Route("api/course")]
[ApiController]
[Authorize(Roles = UserRoles.Teacher)]
[Authorize(Roles = UserRoles.Student)]
public class CourseController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        var courses = await _serviceManager.CourseService.GetCourses();

        return Ok(courses);
    }




    public async Task<ActionResult<CourseDto>> UpdateCourse(UpdateCourseDto course)
    {
        bool success = await _serviceManager.CourseService.UpdateCourse(course);

        return (success ? Ok() : BadRequest(course));
    }

}
