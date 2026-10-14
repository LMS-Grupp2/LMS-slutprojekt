using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface ISubmissionRepository
{
    Task<Submission?> GetByActivityAndStudentAsync(Guid activityId, string studentId);

    void Create(Submission submission);
}