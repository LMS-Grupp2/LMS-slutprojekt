using System.Security.Claims;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.SubmissionDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

/// <summary>
/// A student's own hand-in for one assignment (US09). Students only; the teacher's overview is US10.
/// The controller only handles HTTP, all rules live in SubmissionService.
/// </summary>
[Route("api/student/activities/{activityId:guid}/submission")]
[ApiController]
[Authorize(Roles = UserRoles.Student)]
public class StudentSubmissionsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get my submission for an assignment",
        Description = "Returns the deadline, the files handed in so far and the calculated status. " +
                      "A student can only read assignments in their own course.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudentSubmissionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentSubmissionDto>> GetMySubmission(Guid activityId)
    {
        // The student id comes from the login token, never from the request
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var submission = await _serviceManager.SubmissionService.GetForStudentAsync(activityId, userId);
        return Ok(submission);
    }
}
