using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IStudentCourseService> _studentCourseService;
    private readonly Lazy<IUserService> _userService;
    private readonly Lazy<ICourseService> _courseService;

    public IAuthService AuthService => _authService.Value;
    public IStudentCourseService StudentCourseService => _studentCourseService.Value;
    public IUserService UserService => _userService.Value;
    public ICourseService CourseService => _courseService.Value;



    public ServiceManager(Lazy<IAuthService> authService, Lazy<IStudentCourseService> studentCourseService,
        Lazy<IUserService> userService, Lazy<ICourseService> courseService)
    {
        _authService = authService;
        _studentCourseService = studentCourseService;
        _userService = userService;
        _courseService = courseService;
    }
}
