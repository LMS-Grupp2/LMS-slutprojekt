using LMS.Shared.DTOs.DashboardDtos;

namespace Service.Contracts;

/// <summary>
/// Builds the student's dashboard (US12): the next activities, deadlines, assignments that need action
/// and the latest documents, all limited to the student's own course.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Returns the dashboard for the given student.
    /// Throws CourseNotFoundException (404) if the student is not enrolled in a course.
    /// </summary>
    Task<StudentDashboardDto> GetForStudentAsync(string studentId);
}