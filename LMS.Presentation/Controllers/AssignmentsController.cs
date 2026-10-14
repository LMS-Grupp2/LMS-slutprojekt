using LMS.Shared.DTOs;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;


[Route("api/student/assignments")]
[ApiController]
[Authorize(Roles = "Student")]
public class AssignmentsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get my assignments",
        Description = "Returns the logged-in student's assignments an their status.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignments found", typeof(StudentCourseDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    public async Task<ActionResult<IEnumerable<StudentAssignmentDto>>> GetMyAssignments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var assignments = await _serviceManager.AssignmentService.GetAssignmentsForStudentAsync(userId);

        return Ok(assignments);
    }
}
