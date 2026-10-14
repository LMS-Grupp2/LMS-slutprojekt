
namespace LMS.Shared.DTOs.SubmissionDtos;
/// <summary>
/// One row in the teacher's submissions overview: one student and their status for the assignment.
/// SubmittedAt is nullable because a student who hasn't submitted has no submission time.
/// </summary>
public sealed record SubmissionOverviewRowDto(
    string StudentId,
    string StudentName,
    DateTime? SubmittedAt,
    bool Approved,
    bool NeedChanges,
    string Status,
    List<SubmissionFileDto> Files);