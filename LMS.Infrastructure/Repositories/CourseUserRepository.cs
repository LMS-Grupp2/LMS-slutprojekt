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
}
