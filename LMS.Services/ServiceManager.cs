using Service.Contracts;

namespace LMS.Services;

/// <summary>
/// Gives controllers access to all services. Each service is created lazily,
/// so it is only built when it is actually used.
/// </summary>
public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<ISubmissionService> _submissionService; // <- NEW (US09)
    private readonly Lazy<ICourseService> _courseService;
    private readonly Lazy<IModuleService> _moduleService;
    private readonly Lazy<IStudentCourseService> _studentCourseService;
    private readonly Lazy<IUserService> _userService;
    private readonly Lazy<IActivityService> _activityService;
    private readonly Lazy<IDocumentService> _documentService; // NEW (US06)
    private readonly Lazy<IAssignmentService> _assignmentService;
    private readonly Lazy<IDashboardService> _dashboardService; // NEW (US12)
    private readonly Lazy<INotificationService> _notificationService;

    public IAuthService AuthService => _authService.Value;
    public ISubmissionService SubmissionService => _submissionService.Value; // NEW (US09)
    public ICourseService CourseService => _courseService.Value;
    public IModuleService ModuleService => _moduleService.Value;
    public IStudentCourseService StudentCourseService => _studentCourseService.Value;
    public IUserService UserService => _userService.Value;
    public IActivityService ActivityService => _activityService.Value;
    public IDocumentService DocumentService => _documentService.Value; // NEW (US06)
    public IAssignmentService AssignmentService => _assignmentService.Value;
    public INotificationService NotificationService => _notificationService.Value;
    public IDashboardService DashboardService => _dashboardService.Value; // NEW (US12)

    public ServiceManager(
        Lazy<IDocumentService> documentService, // NEW (US06)
        Lazy<ISubmissionService> submissionService, // NEW (US09)
        Lazy<IAuthService> authService,
        Lazy<ICourseService> courseService,
        Lazy<IModuleService> moduleService,
        Lazy<IStudentCourseService> studentCourseService,
        Lazy<IUserService> userService,
        Lazy<IActivityService> activityService,
        Lazy<IDashboardService> dashboardService,  // NEW (US12)
        Lazy<IAssignmentService> assignmentService,
        Lazy<INotificationService> notificationService)
    {
        _authService = authService;
        _submissionService = submissionService; // NEW (US09)
        _courseService = courseService;
        _moduleService = moduleService;
        _studentCourseService = studentCourseService;
        _userService = userService;
        _activityService = activityService;
        _documentService = documentService; // NEW (US06)
        _assignmentService = assignmentService;
        _dashboardService = dashboardService; // NEW (US12)
        _notificationService = notificationService;
    }
}