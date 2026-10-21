using Domain.Models.Exceptions;
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
    private readonly int maxItems = 10;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get users notifications",
        Description = "Returns the logged-in student's notifications. Latest first")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignments found", typeof(IEnumerable<StudentAssignmentDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(int nbrItems)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        if (nbrItems > maxItems)
            throw new BadRequestException($"Max {maxItems} notifications can be requested for.");
        
        var notifictions = await _serviceManager.NotificationService.GetByStudentIdAsync(userId, nbrItems);

        return Ok(notifictions);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Update notification - only IsRead right now",
        Description = "Updates notification")]
    [SwaggerResponse(StatusCodes.Status200OK, "Notification has been updated")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Unknown notification")]
    public async Task<ActionResult<bool>> NotificationIsRead(UpdateNotificationDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        await _serviceManager.NotificationService.UpdateNotification(dto);

        return Ok();
    }
}
