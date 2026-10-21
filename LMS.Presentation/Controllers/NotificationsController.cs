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

[Route("api/notifications")]
[ApiController]
[Authorize(Roles = UserRoles.Student)]
public class NotificationsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;
    private const int maxItems = 10;  // Max and default number of notification

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get users notifications",
        Description = "Returns the logged-in student's notifications. Latest first")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignments found", typeof(IEnumerable<StudentAssignmentDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(int nbrItems = maxItems)
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
    [HttpPost("api/notifications/{notificationId:guid}")]
    [SwaggerOperation(
        Summary = "Update notification - only change of IsRead is supported right now",
        Description = "Updates notification")]
    [SwaggerResponse(StatusCodes.Status200OK, "Notification has been updated")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not logged in")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad notificationId")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Unknown notification")]
    public async Task<ActionResult<bool>> NotificationIsRead([FromRoute]Guid notificationId, UpdateNotificationDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        if (notificationId != dto.Id)
            throw new BadRequestException("Id in URI is not the the same as in body");

        await _serviceManager.NotificationService.UpdateNotification(notificationId, dto);

        return Ok();
    }
}
