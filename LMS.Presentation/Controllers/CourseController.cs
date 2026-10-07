using LMS.Shared.Constants;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/course")]
[ApiController]
[Authorize(Roles = UserRoles.Teacher)]
public class CourseController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [SwaggerOperation(Summary = "Get all course - without models etc.", Description = "Get all course.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        var courses = await _serviceManager.CourseService.GetCourses();

        return Ok(courses);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get course by id", Description = "Get an existing course by ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<ActionResult<CourseDto?>> GetCourse([FromRoute] Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("No id present in URI.");
        }
        var courseDto = _serviceManager.CourseService.GetCourseById(id);

        if (courseDto != null) {
            return Ok(courseDto);
        }
        else
        {
            return NotFound();
        }
    }


    [HttpPost]
    [SwaggerOperation(Summary = "Create course", Description = "Creates a new course.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CourseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto createCourseDto)
    {
        var courseDto = await _serviceManager.CourseService.CreateCourse(createCourseDto);

        return CreatedAtAction(nameof(GetCourse), new { id = courseDto.Id }, courseDto);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update course", Description = "Updates an existing course by ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateCourse([FromRoute] Guid id, UpdateCourseDto course)
    {
        if (id == Guid.Empty && id == course.Id)
        {
            return BadRequest("No or bad id present in URI.");
        }

        bool success = await _serviceManager.CourseService.UpdateCourse(id, course);

        return NoContent();
    }
}
