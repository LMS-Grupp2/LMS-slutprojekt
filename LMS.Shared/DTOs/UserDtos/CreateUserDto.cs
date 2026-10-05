using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.UserDtos;

public record CreateUserDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; init; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = null!;

    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Password { get; init; } = null!;

    [Required]
    public string Role { get; init; } = null!;

    public Guid? CourseId { get; init; }
}
