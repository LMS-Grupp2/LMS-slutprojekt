using LMS.Shared.Constants;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

/// <summary>
/// Course endpoints for teachers. The controller only handles HTTP,
/// all rules live in CourseService.
/// </summary>
// Plural route, same style as api/modules and api/users.
[Route("api/courses")]
[ApiController]
// Only teachers can list, create and edit courses.
// Students get their own course through the student/course endpoint.
[Authorize(Roles = UserRoles.Teacher)]
public class CourseController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    /// <summary>GET api/courses - lists all courses.</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all courses", Description = "Returns all courses, earliest start date first.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CourseDto>))]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        var courses = await _serviceManager.CourseService.GetCoursesAsync();

        return Ok(courses);
    }

    /// <summary>GET api/courses/{id} - returns one course, or 404 if it does not exist.</summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get course by id", Description = "Gets an existing course by ID.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto?>> GetCourse([FromRoute] Guid id)
    {
        var courseDto = await _serviceManager.CourseService.GetCourseByIdAsync(id);

        return Ok(courseDto);
    }

    /// <summary>POST api/courses - creates a course. Returns 400 for invalid data.</summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create course", Description = "Creates a new course.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CourseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto dto)
    {
        var courseDto = await _serviceManager.CourseService.CreateCourseAsync(dto);

        // 201 Created with a Location header that points to GET api/courses/{id}.
        return CreatedAtAction(nameof(GetCourse), new { id = courseDto.Id }, courseDto);
    }

    /// <summary>PUT api/courses/{id} - updates a course. The id comes from the route only.</summary>
    [HttpPut("{id:guid}")]
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