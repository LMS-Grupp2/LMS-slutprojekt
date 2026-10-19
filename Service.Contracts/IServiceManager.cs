namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    ISubmissionService SubmissionService { get; } // NEW (US09)
    IModuleService ModuleService { get; }
    IStudentCourseService StudentCourseService { get; }
    IUserService UserService { get; }
    ICourseService CourseService { get; }
    IActivityService ActivityService { get; }
    IDocumentService DocumentService { get; } // NEW (US06)
    IAssignmentService AssignmentService { get; }
    IDashboardService DashboardService { get; } // NEW (US12)
}