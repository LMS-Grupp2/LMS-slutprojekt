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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CourseDto>))]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        var courses = await _serviceManager.CourseService.GetCoursesAsync();

        return Ok(courses);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get course by id", Description = "Get an existing course by ID.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto?>> GetCourse([FromRoute] Guid id)
    {
        var courseDto = await _serviceManager.CourseService.GetCourseByIdAsync(id);

        return Ok(courseDto);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create course", Description = "Creates a new course.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CourseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto dto)
    {
        var courseDto = await _serviceManager.CourseService.CreateCourseAsync(dto);

        return CreatedAtAction(nameof(GetCourse), new { id = courseDto.Id }, courseDto);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update course", Description = "Updates an existing course by ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateCourse([FromRoute] Guid id, UpdateCourseDto course)
    {
        await _serviceManager.CourseService.UpdateCourseAsync(id, course);

        return NoContent();
    }
}