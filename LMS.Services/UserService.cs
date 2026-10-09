using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.UserDtos;
using Service.Contracts;
using LMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;


namespace LMS.Services;

/// <summary>
/// Business rules for users: role must be assignable and a student needs a course (400),
/// email must be unique (409), and a student belongs to only one course.
/// Controllers call this service, and it talks to the database only through the unit of work.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        ValidateRoleAndCourse(dto.Role, dto.CourseId);

        if (await _unitOfWork.Users.FindByEmailAsync(dto.Email) is not null)
            throw new ConflictException($"The email '{dto.Email}' is already in use.", "Email in use.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = dto.Name
        };

        if (dto.CourseId is Guid courseId)
            user.CourseUsers.Add(new CourseUser { CourseId = courseId });

        ThrowIfFailed(await _unitOfWork.Users.CreateAsync(user, dto.Password));
        ThrowIfFailed(await _unitOfWork.Users.AddToRoleAsync(user, dto.Role));

        return await GetByIdAsync(user.Id);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _unitOfWork.Users.GetRolesAsync(user);
            result.Add(MapToDto(user, roles.FirstOrDefault() ?? "Role not assigned"));
        }

        return result;
    }

    public async Task<UserDto> GetByIdAsync(string id)
    {
        var user = await _unitOfWork.Users.FindByIdWithCourseAsync(id)
            ?? throw new UserNotFoundException(id);

        var roles = await _unitOfWork.Users.GetRolesAsync(user);

        return MapToDto(user, roles.FirstOrDefault() ?? "Role not assigned");
    }

    public async Task UpdateAsync(string id, UpdateUserDto dto)
    {
        ValidateRoleAndCourse(dto.Role, dto.CourseId);

        var user = await _unitOfWork.Users.FindByIdWithCourseAsync(id)
            ?? throw new UserNotFoundException(id);

        var emailOwner = await _unitOfWork.Users.FindByEmailAsync(dto.Email);
        if (emailOwner is not null && emailOwner.Id != user.Id)
            throw new ConflictException($"The email '{dto.Email}' is already in use.");

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.UserName = dto.Email;

        UpdateCourseLink(user, dto.Role, dto.CourseId);

        ThrowIfFailed(await _unitOfWork.Users.UpdateAsync(user));

        await ChangeRoleAsync(user, dto.Role);
        
    }



    /* Helper methods */

    /// <summary>
    /// Single place that maps a user to its DTO. The role is passed in because Identity keeps roles
    /// in a separate table (fetched with GetRolesAsync), not on ApplicationUser.
    /// Requires CourseUsers.Course to be loaded.
    /// </summary>
    private static UserDto MapToDto(ApplicationUser user, string role)
    {
        return new UserDto(
            user.Id,
            user.Name,
            user.Email!,
            role,
            user.CourseUsers.Select(cu => cu.Course.Name).ToArray()
        );
    }

    /// <summary>
    /// Throws a 400 if the role isn't assignable (e.g. "Demo") or a student has no course.
    /// Runs first in create/update, before any database call.
    /// </summary>
    private static void ValidateRoleAndCourse(string role, Guid? courseId)
    {
        if (!UserRoles.Assignable.Contains(role))
            throw new BadRequestException(
                $"'{role}' is not a valid one. Please choose between: {string.Join(", ", UserRoles.Assignable)}.",
                "Invalid role"
            );

        if (role == UserRoles.Student && courseId is null)
            throw new BadRequestException("A student must have a course assigned.", "Course is required");
    }

    /// <summary>
    /// Identity returns a failed result instead of throwing, so it's easy to ignore by mistake.
    /// This turns it into a 400 with Identity's own messages (e.g. password rules).
    /// </summary>
    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)),
                "Validation failed");
    }

    /// <summary>
    /// Links the user to the given course. A student can only belong to one course,
    /// so their old link is removed first. A teacher keeps their old courses and gets one more.
    /// Does nothing if no course is given or the user is already linked to it.
    /// </summary>
    private static void UpdateCourseLink(ApplicationUser user, string role, Guid? courseId)
    {
        if (courseId is not Guid newCourseId) return;
        if (user.CourseUsers.Any(cu => cu.CourseId == newCourseId)) return;

        if (role == UserRoles.Student)
        {
            foreach (var link in user.CourseUsers.ToList())
                user.CourseUsers.Remove(link);
        }

        user.CourseUsers.Add(new CourseUser { CourseId = newCourseId });
    }

    /// <summary>
    /// Swaps the user's role: removes every old role except the new one, then adds the new one
    /// if it's missing. Runs after the user is saved, because Identity's role methods need an existing user.
    /// </summary>
    private async Task ChangeRoleAsync(ApplicationUser user, string newRole)
    {
        var currentRoles = await _unitOfWork.Users.GetRolesAsync(user);

        foreach (var oldRole in currentRoles.Where(r => r != newRole))
            ThrowIfFailed(await _unitOfWork.Users.RemoveFromRoleAsync(user, oldRole));

        if (!currentRoles.Contains(newRole))
            ThrowIfFailed(await _unitOfWork.Users.AddToRoleAsync(user, newRole));
    }
}
