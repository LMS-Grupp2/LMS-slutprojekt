using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Extensions;
using LMS.Shared.Paging;
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

    /// <summary>
    /// Builds one SQL query from the optional filters, then pages it (US15).
    /// Nothing runs until ToPagedResultAsync: Where/OrderBy/Include only describe the query.
    /// Read only, so tracking is turned off.
    /// </summary>
    public async Task<PagedResult<ApplicationUser>> GetPageAsync(UserListQuery query)
    {
        var users = _userManager.Users
            .AsNoTracking()
            .Include(u => u.CourseUsers)
            .ThenInclude(cu => cu.Course)
            .AsQueryable();

        // Search: name OR email contains the text. EF Core turns Contains into LIKE '%text%',
        // which is case-insensitive with SQL Server's default collation.
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            users = users.Where(u => u.Name.Contains(search) || u.Email!.Contains(search));
        }

        // Role: Identity keeps roles in AspNetUserRoles + AspNetRoles, not on the user row.
        // A sub-query (EXISTS) filters in the database instead of loading everyone.
        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            users = users.Where(u => _context.UserRoles.Any(ur =>
                ur.UserId == u.Id &&
                _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == query.Role)));
        }

        if (query.CourseId is Guid courseId)
        {
            users = users.Where(u => u.CourseUsers.Any(cu => cu.CourseId == courseId));
        }

        // A total order is required for paging: two users with the same name are tie-broken by id,
        // so a row can never show up on two pages or fall between them.
        return await users
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .ToPagedResultAsync(query.Page, query.PageSize);
    }

    public async Task<ApplicationUser?> FindByIdWithCourseAsync(string id)
    {
        return await _userManager.Users
            .Include(u => u.CourseUsers)
            .ThenInclude(cu => cu.Course)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
