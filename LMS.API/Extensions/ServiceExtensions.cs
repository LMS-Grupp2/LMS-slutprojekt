using LMS.Infrastructure.Repositories;
using LMS.Services;

namespace LMS.API.Extensions;

public static class ServiceExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static void AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddLazy<IAuthService>();
        services.AddScoped<ISubmissionService, SubmissionService>(); // NEW (US09)
        services.AddLazy<ISubmissionService>(); // NEW (US09)
        services.AddScoped<IModuleService, ModuleService>();
        services.AddLazy<IModuleService>();
        services.AddScoped<IStudentCourseService, StudentCourseService>();
        services.AddLazy<IStudentCourseService>();
        services.AddScoped<IUserService, UserService>();
        services.AddLazy<IUserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddLazy<ICourseService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddLazy<IActivityService>();
        services.AddScoped<IDocumentService, DocumentService>(); // NEW (US06)
        services.AddLazy<IDocumentService>(); // NEW (US06)
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddLazy<IAssignmentService>();
        services.AddScoped<IDashboardService, DashboardService>(); // NEW (US12)
        services.AddLazy<IDashboardService>(); // NEW (US12)
        services.AddScoped<INotificationService, NotificationService>();
        services.AddLazy<INotificationService>();
    }
}