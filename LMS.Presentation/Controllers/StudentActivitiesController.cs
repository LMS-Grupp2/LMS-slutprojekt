using System.Security.Claims;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.ActivityDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

/// <summary>
/// Read-only activity endpoint for students (#177). The teacher endpoints stay in
/// ActivitiesController. The controller only handles HTTP, the course check lives in ActivityService.
/// </summary>
[Route("api/student/modules/{moduleId:guid}/activities")]
[ApiController]
[Authorize(Roles = UserRoles.Student)]
public class StudentActivitiesController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    /// <summary>GET api/student/modules/{moduleId}/activities - the module's activities, earliest first.
    /// 404 if the module does not exist or belongs to a course the student is not in.</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "List the activities of a module (student)",
        Description = "Returns the module's activities, earliest start time first. A student can only read " +
                      "the activities of modules in their own course.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Activities retrieved successfully", typeof(IEnumerable<ActivityDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Logged in, but not a student")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found (or in a course the student does not belong to)")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivities(Guid moduleId)
    {
        // The service needs to know who asks, so students are limited to their own course.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var activities = await _serviceManager.ActivityService.GetForStudentAsync(moduleId, userId);
        return Ok(activities);
    }
}