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

    [HttpPost]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Hand in a file to an assignment",
        Description = "Uploads one file (multipart/form-data, field 'File'). The first file creates the " +
                      "submission, later files are added to it. The server sets the hand-in time.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(StudentSubmissionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentSubmissionDto>> SubmitFile(
        Guid activityId,
        [FromForm] SubmitFileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        if (request.File is null)
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "No file was uploaded." });

        // Disposed here, the service has finished copying it to disk by then
        await using var stream = request.File.OpenReadStream();

        var submission = await _serviceManager.SubmissionService.SubmitAsync(
            activityId,
            userId,
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            cancellationToken);

        // Location points at the GET above, which returns the same view of the submission
        return CreatedAtAction(nameof(GetMySubmission), new { activityId }, submission);
    }
}

/// <summary>
/// The form field of the hand-in request. Lives here, not in LMS.Shared, because IFormFile is an ASP.NET type.
/// </summary>
public class SubmitFileRequest
{
    public IFormFile? File { get; set; }
}
