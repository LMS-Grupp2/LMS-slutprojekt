using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get notification by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Returns notification or default</returns>
    public async Task<Notification?> GetById(Guid id)
    {
        return await _context.Notifications
            .SingleOrDefaultAsync(notification => notification.Id == id);
    }

    /// <summary>
    /// Get students notifications, latest first
    /// </summary>
    /// <param name="studentId"></param>
    /// <returns>Returns list with students notifications, latest first</returns>
    public async Task<IEnumerable<Notification>> GetByStudentId(string studentId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == studentId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Returns a list of students taking a course by courselId
    /// </summary>
    /// <returns>List of student ids</returns>
    public async Task<IEnumerable<string>> GetStudentsIdnByCourseIdAsync(Guid courseId)
    {
        return await _context.CourseUsers
            .AsNoTracking()
            .Where(c => c.CourseId == courseId)
            .Select(u => u.UserId)
            .Distinct()
            .ToListAsync();
    }

    /// <summary>
    /// Returns a list of students taking a course by moduelId
    /// </summary>
    /// <returns>List of student ids</returns>
    public async Task<IEnumerable<string>> GetStudentsIdnByModuleIdAsync(Guid moduleId)
    {
        return await _context.Modules
            .AsNoTracking()
            .Where(module => module.Id == moduleId)
            .SelectMany(module => _context.CourseUsers
                .Where(courseUser => courseUser.CourseId == module.CourseId)
                .Select(courseUser => courseUser.UserId))
            .Distinct()
            .ToListAsync();
    }

    /// <summary>
    /// Returns a list of students taking a course by acivityId
    /// </summary>
    /// <returns>List of student ids</returns>
    public async Task<IEnumerable<string>> GetStudentsIdnByActivityIdAsync(Guid activityId)
    {
        // TODO - Do we need .AsNoTracking() with this syntax?
        return await (
            from activity in _context.Activities.AsNoTracking()
            join module in _context.Modules
                on activity.ModuleId equals module.Id
            join courseUser in _context.CourseUsers
                on module.CourseId equals courseUser.CourseId
            where activity.Id == activityId
            select courseUser.UserId
        )
        .Distinct()
        .ToListAsync();
    }

    public void Add(Notification notification) => _context.Notifications.Add(notification);

}