namespace LMS.Shared.DTOs.SubmissionDtos;

/// <summary>
/// A student's view of one assignment: the deadline and what they have handed in so far.
/// Returned even when nothing is handed in yet (SubmissionId and SubmittedAt are then null),
/// so the page can show the deadline and the status "Not submitted" or "Missing".
/// </summary>
public sealed record StudentSubmissionDto(
    Guid? SubmissionId,
    Guid ActivityId,
    string ActivityName,
    DateTime Deadline,
    DateTime? SubmittedAt,
    bool IsLate,
    string Status,
    List<SubmissionFileDto> Files);
