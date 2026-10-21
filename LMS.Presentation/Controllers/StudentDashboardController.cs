using System.Security.Claims;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

/// <summary>
/// The student's dashboard (US12). The controller only handles HTTP: it reads the student's id from the token
/// and asks DashboardService. The course is never taken from the request, so a student can only ever
/// see the dashboard of their own course.
/// </summary>
[Route("api/student/dashboard")]
[ApiController]
[Authorize(Roles = UserRoles.Student)]
public class StudentDashboardController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    /// <summary>GET api/student/dashboard - the signed-in student's overview.</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get my dashboard",
        Description = "Returns the logged-in student's overview: next activities, deadlines, assignments " +
                      "that need action and the latest documents, all from the student's own course.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Dashboard found", typeof(StudentDashboardDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Logged in, but not a student")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The student is not enrolled in a course")]
    public async Task<ActionResult<StudentDashboardDto>> GetMyDashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var dashboard = await _serviceManager.DashboardService.GetForStudentAsync(userId);
        return Ok(dashboard);
    }
}