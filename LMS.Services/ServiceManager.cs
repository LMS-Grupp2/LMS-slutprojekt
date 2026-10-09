using Service.Contracts;

namespace LMS.Services;

/// <summary>
/// Gives controllers access to all services. Each service is created lazily,
/// so it is only built when it is actually used.
/// </summary>
public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<ICourseService> _courseService;
    private readonly Lazy<IModuleService> _moduleService;
    private readonly Lazy<IStudentCourseService> _studentCourseService;
    private readonly Lazy<IUserService> _userService;

    private readonly Lazy<IActivityService> _activityService;

    public IAuthService AuthService => _authService.Value;
    public ICourseService CourseService => _courseService.Value;
    public IModuleService ModuleService => _moduleService.Value;
    public IStudentCourseService StudentCourseService => _studentCourseService.Value;
    public IUserService UserService => _userService.Value;
    public IActivityService ActivityService => _activityService.Value;

    public ServiceManager(
        Lazy<IAuthService> authService,
        Lazy<ICourseService> courseService,
        Lazy<IModuleService> moduleService,
        Lazy<IStudentCourseService> studentCourseService,
        Lazy<IUserService> userService,
        Lazy<IActivityService> activityService)
    {
        _authService = authService;
        _courseService = courseService;
        _moduleService = moduleService;
        _studentCourseService = studentCourseService;
        _userService = userService;
        _activityService = activityService;
        
    }
}