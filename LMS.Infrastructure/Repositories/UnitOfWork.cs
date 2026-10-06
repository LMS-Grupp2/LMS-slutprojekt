using Domain.Contracts;
using Domain.Contracts.Repositories;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public ICourseRepository Courses { get; } = new CourseRepository(context);
}