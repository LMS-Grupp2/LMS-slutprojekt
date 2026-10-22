using LMS.Shared.DTOs.UserDtos;
using LMS.Shared.Paging;

namespace Service.Contracts;

public interface IUserService
{
    /// <summary>One page of users with their role and course names, filtered by the query (US15).</summary>
    Task<PagedResult<UserDto>> GetPageAsync(UserListQuery query);
    Task<UserDto> GetByIdAsync(string id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task UpdateAsync(string id, UpdateUserDto dto);
}
