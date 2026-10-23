namespace LMS.Shared.DTOs.DashboardDtos;

/// <summary>
/// Everything the teacher's dashboard (US13) shows, in one response.
/// The counts use the same status words as the rest of the app (SubmissionStatus),
/// so the numbers on the dashboard always match the Submissions page.
/// </summary>
/// <param name="AwaitingFeedbackCount">Hand-ins on time that wait for feedback.</param>
/// <param name="OldestAwaitingSubmittedAt">When the oldest of them was handed in (UTC), or null if there are none.</param>
/// <param name="NewSubmissionsCount">Hand-ins from the last 24 hours.</param>
/// <param name="LateCount">Hand-ins after the deadline that wait for feedback.</param>
/// <param name="LateAssignmentName">The assignment with the most late hand-ins, or null if none is late.</param>
/// <param name="MissingCount">Students who handed in nothing before the deadline passed.</param>
/// <param name="NeedsAttention">A short list of the hand-ins to look at first.</param>
/// <param name="UpcomingActivities">The next activities across all courses.</param>
/// <param name="CurrentCourses">The courses that are running now, with the current module.</param>
public sealed record TeacherDashboardDto(
    int AwaitingFeedbackCount,
    DateTime? OldestAwaitingSubmittedAt,
    int NewSubmissionsCount,
    int LateCount,
    string? LateAssignmentName,
    int MissingCount,
    IReadOnlyList<TeacherAttentionItemDto> NeedsAttention,
    IReadOnlyList<TeacherActivityDto> UpcomingActivities,
    IReadOnlyList<TeacherCourseDto> CurrentCourses);