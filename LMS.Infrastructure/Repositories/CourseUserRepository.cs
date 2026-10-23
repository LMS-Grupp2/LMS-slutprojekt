using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class CourseUserRepository : ICourseUserRepository
{
    private readonly ApplicationDbContext _context;

    public CourseUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Create(CourseUser courseUser)
    {
        _context.CourseUsers.Add(courseUser);
    }

    public async Task<IEnumerable<CourseUser>> GetByUserIdAsync(string userId)
    {
        return await _context.CourseUsers
            .Where(cu => cu.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseUser>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.CourseUsers
            .AsNoTracking()
            .Include(cu => cu.User)
            .Where(cu => cu.CourseId == courseId)
            .ToListAsync();
    }

    // One query for the members of several courses (instead of one query per course)
    public async Task<IEnumerable<CourseUser>> GetByCourseIdsAsync(IEnumerable<Guid> courseIds)
    {
        // Materialised first, so EF Core can translate Contains to an SQL IN (...)
        var ids = courseIds.ToList();

        return await _context.CourseUsers
            .AsNoTracking()
            .Include(cu => cu.User)
            .Where(cu => ids.Contains(cu.CourseId))
            .ToListAsync();
    }
}