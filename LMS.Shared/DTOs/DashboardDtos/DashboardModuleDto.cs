namespace LMS.Shared.DTOs.DashboardDtos;

/// <summary>
/// The "Current module" card on the dashboard (US12): the module that is running today,
/// or the next one if none is running, with how far into it we are.
/// </summary>
public sealed record DashboardModuleDto(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    // How much of the module has passed, 0 to 100. 0 before it starts, 100 after it ends.
    // Calculated from the dates, so the progress bar needs no extra data.
    int ProgressPercent);