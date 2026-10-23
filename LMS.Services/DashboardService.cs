using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DashboardDtos;
using Service.Contracts;

namespace LMS.Services;

/// <summary>
/// Builds the student's dashboard (US12). Everything is limited to the student's own course:
/// the course is found from the student's enrollment, never from an id sent by the client.
/// The assignments (with their status) come from AssignmentService, so the status rules live in one place.
/// </summary>
public class DashboardService : IDashboardService
{
    // How many rows each list on the dashboard shows
    private const int UpcomingActivityCount = 5;
    private const int DeadlineCount = 5;
    private const int RecentDocumentCount = 5;

    // "Due this week" means a deadline within this many days
    private const int DueSoonDays = 7;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAssignmentService _assignmentService;

    public DashboardService(IUnitOfWork unitOfWork, IAssignmentService assignmentService)
    {
        _unitOfWork = unitOfWork;
        _assignmentService = assignmentService;
    }

    public async Task<StudentDashboardDto> GetForStudentAsync(string studentId)
    {
        // 1. The student's own course. A student who is not enrolled gets a 404, like on the course page.
        var enrollments = await _unitOfWork.CourseUsers.GetByUserIdAsync(studentId);
        var courseId = enrollments.FirstOrDefault()?.CourseId
            ?? throw new CourseNotFoundException("You are not enrolled in any course.", "Course not found");

        var course = await _unitOfWork.Courses.GetCourseByIdAsync(courseId)
            ?? throw new CourseNotFoundException("You are not enrolled in any course.", "Course not found");

        // The same "now" for the whole dashboard. Server times are UTC, activity times are Swedish wall-clock time.
        var now = DateTime.UtcNow;
        var courseNow = CourseTime.FromUtc(now);

        // 2. The assignments with their status (calculated by AssignmentService, one query)
        var assignments = (await _assignmentService.GetAssignmentsForStudentAsync(studentId)).ToList();

        // Assignments that were never handed in and whose deadline has passed. Oldest deadline first.
        var overdue = assignments
            .Where(a => a.Status == SubmissionStatus.Missing)
            .OrderBy(a => a.EndTime)
            .ToList();

        // 3. The "Deadlines" card: missing first, then not submitted (nearest deadline first), then the rest
        var deadlines = overdue
            .Concat(assignments
                .Where(a => a.Status == SubmissionStatus.NotSubmitted)
                .OrderBy(a => a.EndTime))
            .Concat(assignments
                .Where(a => a.Status != SubmissionStatus.Missing && a.Status != SubmissionStatus.NotSubmitted)
                .OrderByDescending(a => a.EndTime))
            .Take(DeadlineCount)
            .ToList();

        // 4. The counts for the cards. Counted over all assignments, not over the shortened list above.
        var notSubmittedCount = assignments.Count(a =>
            a.Status == SubmissionStatus.NotSubmitted || a.Status == SubmissionStatus.Missing);

        var dueThisWeekCount = assignments.Count(a =>
            a.Status == SubmissionStatus.NotSubmitted
            && CourseTime.ToUtc(a.EndTime) <= now.AddDays(DueSoonDays));

        var feedbackGivenCount = assignments.Count(a => a.HasFeedback);

        // 5. The current module (from the module dates, "today" in Swedish time)
        var modules = (await _unitOfWork.Modules.GetByCourseIdAsync(course.Id)).ToList();
        var currentModule = FindCurrentModule(modules, DateOnly.FromDateTime(courseNow));

        // 6. The next activities in the course (one query, Module and ActivityType are included)
        var activities = (await _unitOfWork.Activities
                .GetUpcomingByCourseIdAsync(course.Id, courseNow, UpcomingActivityCount))
            .Select(a => new DashboardActivityDto(
                a.Id,
                a.Name,
                a.ActivityType.Name,
                a.Module.Name,
                a.StartTime,
                a.EndTime,
                a.ModuleId))
            .ToList();

        // 7. The latest documents (one query, hand-in files are left out)
        var documents = (await _unitOfWork.Documents.GetRecentByCourseIdAsync(course.Id, RecentDocumentCount))
            .Select(d => new DashboardDocumentDto(
                d.Id,
                d.Name,
                d.FileType.ToString(),
                ContextOf(d),
                d.User.Name,
                d.UploadedAt))
            .ToList();

        return new StudentDashboardDto(
            course.Id,
            course.Name,
            currentModule,
            activities,
            notSubmittedCount,
            overdue.Count,
            dueThisWeekCount,
            feedbackGivenCount,
            overdue,
            deadlines,
            documents);
    }

    // The module that is running today. If none is running, the next one. If all have ended, the last one.
    // Null only when the course has no modules. The modules come sorted by start date from the repository.
    private static DashboardModuleDto? FindCurrentModule(List<Module> modules, DateOnly today)
    {
        var module = modules.FirstOrDefault(m => m.StartDate <= today && today <= m.EndDate)
            ?? modules.FirstOrDefault(m => m.StartDate > today)
            ?? modules.LastOrDefault();

        return module is null
            ? null
            : new DashboardModuleDto(module.Id, module.Name, module.StartDate, module.EndDate,
                ProgressPercent(module, today));
    }

    // How much of the module has passed, 0 to 100: 0 before it starts, 100 after it ends
    private static int ProgressPercent(Module module, DateOnly today)
    {
        if (today <= module.StartDate)
            return 0;

        if (today >= module.EndDate)
            return 100;

        var totalDays = module.EndDate.DayNumber - module.StartDate.DayNumber;
        var passedDays = today.DayNumber - module.StartDate.DayNumber;
        return (int)Math.Round(100.0 * passedDays / totalDays);
    }

    // Where a document belongs, as a short text for the dashboard
    private static string ContextOf(Document document) =>
        document.Module is not null ? $"Module: {document.Module.Name}"
        : document.Activity is not null ? $"Activity: {document.Activity.Name}"
        : "Course";
}