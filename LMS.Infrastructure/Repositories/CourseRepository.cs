using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class CourseRepository(ApplicationDbContext context) : ICourseRepository
{
    public async Task CreateCourseAsync(Course course)
    {
        await context.Courses.AddAsync(course);
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await context.Courses.ToListAsync();
    }

    public async Task<Course?> GetCourseByIdAsync(Guid Id)
    {
        return await context.Courses.FirstOrDefaultAsync(c => c.Id == Id);
    }

    public async Task<Course?> GetCourseForUserAsync(string userId) =>
        await context.Courses
            .AsNoTracking()
            .Include(c => c.CourseUsers)
                .ThenInclude(cu => cu.User)
            .FirstOrDefaultAsync(c => c.CourseUsers.Any(cu => cu.UserId == userId));

    public async Task<bool> UpdateCourse(Course course)
    {
        context.Courses.Update(course);
        var changed = await context.SaveChangesAsync();
        return changed > 0;
    }
}