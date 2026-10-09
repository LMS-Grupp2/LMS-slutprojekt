namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    IModuleService ModuleService { get; }
    IStudentCourseService StudentCourseService { get; }
    IUserService UserService { get; }
    ICourseService CourseService { get; }
    IActivityService ActivityService { get;  }
}