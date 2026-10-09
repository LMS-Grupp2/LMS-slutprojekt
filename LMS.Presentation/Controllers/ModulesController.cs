using System.Security.Claims;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.ModuleDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

/// <summary>
/// Module endpoints. The controller only handles HTTP, all rules live in ModuleService.
/// </summary>
[Route("api")]
[ApiController]
// Both roles can reach the controller. The endpoints below that only teachers may use
// have their own [Authorize(Roles = Teacher)], which is checked in addition to this one.
[Authorize(Roles = UserRoles.Teacher + "," + UserRoles.Student)]
[Consumes("application/json")]
[Produces("application/json")]
public class ModulesController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    /// <summary>GET api/courses/{courseId}/modules - teachers: any course, students: only their own.</summary>
    [HttpGet("courses/{courseId:guid}/modules")]
    [SwaggerOperation(
        Summary = "List modules of a course",
        Description = "Returns the modules of the course, ordered by start date. A student can only list the modules of their own course.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Modules retrieved successfully", typeof(IEnumerable<ModuleDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Course not found (or a course the student does not belong to)")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetModules(Guid courseId)
    {
        // The service needs to know who asks, so students are limited to their own course.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var modules = await _serviceManager.ModuleService.GetByCourseIdAsync(
            courseId, userId, User.IsInRole(UserRoles.Teacher));

        return Ok(modules);
    }

    /// <summary>GET api/modules/{id} - teachers only.</summary>
    [HttpGet("modules/{id:guid}")]
    [Authorize(Roles = UserRoles.Teacher)]
    [SwaggerOperation(
        Summary = "Get module",
        Description = "Returns a single module by id.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Module retrieved successfully", typeof(ModuleDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    public async Task<ActionResult<ModuleDto>> GetModule(Guid id) =>
        Ok(await _serviceManager.ModuleService.GetByIdAsync(id));

    /// <summary>POST api/courses/{courseId}/modules - teachers only.</summary>
    [HttpPost("courses/{courseId:guid}/modules")]
    [Authorize(Roles = UserRoles.Teacher)]
    [SwaggerOperation(
        Summary = "Create module",
        Description = "Creates a module in the course. Dates must lie inside the course dates and must not overlap another module.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Module created successfully", typeof(ModuleDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or dates outside the course")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Course not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "The module overlaps another module in the course")]
    public async Task<ActionResult<ModuleDto>> CreateModule(Guid courseId, CreateModuleDto dto)
    {
        var created = await _serviceManager.ModuleService.CreateAsync(courseId, dto);
        return CreatedAtAction(nameof(GetModule), new { id = created.Id }, created);
    }

    /// <summary>PUT api/modules/{id} - teachers only.</summary>
    [HttpPut("modules/{id:guid}")]
    [Authorize(Roles = UserRoles.Teacher)]
    [SwaggerOperation(
        Summary = "Update module",
        Description = "Updates name, description and dates of a module. Dates must lie inside the course dates and must not overlap another module.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Module updated successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or dates outside the course")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "The module overlaps another module in the course")]
    public async Task<IActionResult> UpdateModule(Guid id, UpdateModuleDto dto)
    {
        await _serviceManager.ModuleService.UpdateAsync(id, dto);
        return NoContent();
    }
}