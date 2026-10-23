using LMS.Shared.DTOs.DashboardDtos;

namespace Service.Contracts;

/// <summary>
/// Builds the teacher's dashboard (US13): what needs the teacher's attention right now.
/// Teachers see all courses, like on the Courses and Submissions pages.
/// </summary>
public interface ITeacherDashboardService
{
    /// <summary>
    /// Returns the counts (awaiting feedback, new, late, missing), the hand-ins that need attention,
    /// the next activities and the courses that are running now.
    /// </summary>
    Task<TeacherDashboardDto> GetForTeacherAsync();
}