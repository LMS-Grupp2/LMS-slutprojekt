using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.UserDtos;
using Service.Contracts;


namespace LMS.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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

    public Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        throw new NotImplementedException();
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

    public Task<UserDto> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(string id, UpdateUserDto dto)
    {
        throw new NotImplementedException();
    }
}
