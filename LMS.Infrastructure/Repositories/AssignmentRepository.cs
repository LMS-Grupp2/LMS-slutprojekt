using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public AssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Activity>> GetAssignmentsForStudentAsync(string studentId)
    {
        var assignments = await _context.Activities
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Include(a => a.Module)
                .ThenInclude(m => m.Course)
            .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
            .Where(a => a.ActivityType.Name == "Assignment"
                        && a.Module.Course.CourseUsers.Any(cu => cu.UserId == studentId))
            .OrderBy(a => a.EndTime)
            .ToListAsync();

        return assignments;
    }
}
