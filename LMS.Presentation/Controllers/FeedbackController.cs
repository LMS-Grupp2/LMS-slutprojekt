using System.Security.Claims;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.FeedbackDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;


namespace LMS.Presentation.Controllers;

/// <summary>
/// Feedback on one submission (US11). Teachers give feedback, the student reads it on their own submission.
/// The controller only handles HTTP, all rules live in FeedbackService.
/// </summary>
[Route("api/submissions/{submissionId:guid}/feedback")]
[ApiController]
[Authorize(Roles = UserRoles.Teacher + "," + UserRoles.Student)]
public class FeedbackController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [SwaggerOperation(Summary = "Get the feedback on a submission",
        Description = "Newest first. A student only gets feedback on their own submission")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FeedbackDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedback(Guid submissionId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var feedback = await _serviceManager.FeedbackService
            .GetBySubmissionIdAsync(submissionId, userId, User.IsInRole(UserRoles.Teacher));

        return Ok(feedback);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Teacher)] //will narrow the class rule to teach only for this point
    [SwaggerOperation(Summary = "Give feedback on a submission",
        Description = "Saves the comment with time and teacher, and sets Approved or Needs changes.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FeedbackDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackDto>> GiveFeedback(Guid submissionId, CreateFeedbackDto dto)
    {
        if (!TryGetUserId(out var teacherId))
            return Unauthorized();

        var feedback = await _serviceManager.FeedbackService
            .GiveFeedbackAsync(submissionId, teacherId, dto);

        return StatusCode(StatusCodes.Status201Created, feedback);
    }

    /// <summary>
    /// Reads the logged-in user's id from the JWT (NameIdentifier claim).
    /// Returns false if the claim is missing, so the endpoint can answer 401 instead of crashing.
    /// </summary>
    private bool TryGetUserId(out string userId)
    {
        userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        return userId.Length > 0;
    }
}
