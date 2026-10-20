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

    public async Task<Notification?> GetById(Guid id)
    {
        return await _context.Notifications
            .SingleOrDefaultAsync(notification => notification.Id == id);
    }

    public async Task<IEnumerable<Notification>> GetByStudentId(string studentId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetStudentsIdnByCourseIdAsync(Guid courseId)
    {
        return await _context.CourseUsers
            .AsNoTracking()
            .Where(c => c.CourseId == courseId)
            .Select(u => u.UserId)
            .Distinct()
            .ToListAsync();
    }

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


    // Todo - add AsNoTracking()
    public async Task<IEnumerable<string>> GetStudentsIdnByActivityIdAsync(Guid activityId)
    {
        return await (
            from activity in _context.Activities
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