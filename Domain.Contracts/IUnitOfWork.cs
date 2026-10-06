using Domain.Contracts.Repositories;

namespace Domain.Contracts;

public interface IUnitOfWork
{
    ICourseRepository Courses { get; }
}