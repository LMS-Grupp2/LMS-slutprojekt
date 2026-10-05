using Microsoft.AspNetCore.Identity;

namespace Domain.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = null!;
    public DateTime? LoggedIn { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpireTime { get; set; }

    public ICollection<CourseUser> CourseUsers { get; set; } = [];

}
