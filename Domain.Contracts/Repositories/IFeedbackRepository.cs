
using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface IFeedbackRepository
{ 
    void Create(Feedback feedback);
    Task<IEnumerable<Feedback>> GetBySubmissionIdAsync(Guid submissionId);
}
                    