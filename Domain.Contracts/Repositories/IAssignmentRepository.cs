using Domain.Models.Entities;

namespace Domain.Contracts.Repositories
{
    public interface IAssignmentRepository
    {
        Task<IEnumerable<Activity>> GetAssignmentsForStudentAsync(string studentId);
    }
}