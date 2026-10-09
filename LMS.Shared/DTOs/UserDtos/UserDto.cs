namespace LMS.Shared.DTOs.UserDtos;

public sealed record UserDto(
    string Id,
    string Name,
    string Email,
    string Role,
    IReadOnlyCollection<string> CourseNames
);
