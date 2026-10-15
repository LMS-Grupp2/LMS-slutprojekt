using LMS.Shared.DTOs.SubmissionDtos;

namespace Service.Contracts;

public interface ISubmissionService
{
    Task<StudentSubmissionDto> GetForStudentAsync(Guid activityId, string studentId);

    /// <summary>
    /// Hands in one file to an assignment. Creates the student's submission the first time,
    /// adds the file to it on later calls, and sets the hand-in time to now (UTC).
    /// Late hand-ins are allowed; the status shows them as late.
    /// </summary>
    Task<StudentSubmissionDto> SubmitAsync(
        Guid activityId,
        string studentId,
        Stream content,
        string fileName,
        string contentType,
        long length,
        CancellationToken cancellationToken);
}

