using LMS.Shared.Constants;
using LMS.Shared.DTOs.ModuleDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api")]
[ApiController]
[Authorize(Roles = UserRoles.Teacher)]
[Consumes("application/json")]
[Produces("application/json")]
public class ModulesController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet("courses/{courseId:guid}/modules")]
    [SwaggerOperation(
        Summary = "List modules of a course",
        Description = "Returns the modules of the course, ordered by start date.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Modules retrieved successfully", typeof(IEnumerable<ModuleDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Course not found")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetModules(Guid courseId) =>
        Ok(await _serviceManager.ModuleService.GetByCourseIdAsync(courseId));

    [HttpGet("modules/{id:guid}")]
    [SwaggerOperation(
        Summary = "Get module",
        Description = "Returns a single module by id.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Module retrieved successfully", typeof(ModuleDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    public async Task<ActionResult<ModuleDto>> GetModule(Guid id) =>
        Ok(await _serviceManager.ModuleService.GetByIdAsync(id));

    [HttpPost("courses/{courseId:guid}/modules")]
    [SwaggerOperation(
        Summary = "Create module",
        Description = "Creates a module in the course. Dates must lie inside the course dates.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Module created successfully", typeof(ModuleDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or dates outside the course")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Course not found")]
    public async Task<ActionResult<ModuleDto>> CreateModule(Guid courseId, CreateModuleDto dto)
    {
        var created = await _serviceManager.ModuleService.CreateAsync(courseId, dto);
        return CreatedAtAction(nameof(GetModule), new { id = created.Id }, created);
    }

    [HttpPut("modules/{id:guid}")]
    [SwaggerOperation(
        Summary = "Update module",
        Description = "Updates name, description and dates of a module. Dates must lie inside the course dates.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Module updated successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or dates outside the course")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    public async Task<IActionResult> UpdateModule(Guid id, UpdateModuleDto dto)
    {
        await _serviceManager.ModuleService.UpdateAsync(id, dto);
        return NoContent();
    }
}
