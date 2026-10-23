namespace LMS.Shared.DTOs.DashboardDtos;

/// <summary>
/// One upcoming activity on the teacher's dashboard. Unlike the student's version it also
/// names the course, because the teacher sees the activities of all courses together.
/// </summary>
/// <param name="Id">The activity.</param>
/// <param name="Name">For example "Component lifecycle - exercise".</param>
/// <param name="ActivityTypeName">Lecture, E-Learning, Exercise, Assignment, ...</param>
/// <param name="ModuleName">The module the activity belongs to.</param>
/// <param name="CourseName">The course the module belongs to.</param>
/// <param name="StartTime">Swedish wall-clock time, shown as it is.</param>
/// <param name="EndTime">Swedish wall-clock time, shown as it is.</param>
/// <param name="ModuleId">The module, so the page can link to the activity list of the module.</param>
public sealed record TeacherActivityDto(
    Guid Id,
    string Name,
    string ActivityTypeName,
    string ModuleName,
    string CourseName,
    DateTime StartTime,
    DateTime EndTime,
    Guid ModuleId);