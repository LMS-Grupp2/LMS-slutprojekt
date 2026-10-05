using LMS.Shared.DTOs.UserDtos;

namespace Service.Contracts;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto> GetByIdAsync(string id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task UpdateAsync(string id, UpdateUserDto dto);
}
