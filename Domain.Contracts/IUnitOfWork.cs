using Domain.Contracts.Repositories;

namespace Domain.Contracts;

public interface IUnitOfWork
{
    ICourseRepository Courses { get; }
    IModuleRepository Modules { get; }
    IUserRepository Users { get; }
    ICourseUserRepository CourseUsers { get; }
    Task SaveChangesAsync();
}