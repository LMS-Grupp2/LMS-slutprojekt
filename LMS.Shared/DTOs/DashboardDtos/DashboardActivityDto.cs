namespace LMS.Shared.DTOs.DashboardDtos;

/// <summary>
/// One row in the "Upcoming activities" card on the dashboard (US12).
/// A small version of the activity: the dashboard needs the module name and type, not the whole activity.
/// StartTime and EndTime are Swedish wall-clock time, shown as they are.
/// </summary>
public sealed record DashboardActivityDto(
    Guid Id,
    string Name,
    // For example Lecture, E-learning or Assignment. Shown as a badge.
    string ActivityTypeName,
    string ModuleName,
    DateTime StartTime,
    DateTime EndTime,
    Guid ModuleId);