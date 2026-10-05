using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IUserService> _userService;

    public IAuthService AuthService => _authService.Value;
    public IUserService UserService => _userService.Value;

    public ServiceManager(Lazy<IAuthService> authService, Lazy<IUserService> userService)
    {
        _authService = authService;
        _userService = userService;
    }
}
