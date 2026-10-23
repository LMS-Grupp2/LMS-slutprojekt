using LMS.Shared.DTOs.AssignmentDtos;

namespace LMS.Shared.DTOs.DashboardDtos;

/// <summary>
/// Output for the student's dashboard (US12): what the student needs to see right after login,
/// limited to the student's own course. Only data leaves the API, never entities.
/// New feedback and notifications are added later (US11 and US14).
/// </summary>
public sealed record StudentDashboardDto(
    // The student's own course
    Guid CourseId,
    string CourseName,

    // The module that is running today (or the next one if none is running). Null if the course has no modules.
    DashboardModuleDto? CurrentModule,

    // The next activities in the course, earliest first (activities that have not ended yet).
    // The first one is shown as "Next activity".
    IReadOnlyList<DashboardActivityDto> UpcomingActivities,

    // Counts for the cards. They are counted over ALL the student's assignments,
    // so they stay correct even though the lists below are shortened.
    // NotSubmittedCount: nothing handed in ("Not submitted" and "Missing")
    // OverdueCount: nothing handed in and the deadline has passed ("Missing")
    // DueThisWeekCount: nothing handed in and the deadline is within 7 days
    // FeedbackGivenCount: assignments where the teacher has given feedback
    int NotSubmittedCount,
    int OverdueCount,
    int DueThisWeekCount,
    int FeedbackGivenCount,

    // Assignments with status "Missing". Shown in the red banner, oldest deadline first.
    IReadOnlyList<StudentAssignmentDto> OverdueAssignments,

    // A short list for the "Deadlines" card: missing first, then not submitted (nearest deadline first),
    // then the rest (newest deadline first).
    IReadOnlyList<StudentAssignmentDto> Deadlines,

    // The newest documents in the course (course material and documents shared by students)
    IReadOnlyList<DashboardDocumentDto> RecentDocuments);