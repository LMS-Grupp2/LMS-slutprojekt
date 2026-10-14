using Domain.Models.Entities;

namespace LMS.Infrastructure.Repositories
{
    public interface IAssignmentRepository
    {
        Task<IEnumerable<Activity>> GetAssignmentsForStudentAsync(string studentId);
    }
}