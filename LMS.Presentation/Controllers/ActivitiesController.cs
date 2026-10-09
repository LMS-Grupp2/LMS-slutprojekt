using LMS.Shared.Constants;
using LMS.Shared.DTOs.ActivityDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;


/// <summary>
/// Activity endpoints. The controller only handles HTTP, all rules live in ActivityService.
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = UserRoles.Teacher)]
public class ActivitiesController(IServiceManager serviceManager) : ControllerBase
{

    private readonly IServiceManager _serviceManager = serviceManager;

    /// <summary>GET api/modules/{moduleId}/activities - the module's activities, earliest first, 
    /// or 404 if the module does not exist.</summary>
    [HttpGet("modules/{moduleId:guid}/activities")]
    [SwaggerOperation(Summary = "List the activities of a module", 
        Description = "Returns the module's activities, earliest start time first")]
    [ProducesResponseType(StatusCodes.Status200OK, Type =  typeof(IEnumerable<ActivityDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivities(Guid moduleId)
    {
        var activities = await _serviceManager.ActivityService.GetByModuleIdAsync(moduleId);
        return Ok(activities);
    }

    /// <summary>GET api/activities/{id} - one activity, or 404 if it does not exist.</summary>
    [HttpGet("activities/{id:guid}")]
    [SwaggerOperation(Summary = "Get activity", Description = "Returns a single activity by id.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActivityDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivityDto>> GetActivity(Guid id)
    {
        var activity = await _serviceManager.ActivityService.GetByIdAsync(id);
        return Ok(activity);
    }

    
    /// <summary>POST api/activities - creates an activity. 400 for invalid data or dates outside 
    /// the module,404 if the module doesn't exist, 409 on overlap.</summary>
    [HttpPost("activities")]
    [SwaggerOperation(Summary = "Create activity", Description = "Creates an activity. Must be inside " +
     "the module's dates and must not overlap another activity in the module.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActivityDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActivityDto>> CreateActivity(CreateActivityDto dto)
    {
        var created = await _serviceManager.ActivityService.CreateAsync(dto);

        return CreatedAtAction(nameof(GetActivity), new { id = created.Id }, created);
    }

    /// <summary>PUT api/activities/{id} - updates an activity. 404 if missing, 400 for invalid data, 409 on overlap.</summary>
    [HttpPut("activities/{id:guid}")]
    [SwaggerOperation(Summary = "Update activity", Description = "Updates an existing activity." )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]         
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateActivity(Guid id, UpdateActivityDto dto)
    {
        await _serviceManager.ActivityService.UpdateAsync(id, dto);
        return NoContent();
    }

    /// <summary>DELETE api/activities/{id} - deletes an activity, or 404 if it does not exist.</summary>
    [HttpDelete("activities/{id:guid}")]
    [SwaggerOperation(Summary = "Delete activity", Description = "Deletes an activity by id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]           
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        await _serviceManager.ActivityService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>GET api/activity-types - all activity types, sorted by name (for the type dropdown).</summary>
    [HttpGet("activity-types")]                                                             
    [SwaggerOperation(Summary = "List activity types", Description = "Returns all activity types, sorted by name.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ActivityTypeDto>))]
    public async Task<ActionResult<IEnumerable<ActivityTypeDto>>> GetActivityTypes()
    {
        var types = await _serviceManager.ActivityService.GetActivityTypesAsync();
        return Ok(types);
    }
}
