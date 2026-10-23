using Domain.Contracts;
using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using Domain.Models.Enums;
using Domain.Models.Exceptions;
using LMS.Services;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.AssignmentDtos;
using Moq;
using Service.Contracts;

namespace LMS.Tests;

/// <summary>
/// DashboardService (US12) with a fake database (mocked repositories) and a fake AssignmentService,
/// so the dashboard rules can be tested without SQL Server. All dates are calculated from "now",
/// so the tests do not depend on the calendar.
/// </summary>
public class DashboardServiceTests
{
    private const string StudentId = "student-1";
    private static readonly Guid CourseId = Guid.NewGuid();

    private readonly Mock<ICourseUserRepository> _courseUsers = new();
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly Mock<IModuleRepository> _modules = new();
    private readonly Mock<IActivityRepository> _activities = new();
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<IAssignmentService> _assignments = new();
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.CourseUsers).Returns(_courseUsers.Object);
        unitOfWork.Setup(u => u.Courses).Returns(_courses.Object);
        unitOfWork.Setup(u => u.Modules).Returns(_modules.Object);
        unitOfWork.Setup(u => u.Activities).Returns(_activities.Object);
        unitOfWork.Setup(u => u.Documents).Returns(_documents.Object);

        // The student is enrolled in one course
        _courseUsers.Setup(r => r.GetByUserIdAsync(StudentId))
            .ReturnsAsync([new CourseUser { CourseId = CourseId, UserId = StudentId }]);
        _courses.Setup(r => r.GetCourseByIdAsync(CourseId))
            .ReturnsAsync(new Course { Id = CourseId, Name = "Fullstack .NET" });

        // By default the course has nothing else: no assignments, modules, activities or documents
        GivenAssignments();
        _modules.Setup(r => r.GetByCourseIdAsync(CourseId)).ReturnsAsync(new List<Module>());
        _activities.Setup(r => r.GetUpcomingByCourseIdAsync(CourseId, It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Activity>());
        _documents.Setup(r => r.GetRecentByCourseIdAsync(CourseId, It.IsAny<int>()))
            .ReturnsAsync(new List<Document>());

        _service = new DashboardService(unitOfWork.Object, _assignments.Object);
    }

    [Fact]
    public async Task StudentNotEnrolled_ThrowsNotFound()
    {
        //ARRANGE
        _courseUsers.Setup(r => r.GetByUserIdAsync("stranger")).ReturnsAsync(new List<CourseUser>());

        //ACT + ASSERT
        await Assert.ThrowsAsync<CourseNotFoundException>(() => _service.GetForStudentAsync("stranger"));
    }

    [Fact]
    public async Task Dashboard_ReturnsTheStudentsOwnCourse()
    {
        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        Assert.Equal(CourseId, result.CourseId);
        Assert.Equal("Fullstack .NET", result.CourseName);
    }

    [Fact]
    public async Task Counts_AreCalculatedOverAllAssignments()
    {
        //ARRANGE
        GivenAssignments(
            Assignment("Missing one", SubmissionStatus.Missing, daysFromNow: -3),
            Assignment("Due soon", SubmissionStatus.NotSubmitted, daysFromNow: 3),
            Assignment("Due later", SubmissionStatus.NotSubmitted, daysFromNow: 20),
            Assignment("Waiting", SubmissionStatus.AwaitingFeedback, daysFromNow: -5),
            Assignment("Graded", SubmissionStatus.FeedbackGiven, daysFromNow: -10, hasFeedback: true));

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        Assert.Equal(3, result.NotSubmittedCount);   // Not submitted (2) + Missing (1)
        Assert.Equal(1, result.OverdueCount);        // Only Missing
        Assert.Equal(1, result.DueThisWeekCount);    // Only "Due soon", not "Due later"
        Assert.Equal(1, result.FeedbackGivenCount);  // Only "Graded"
    }

    [Fact]
    public async Task OverdueAssignments_AreTheMissingOnes_OldestDeadlineFirst()
    {
        //ARRANGE
        GivenAssignments(
            Assignment("Newer", SubmissionStatus.Missing, daysFromNow: -2),
            Assignment("Older", SubmissionStatus.Missing, daysFromNow: -9),
            Assignment("Handed in late", SubmissionStatus.Late, daysFromNow: -4));

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        Assert.Equal(["Older", "Newer"], result.OverdueAssignments.Select(a => a.ActivityName));
    }

    [Fact]
    public async Task Deadlines_ListMissingFirst_ThenNotSubmitted_ThenTheRest()
    {
        //ARRANGE
        GivenAssignments(
            Assignment("Graded", SubmissionStatus.FeedbackGiven, daysFromNow: -8, hasFeedback: true),
            Assignment("Later", SubmissionStatus.NotSubmitted, daysFromNow: 10),
            Assignment("Missing", SubmissionStatus.Missing, daysFromNow: -1),
            Assignment("Soon", SubmissionStatus.NotSubmitted, daysFromNow: 2));

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        Assert.Equal(["Missing", "Soon", "Later", "Graded"], result.Deadlines.Select(d => d.ActivityName));
    }

    [Fact]
    public async Task Deadlines_ShowAtMostFive_ButCountsStillUseAllAssignments()
    {
        //ARRANGE: seven assignments that are not submitted yet
        GivenAssignments(Enumerable.Range(1, 7)
            .Select(i => Assignment($"A{i}", SubmissionStatus.NotSubmitted, daysFromNow: i))
            .ToArray());

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        Assert.Equal(5, result.Deadlines.Count);
        Assert.Equal(7, result.NotSubmittedCount);
    }

    [Fact]
    public async Task CurrentModule_IsTheOneRunningToday_WithProgress()
    {
        //ARRANGE: 10 days ago to 10 days ahead, so today is exactly halfway
        var today = Today();
        var running = Module("Running", today.AddDays(-10), today.AddDays(10));
        var ended = Module("Ended", today.AddDays(-60), today.AddDays(-30));
        _modules.Setup(r => r.GetByCourseIdAsync(CourseId)).ReturnsAsync(new List<Module> { ended, running });

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        Assert.NotNull(result.CurrentModule);
        Assert.Equal("Running", result.CurrentModule.Name);
        Assert.Equal(50, result.CurrentModule.ProgressPercent);
    }

    [Fact]
    public async Task CurrentModule_IsTheNextOne_WhenNoneIsRunning()
    {
        //ARRANGE
        var today = Today();
        var ended = Module("Ended", today.AddDays(-60), today.AddDays(-30));
        var next = Module("Next", today.AddDays(5), today.AddDays(30));
        _modules.Setup(r => r.GetByCourseIdAsync(CourseId)).ReturnsAsync(new List<Module> { ended, next });

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT: not started yet, so 0 percent
        Assert.Equal("Next", result.CurrentModule!.Name);
        Assert.Equal(0, result.CurrentModule.ProgressPercent);
    }

    [Fact]
    public async Task CurrentModule_IsTheLastOne_WhenAllHaveEnded()
    {
        //ARRANGE
        var today = Today();
        var first = Module("First", today.AddDays(-90), today.AddDays(-60));
        var last = Module("Last", today.AddDays(-59), today.AddDays(-30));
        _modules.Setup(r => r.GetByCourseIdAsync(CourseId)).ReturnsAsync(new List<Module> { first, last });

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT: finished, so 100 percent
        Assert.Equal("Last", result.CurrentModule!.Name);
        Assert.Equal(100, result.CurrentModule.ProgressPercent);
    }

    [Fact]
    public async Task CurrentModule_IsNull_WhenTheCourseHasNoModules()
    {
        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        Assert.Null(result.CurrentModule);
    }

    [Fact]
    public async Task UpcomingActivities_AreMappedWithTypeAndModuleName()
    {
        //ARRANGE
        var module = Module("Module 3", Today().AddDays(-5), Today().AddDays(50));
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Name = "Lecture 1",
            ModuleId = module.Id,
            Module = module,
            ActivityType = new ActivityType { Id = Guid.NewGuid(), Name = "Lecture" },
            StartTime = DateTime.Today.AddDays(1).AddHours(9),
            EndTime = DateTime.Today.AddDays(1).AddHours(10)
        };
        _activities.Setup(r => r.GetUpcomingByCourseIdAsync(CourseId, It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Activity> { activity });

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        var item = Assert.Single(result.UpcomingActivities);
        Assert.Equal("Lecture 1", item.Name);
        Assert.Equal("Lecture", item.ActivityTypeName);
        Assert.Equal("Module 3", item.ModuleName);
    }

    [Fact]
    public async Task RecentDocuments_ShowWhereEachDocumentBelongs()
    {
        //ARRANGE: one document on the course, one on a module and one on an activity
        var module = Module("Module 1", Today().AddDays(-5), Today().AddDays(50));
        var activity = new Activity { Id = Guid.NewGuid(), Name = "Exercise 2" };
        var teacher = new ApplicationUser { Id = "teacher-1", Name = "Demo Teacher" };

        _documents.Setup(r => r.GetRecentByCourseIdAsync(CourseId, It.IsAny<int>()))
            .ReturnsAsync(new List<Document>
            {
                Document("Course.pdf", teacher),
                Document("Module.pdf", teacher, module: module),
                Document("Activity.pdf", teacher, activity: activity)
            });

        //ACT
        var result = await _service.GetForStudentAsync(StudentId);

        //ASSERT
        Assert.Equal(
            ["Course", "Module: Module 1", "Activity: Exercise 2"],
            result.RecentDocuments.Select(d => d.Context));
        Assert.All(result.RecentDocuments, d => Assert.Equal("Demo Teacher", d.UploadedByName));
    }

    // ---------- Helpers ----------

    // "Today" in Swedish time, the same day the service uses for the modules
    private static DateOnly Today() => DateOnly.FromDateTime(CourseTime.FromUtc(DateTime.UtcNow));

    // The fake AssignmentService returns exactly these assignments
    private void GivenAssignments(params StudentAssignmentDto[] assignments) =>
        _assignments.Setup(a => a.GetAssignmentsForStudentAsync(StudentId)).ReturnsAsync(assignments);

    // A deadline "daysFromNow" days from now, as Swedish wall-clock time (like the real activity times)
    private static StudentAssignmentDto Assignment(
        string name, string status, int daysFromNow, bool hasFeedback = false) => new()
        {
            ActivityId = Guid.NewGuid(),
            ActivityName = name,
            EndTime = CourseTime.FromUtc(DateTime.UtcNow.AddDays(daysFromNow)),
            ModuleId = Guid.NewGuid(),
            ModuleName = "Module",
            CourseId = CourseId,
            CourseName = "Fullstack .NET",
            Status = status,
            HasFeedback = hasFeedback
        };

    private static Module Module(string name, DateOnly start, DateOnly end) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        StartDate = start,
        EndDate = end,
        CourseId = CourseId
    };

    private static Document Document(
        string name, ApplicationUser user, Module? module = null, Activity? activity = null) => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            StoredFileName = Guid.NewGuid().ToString(),
            ContentType = "application/pdf",
            FileType = FileType.Pdf,
            UploadedAt = DateTime.UtcNow,
            UserId = user.Id,
            User = user,
            Module = module,
            Activity = activity
        };
}