using Domain.Contracts.Repositories;
using LMS.Infrastructure.Repositories;

namespace Domain.Contracts;

public interface IUnitOfWork
{
    ICourseRepository Courses { get; }
    ISubmissionRepository Submissions { get; } // < US09: access to submissions
    IModuleRepository Modules { get; }
    IUserRepository Users { get; }
    ICourseUserRepository CourseUsers { get; }
    IActivityRepository Activities { get; }
    IDocumentRepository Documents { get; } // NEW (US06): access to document metadata
    IAssignmentRepository Assignment { get; }
    Task SaveChangesAsync();
}