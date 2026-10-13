namespace LMS.Shared.DTOs.SubmissionDtos;

/// <summary>
/// The teacher's submissions overview for one assignment.
/// The deadline is here once because it's the same for every student; Rows has one entry per student.
/// </summary>
public sealed record SubmissionOverviewDto(
    Guid ActivityId,
    string ActivityName,
    DateTime Deadline,
    List<SubmissionOverviewRowDto> Rows);

