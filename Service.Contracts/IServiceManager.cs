namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    IStudentCourseService StudentCourseService { get; }
    IUserService UserService { get; }
}