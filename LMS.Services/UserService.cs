using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.UserDtos;
using Service.Contracts;
using LMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;


namespace LMS.Services;

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

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)),
                "Validation failed");
    }

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

    private async Task ChangeRoleAsync(ApplicationUser user, string newRole)
    {
        var currentRoles = await _unitOfWork.Users.GetRolesAsync(user);

        foreach (var oldRole in currentRoles.Where(r => r != newRole))
            ThrowIfFailed(await _unitOfWork.Users.RemoveFromRoleAsync(user, oldRole));

        if (!currentRoles.Contains(newRole))
            ThrowIfFailed(await _unitOfWork.Users.AddToRoleAsync(user, newRole));
    }
}
