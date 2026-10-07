using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class CourseRepository(ApplicationDbContext context) : ICourseRepository
{
    public async Task CreateCourseAsync(Course course)
    {
        context.Add(course);
        //await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await context.Courses.ToListAsync();
    }

    public async Task<Course?> GetCourseById(Guid Id)
    {
        return await context.Courses.FirstOrDefaultAsync(c => c.Id == Id);
    }

    public async Task<Course?> GetCourseForUserAsync(string userId) =>
        await context.Courses
            .AsNoTracking()
            .Include(c => c.CourseUsers)
                .ThenInclude(cu => cu.User)
            .FirstOrDefaultAsync(c => c.CourseUsers.Any(cu => cu.UserId == userId));

    public Task<bool> UpdateCourse(Course course)
    {


        throw new NotImplementedException();
    }
}