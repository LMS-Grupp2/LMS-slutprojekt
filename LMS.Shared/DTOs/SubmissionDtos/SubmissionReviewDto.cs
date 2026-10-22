using LMS.Shared.DTOs.FeedbackDtos;


namespace LMS.Shared.DTOs.SubmissionDtos;

/// <summary>
/// One student's submission for the teacher's review page: who, which assignment, files, status and earlier feedback.
/// SubmittedAt is UTC (show with ToLocalTime); Deadline is the activity's EndTime in Swedish time.
/// </summary>
public sealed record SubmissionReviewDto(
    string CourseName,
    string AssignmentName,
    string StudentName,
    string Status,
    DateTime SubmittedAt,
    DateTime Deadline,
    List<FeedbackDto> Feedbacks,
    List<SubmissionFileDto> Files
);