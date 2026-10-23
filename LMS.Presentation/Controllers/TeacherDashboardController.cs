using LMS.Shared.Constants;
using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

/// <summary>
/// The teacher's dashboard (US13). Teachers only. The controller only handles HTTP,
/// all rules live in TeacherDashboardService.
/// </summary>
[Route("api/teacher/dashboard")]
[ApiController]
[Authorize(Roles = UserRoles.Teacher)]
public class TeacherDashboardController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    /// <summary>GET api/teacher/dashboard - the teacher's overview of all courses.</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get the teacher dashboard",
        Description = "Returns what needs the teacher's attention: counts of hand-ins awaiting feedback, " +
                      "new, late and missing, the hand-ins to look at first, the next activities " +
                      "and the courses that are running now.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Dashboard found", typeof(TeacherDashboardDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Logged in, but not a teacher")]
    public async Task<ActionResult<TeacherDashboardDto>> GetDashboard()
    {
        var dashboard = await _serviceManager.TeacherDashboardService.GetForTeacherAsync();
        return Ok(dashboard);
    }
}