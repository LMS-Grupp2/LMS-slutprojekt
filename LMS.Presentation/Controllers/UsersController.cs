using LMS.Shared.DTOs.UserDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

// TODO (US-107): restrict to the Teacher role with [Authorize(Roles = UserRoles.Teacher)].
[Route("api/users")]
[ApiController]
[Authorize]
[Consumes("application/json")]
[Produces("application/json")]
public class UsersController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List users",
        Description = "Returns all users with their role and course names.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Users retrieved successfully", typeof(IEnumerable<UserDto>))]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers() =>
        Ok(await _serviceManager.UserService.GetAllAsync());

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get user",
        Description = "Returns a single user by id.")]
    [SwaggerResponse(StatusCodes.Status200OK, "User retrieved successfully", typeof(UserDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<ActionResult<UserDto>> GetUser(string id) =>
        Ok(await _serviceManager.UserService.GetByIdAsync(id));

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create user",
        Description = "Creates a user with the given name, email, password and role (Student or Teacher). A student must have a course.")]
    [SwaggerResponse(StatusCodes.Status201Created, "User created successfully", typeof(UserDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input, invalid role or missing course")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Email already in use")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        var created = await _serviceManager.UserService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update user",
        Description = "Updates name, email, role and course of an existing user.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "User updated successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input, invalid role or missing course")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Email already in use")]
    public async Task<IActionResult> UpdateUser(string id, UpdateUserDto dto)
    {
        await _serviceManager.UserService.UpdateAsync(id, dto);
        return NoContent();
    }
}