using LMS.Shared.DTOs.SubmissionDtos;

namespace Service.Contracts;

public interface ISubmissionService
{
    Task<StudentSubmissionDto> GetForStudentAsync(Guid activityId, string studentId);
}

