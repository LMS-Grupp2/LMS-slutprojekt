using LMS.Shared.DTOs.FeedbackDtos;

namespace Service.Contracts;

public interface IFeedbackService
{
    Task<FeedbackDto> GiveFeedbackAsync(Guid submissionId, string teacherId, CreateFeedbackDto dto);
    Task<IEnumerable<FeedbackDto>> GetBySubmissionIdAsync(Guid submissionId, string userId, bool isTeacher);
}
