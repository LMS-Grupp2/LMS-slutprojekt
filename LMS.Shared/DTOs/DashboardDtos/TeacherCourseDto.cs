namespace LMS.Shared.DTOs.DashboardDtos;

/// <summary>
/// One course in the "Current courses" card of the teacher's dashboard:
/// the course, how many students it has, and which module is running now.
/// </summary>
/// <param name="Id">The course, used for the Open button.</param>
/// <param name="Name">For example ".NET 2026".</param>
/// <param name="StartDate">First day of the course.</param>
/// <param name="EndDate">Last day of the course.</param>
/// <param name="StudentCount">Only students, teachers in the course are not counted.</param>
/// <param name="CurrentModuleName">The module running today (or the next one), null if the course has no modules.</param>
/// <param name="ProgressPercent">How far the current module has come, 0 to 100.</param>
public sealed record TeacherCourseDto(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentCount,
    string? CurrentModuleName,
    int ProgressPercent);