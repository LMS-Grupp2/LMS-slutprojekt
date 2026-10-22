using Domain.Contracts;
using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;


namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IUserRepository Users { get; }
    public ISubmissionRepository Submissions { get; } // NEW (US09)
    public IModuleRepository Modules { get; }
    public ICourseUserRepository CourseUsers { get; }
    public ICourseRepository Courses { get; }
    public IActivityRepository Activities { get; }
    public IDocumentRepository Documents { get; } // NEW (US06)
    public IAssignmentRepository Assignment { get; }
    public INotificationRepository Notification { get; }

    public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        Users = new UserRepository(userManager, context);
        Submissions = new SubmissionRepository(context); // NEW (US09)
        Modules = new ModuleRepository(context);
        CourseUsers = new CourseUserRepository(context);
        Courses = new CourseRepository(context);
        Activities = new ActivityRepository(context);
        Documents = new DocumentRepository(context); // NEW (US06)
        Assignment = new AssignmentRepository(context);
        Notification = new NotificationRepository(context);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}