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

    /// <summary>
    /// Gets assignments for the specified student, including related ActivityType, Module, Course, and the student's
    /// submissions with feedback, ordered by end time.
    /// </summary>
    /// <remarks>Includes ActivityType, Module, and Course, and only the specified student's submissions with
    /// feedback. Filters to activities of type 'Assignment' where the student is associated with the course, orders
    /// results by EndTime, and queries with AsNoTracking.</remarks>
    /// <param name="studentId">The identifier of the student whose assignments to retrieve.</param>
    /// <returns>A task result contains an IEnumerable<Activity> of assignment activities for the specified student.</returns>
    public async Task<IEnumerable<Activity>> GetAssignmentsForStudentAsync(string studentId)
    {
        var assignments = await _context.Activities
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Include(a => a.Module)
                .ThenInclude(m => m.Course)
            .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
                .ThenInclude(s => s.Feedbacks)
            .Where(a => a.ActivityType.Name == "Assignment"
                        && a.Module.Course.CourseUsers.Any(cu => cu.UserId == studentId))
            .OrderBy(a => a.EndTime)
            .ToListAsync();

        return assignments;
    }
}
