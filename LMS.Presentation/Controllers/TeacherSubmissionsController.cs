using LMS.Shared.Constants;
using LMS.Shared.DTOs.SubmissionDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;


namespace LMS.Presentation.Controllers;

/// <summary>
/// The teacher's overview of the hand-ins for one assignment (US10). Teachers only.
/// The controller only handles HTTP, all rules live in SubmissionService.
/// </summary>
[Route("api/activitie/{activityId:guid}/submissions")]
[ApiController]
[Authorize(Roles = UserRoles.Teacher)]
public class TeacherSubmissionsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [SwaggerOperation(Summary = "Get the submission overview for an assignment",
        Description = "One row per student in the assignment's course, " +
        "including students who have not handed in.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubmissionOverviewDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmissionOverviewDto>> GetOverview(Guid activityId)
    {
        var overview = await _serviceManager.SubmissionService.GetOverviewAsync(activityId);
        return Ok(overview);
    }
}
