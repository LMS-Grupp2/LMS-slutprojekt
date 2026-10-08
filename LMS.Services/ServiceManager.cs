using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IModuleService> _moduleService;
    private readonly Lazy<IStudentCourseService> _studentCourseService;
    private readonly Lazy<IUserService> _userService;

    public IAuthService AuthService => _authService.Value;
    public IModuleService ModuleService => _moduleService.Value;
    public IStudentCourseService StudentCourseService => _studentCourseService.Value;
    public IUserService UserService => _userService.Value;

    public ServiceManager(Lazy<IAuthService> authService, Lazy<IModuleService> moduleService,
        Lazy<IStudentCourseService> studentCourseService, Lazy<IUserService> userService)
    {
        _authService = authService;
        _moduleService = moduleService;
        _studentCourseService = studentCourseService;
        _userService = userService;
    }
}
