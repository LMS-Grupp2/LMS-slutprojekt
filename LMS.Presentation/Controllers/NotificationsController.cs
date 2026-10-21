using LMS.Shared.Constants;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.NotificationDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

// GetLatestNotifications(int quantity)  // Max 10 ???
// NotificationIsRead(bool all = false)

[Route("api/notifications")]
[ApiController]
[Authorize(Roles = UserRoles.Student)]
public class NotificationsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get users notifications",
        Description = "Returns the logged-in student's notifications. Latest first")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignments found", typeof(IEnumerable<StudentAssignmentDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        // TODO - add max numbers
        var notifictions = await _serviceManager.NotificationService.GetByStudentIdAsync(userId);

        return Ok(notifictions);
    }
}
