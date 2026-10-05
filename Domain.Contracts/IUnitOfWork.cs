using Domain.Contracts.Repositories;

namespace Domain.Contracts;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICourseUserRepository CourseUsers { get; }
    Task SaveChangesAsync();
}