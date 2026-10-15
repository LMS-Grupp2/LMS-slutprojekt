using Domain.Contracts;
using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Services;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DocumentDtos;
using Moq;
using Service.Contracts;

namespace LMS.Tests;

/// <summary>
/// SubmissionService with a fake database (mocked repositories) and a fake DocumentService,
/// so the hand-in rules can be tested without SQL Server or files on disk.
/// </summary>
public class SubmissionServiceTests
{
    private const string StudentId = "student-1";
    private static readonly Guid StudentCourseId = Guid.NewGuid();

    private readonly Mock<IActivityRepository> _activities = new();
    private readonly Mock<IModuleRepository> _modules = new();
    private readonly Mock<ICourseUserRepository> _courseUsers = new();
    private readonly Mock<ISubmissionRepository> _submissions = new();
    private readonly Mock<IDocumentService> _documents = new();
    private readonly SubmissionService _service;

    public SubmissionServiceTests()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Activities).Returns(_activities.Object);
        unitOfWork.Setup(u => u.Modules).Returns(_modules.Object);
        unitOfWork.Setup(u => u.CourseUsers).Returns(_courseUsers.Object);
        unitOfWork.Setup(u => u.Submissions).Returns(_submissions.Object);

        // The student is enrolled in one course
        _courseUsers.Setup(r => r.GetByUserIdAsync(StudentId))
            .ReturnsAsync([new CourseUser { CourseId = StudentCourseId, UserId = StudentId }]);

        // The file upload always succeeds in these tests
        _documents.Setup(d => d.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDto(Guid.NewGuid(), "report.pdf", "application/pdf", 3, "Pdf",
                DateTime.UtcNow, "Student", null, null, null));

        _service = new SubmissionService(unitOfWork.Object, _documents.Object);
    }

    [Fact]
    public async Task FirstHandIn_CreatesSubmission_AndLinksTheFileToIt()
    {
        //ARRANGE
        var activity = GivenActivity();
        Submission? created = null;
        _submissions.Setup(r => r.Create(It.IsAny<Submission>())).Callback<Submission>(s => created = s);
        var before = DateTime.UtcNow;

        //ACT
        var result = await SubmitAsync(activity.Id);

        //ASSERT
        Assert.NotNull(created);
        Assert.Equal(StudentId, created.StudentId);
        Assert.Equal(activity.Id, created.ActivityId);
        Assert.InRange(created.SubmittedAt, before, DateTime.UtcNow);
        Assert.Equal(created.Id, result.SubmissionId);

        // The file goes to the submission only, never to the course, module or activity (privacy)
        _documents.Verify(d => d.UploadAsync(
            It.IsAny<Stream>(), "report.pdf", It.IsAny<string>(), It.IsAny<long>(),
            null, null, null, created.Id, StudentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LaterHandIn_ReusesSubmission_AndRefreshesTime()
    {
        //ARRANGE
        var activity = GivenActivity();
        var existing = new Submission
        {
            Id = Guid.NewGuid(), ActivityId = activity.Id, StudentId = StudentId,
            SubmittedAt = DateTime.UtcNow.AddDays(-2)
        };
        _submissions.Setup(r => r.GetByActivityAndStudentAsync(activity.Id, StudentId)).ReturnsAsync(existing);
        var oldTime = existing.SubmittedAt;

        //ACT
        await SubmitAsync(activity.Id);

        //ASSERT
        _submissions.Verify(r => r.Create(It.IsAny<Submission>()), Times.Never);
        Assert.True(existing.SubmittedAt > oldTime);
    }

    [Fact]
    public async Task LateHandIn_IsAllowed_AndShownAsLate()
    {
        //ARRANGE
        var activity = GivenActivity(deadline: DateTime.UtcNow.AddDays(-1));

        //ACT
        var result = await SubmitAsync(activity.Id);

        //ASSERT
        Assert.True(result.IsLate);
        Assert.Equal(SubmissionStatus.Late, result.Status);
    }

    [Fact]
    public async Task HandIn_AfterFeedback_Is409_AndNothingIsStored()
    {
        //ARRANGE
        var activity = GivenActivity();
        var assessed = new Submission
        {
            Id = Guid.NewGuid(), ActivityId = activity.Id, StudentId = StudentId,
            SubmittedAt = DateTime.UtcNow.AddDays(-2),
            Feedbacks = [new Feedback { Text = "Good work" }]
        };
        _submissions.Setup(r => r.GetByActivityAndStudentAsync(activity.Id, StudentId)).ReturnsAsync(assessed);
        var oldTime = assessed.SubmittedAt;

        //ACT + ASSERT
        await Assert.ThrowsAsync<ConflictException>(() => SubmitAsync(activity.Id));
        AssertNothingStored();
        Assert.Equal(oldTime, assessed.SubmittedAt);
    }

    [Fact]
    public async Task AssignmentInAnotherCourse_Is404_AndNothingIsStored()
    {
        //ARRANGE
        var activity = GivenActivity(courseId: Guid.NewGuid());

        //ACT + ASSERT
        await Assert.ThrowsAsync<ActivityNotFoundException>(() => SubmitAsync(activity.Id));
        AssertNothingStored();
    }

    [Fact]
    public async Task ActivityThatIsNotAnAssignment_Is400_AndNothingIsStored()
    {
        //ARRANGE
        var activity = GivenActivity(typeName: "Lecture");

        //ACT + ASSERT
        await Assert.ThrowsAsync<BadRequestException>(() => SubmitAsync(activity.Id));
        AssertNothingStored();
    }


    /* Helpers */

    // An activity in a module. By default: an assignment in the student's course, deadline tomorrow.
    private Activity GivenActivity(
        string typeName = ActivityTypeNames.Assignment, Guid? courseId = null, DateTime? deadline = null)
    {
        var module = new Module { Id = Guid.NewGuid(), Name = "Module 1", CourseId = courseId ?? StudentCourseId };
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Name = "Assignment 1",
            EndTime = deadline ?? DateTime.UtcNow.AddDays(1),
            ModuleId = module.Id,
            ActivityType = new ActivityType { Name = typeName }
        };

        _activities.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        _modules.Setup(r => r.GetByIdAsync(module.Id, It.IsAny<bool>())).ReturnsAsync(module);
        return activity;
    }

    private Task<LMS.Shared.DTOs.SubmissionDtos.StudentSubmissionDto> SubmitAsync(Guid activityId) =>
        _service.SubmitAsync(activityId, StudentId, new MemoryStream([1, 2, 3]), "report.pdf",
            "application/pdf", 3, CancellationToken.None);

    private void AssertNothingStored()
    {
        _submissions.Verify(r => r.Create(It.IsAny<Submission>()), Times.Never);
        _documents.Verify(d => d.UploadAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(),
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
