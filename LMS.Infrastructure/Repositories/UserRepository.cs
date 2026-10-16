using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<ApplicationUser?> FindByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IEnumerable<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.RemoveFromRoleAsync(user, role);
    }

    /// <summary>
    /// Returns every user's role name in one query, as userId → role name.
    /// Each user has one role, so the first role per user is used.
    /// </summary>
    public async Task<IDictionary<string, string>> GetRoleNamesByUserIdAsync()
    {
        var pairs = await _context.UserRoles
            .Join(_context.Roles,
                 userRole => userRole.RoleId,
                 role => role.Id,
                 (userRole, role) => new { userRole.UserId, RoleName = role.Name ?? "" })
            .ToListAsync();

        return pairs
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.First().RoleName);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _userManager.Users
            .Include(u => u.CourseUsers)
            .ThenInclude(cu => cu.Course)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> FindByIdWithCourseAsync(string id)
    {
        return await _userManager.Users
            .Include(u => u.CourseUsers)
            .ThenInclude(cu => cu.Course)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
