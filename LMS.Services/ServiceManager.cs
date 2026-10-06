using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IStudentCourseService> _studentCourseService;

    public IAuthService AuthService => _authService.Value;
    public IStudentCourseService StudentCourseService => _studentCourseService.Value;

    public ServiceManager(Lazy<IAuthService> authService, Lazy<IStudentCourseService> studentCourseService)
    {
        _authService = authService;
        _studentCourseService = studentCourseService;
    }
}
