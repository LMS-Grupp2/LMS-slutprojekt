using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DashboardDtos;
using LMS.Shared.Paging;
using Service.Contracts;

namespace LMS.Services;

/// <summary>
/// Builds the teacher's dashboard (US13). The teacher sees all courses, like on the Courses and Submissions pages.
/// Every status comes from SubmissionStatusCalculator, the same rules as the student's pages and the
/// Submissions page, so the numbers on the dashboard always match the rest of the app.
/// </summary>
public class TeacherDashboardService : ITeacherDashboardService
{
    // How many rows each list on the dashboard shows
    private const int NeedsAttentionCount = 5;
    private const int UpcomingActivityCount = 4;

    // "New submissions" means hand-ins from this many hours back
    private const int NewSubmissionHours = 24;

    private readonly IUnitOfWork _unitOfWork;

    public TeacherDashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TeacherDashboardDto> GetForTeacherAsync()
    {
        // The same "now" for the whole dashboard. Server times are UTC, activity times are Swedish wall-clock time.
        var now = DateTime.UtcNow;
        var courseNow = CourseTime.FromUtc(now);
        var today = DateOnly.FromDateTime(courseNow);

        // 1. All courses and, per course, its students. An empty query (no search, no status) returns every course.
        //    Teachers can also be course members, so the members are filtered by role
        //    (two calls: the members of a course + the roles of all users).
        var courses = (await _unitOfWork.Courses.GetAllCoursesAsync(new CourseListQuery(), today)).ToList();
        var roleNames = await _unitOfWork.Users.GetRoleNamesByUserIdAsync();

        var studentsByCourse = new Dictionary<Guid, List<CourseUser>>();
        foreach (var course in courses)
        {
            var members = await _unitOfWork.CourseUsers.GetByCourseIdAsync(course.Id);
            studentsByCourse[course.Id] = members
                .Where(m => roleNames.TryGetValue(m.UserId, out var role) && role == UserRoles.Student)
                .ToList();
        }

        // 2. One row per student and assignment (a left join in memory): no submission means null,
        //    which the calculator turns into "Not submitted" or "Missing".
        var assignments = await _unitOfWork.Activities.GetAssignmentsWithSubmissionsAsync(ActivityTypeNames.Assignment);
        var rows = new List<Row>();

        foreach (var assignment in assignments)
        {
            // An assignment in a course that no longer exists has no students to show
            if (!studentsByCourse.TryGetValue(assignment.Module.CourseId, out var students))
                continue;

            var deadlineUtc = CourseTime.ToUtc(assignment.EndTime);
            var submissionsByStudent = assignment.Submissions.ToDictionary(s => s.StudentId);

            foreach (var student in students)
            {
                var submission = submissionsByStudent.GetValueOrDefault(student.UserId);
                var status = SubmissionStatusCalculator.Calculate(submission, deadlineUtc, now);
                rows.Add(new Row(student.User, assignment, submission, status));
            }
        }

        // 3. The four cards. Counted over all rows, not over the shortened list below.
        var awaiting = rows.Where(r => r.Status == SubmissionStatus.AwaitingFeedback).ToList();
        var late = rows.Where(r => r.Status == SubmissionStatus.Late).ToList();
        var missing = rows.Where(r => r.Status == SubmissionStatus.Missing).ToList();

        // How long the oldest hand-in has waited for feedback (UTC, the page turns it into "4 days ago")
        DateTime? oldestAwaiting = awaiting.Count > 0
            ? awaiting.Min(r => r.Submission!.SubmittedAt)
            : null;

        // The assignment with the most late hand-ins (the name is shown under the "Late" count)
        var lateAssignmentName = late
            .GroupBy(r => r.Assignment.Name)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => g.Key)
            .FirstOrDefault();

        // Every hand-in from the last 24 hours counts as new, whatever its status is now
        var newSince = now.AddHours(-NewSubmissionHours);
        var newSubmissionsCount = rows.Count(r => r.Submission is not null && r.Submission.SubmittedAt >= newSince);

        // 4. "Needs your attention": late first, then awaiting feedback (oldest first), then missing
        //    (oldest deadline first). Only a few rows, the Submissions page shows everything.
        var needsAttention = late.OrderBy(r => r.Submission!.SubmittedAt)
            .Concat(awaiting.OrderBy(r => r.Submission!.SubmittedAt))
            .Concat(missing.OrderBy(r => r.Assignment.EndTime))
            .Take(NeedsAttentionCount)
            .Select(r => new TeacherAttentionItemDto(
                r.Student.Id,
                r.Student.Name,
                r.Assignment.Id,
                r.Assignment.Name,
                r.Status,
                r.Submission?.SubmittedAt,
                r.Assignment.EndTime))
            .ToList();

        // 5. The next activities in all courses (one query, Module + Course + ActivityType are included)
        var upcoming = (await _unitOfWork.Activities.GetUpcomingAcrossCoursesAsync(courseNow, UpcomingActivityCount))
            .Select(a => new TeacherActivityDto(
                a.Id,
                a.Name,
                a.ActivityType.Name,
                a.Module.Name,
                a.Module.Course.Name,
                a.StartTime,
                a.EndTime,
                a.ModuleId))
            .ToList();

        // 6. The courses that are running today, oldest start first, each with its current module
        var currentCourses = new List<TeacherCourseDto>();
        foreach (var course in courses
                     .Where(c => c.StartDate <= today && today <= c.EndDate)
                     .OrderBy(c => c.StartDate))
        {
            var modules = (await _unitOfWork.Modules.GetByCourseIdAsync(course.Id)).ToList();
            var module = CurrentModuleOf(modules, today);

            currentCourses.Add(new TeacherCourseDto(
                course.Id,
                course.Name,
                course.StartDate,
                course.EndDate,
                studentsByCourse[course.Id].Count,
                module?.Name,
                module is null ? 0 : ProgressPercent(module, today)));
        }

        return new TeacherDashboardDto(
            awaiting.Count,
            oldestAwaiting,
            newSubmissionsCount,
            late.Count,
            lateAssignmentName,
            missing.Count,
            needsAttention,
            upcoming,
            currentCourses);
    }

    // The module that is running today. If none is running, the next one. If all have ended, the last one.
    // Null only when the course has no modules. The modules come sorted by start date from the repository.
    private static Module? CurrentModuleOf(List<Module> modules, DateOnly today) =>
        modules.FirstOrDefault(m => m.StartDate <= today && today <= m.EndDate)
        ?? modules.FirstOrDefault(m => m.StartDate > today)
        ?? modules.LastOrDefault();

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

    // One student's situation for one assignment. Submission is null when nothing was handed in.
    private sealed record Row(ApplicationUser Student, Activity Assignment, Submission? Submission, string Status);
}