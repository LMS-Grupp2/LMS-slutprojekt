using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(Guid submissionId);

    Task<Submission?> GetByActivityAndStudentAsync(Guid activityId, string studentId);

    void Create(Submission submission);

    Task<IEnumerable<Submission>> GetByActivityIdAsync(Guid activityId);

    Task<Submission?> GetForReviewAsync(Guid submissionId);
}