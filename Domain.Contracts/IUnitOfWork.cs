using Domain.Contracts.Repositories;
using LMS.Infrastructure.Repositories;

namespace Domain.Contracts;

public interface IUnitOfWork
{
    ICourseRepository Courses { get; }
    IModuleRepository Modules { get; }
    IUserRepository Users { get; }
    ICourseUserRepository CourseUsers { get; }
    IActivityRepository Activities { get; }
    IAssignmentRepository Assignment { get; }
    Task SaveChangesAsync();
}