using Domain.Contracts;
using Domain.Contracts.Repositories;
using Domain.Models.Configurations;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace LMS.Tests;

/// <summary>
/// Who may upload and delete documents (US07): a student only to their own course or module and only
/// their own documents, a teacher anywhere. No files are written, only the rules are tested.
/// </summary>
public class DocumentServiceStudentTests
{
    private const string StudentId = "student-1";
    private const string ClassmateId = "student-2";
    private const string TeacherId = "teacher-1";

    private readonly Guid _ownCourseId = Guid.NewGuid();
    private readonly Guid _otherCourseId = Guid.NewGuid();
    private readonly Guid _ownModuleId = Guid.NewGuid();
    private readonly Guid _otherModuleId = Guid.NewGuid();

    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly DocumentService _service;

    public DocumentServiceStudentTests()
    {
        // The student is enrolled in one course only
        var courseUsers = new Mock<ICourseUserRepository>();
        courseUsers
            .Setup(r => r.GetByUserIdAsync(StudentId))
            .ReturnsAsync(new List<CourseUser> { new() { CourseId = _ownCourseId, UserId = StudentId } });

        // One module in the student's course and one in another course
        var modules = new Mock<IModuleRepository>();
        modules
            .Setup(r => r.GetByIdAsync(_ownModuleId, It.IsAny<bool>()))
            .ReturnsAsync(new Module { Id = _ownModuleId, CourseId = _ownCourseId });
        modules
            .Setup(r => r.GetByIdAsync(_otherModuleId, It.IsAny<bool>()))
            .ReturnsAsync(new Module { Id = _otherModuleId, CourseId = _otherCourseId });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.CourseUsers).Returns(courseUsers.Object);
        unitOfWork.Setup(u => u.Modules).Returns(modules.Object);
        unitOfWork.Setup(u => u.Documents).Returns(_documents.Object);

        var settings = Options.Create(new DocumentStorageSettings
        {
            UploadsPath = Path.Combine(Path.GetTempPath(), $"lms-tests-{Guid.NewGuid():N}")
        });

        _service = new DocumentService(unitOfWork.Object, settings);
    }

    /* Upload rules */

    [Fact]
    public async Task Student_CanUpload_ToOwnCourse()
    {
        //ACT + ASSERT: no exception
        await _service.EnsureCanUploadAsync(_ownCourseId, null, null, StudentId, isTeacher: false);
    }

    [Fact]
    public async Task Student_CanUpload_ToModuleInOwnCourse()
    {
        //ACT + ASSERT: no exception
        await _service.EnsureCanUploadAsync(null, _ownModuleId, null, StudentId, isTeacher: false);
    }

    [Fact]
    public async Task Student_CannotUpload_ToOtherCourse()
    {
        //ACT + ASSERT: 404, so the student cannot tell the course exists
        await Assert.ThrowsAsync<CourseNotFoundException>(
            () => _service.EnsureCanUploadAsync(_otherCourseId, null, null, StudentId, isTeacher: false));
    }

    [Fact]
    public async Task Student_CannotUpload_ToModuleInOtherCourse()
    {
        //ACT + ASSERT
        await Assert.ThrowsAsync<ModuleNotFoundException>(
            () => _service.EnsureCanUploadAsync(null, _otherModuleId, null, StudentId, isTeacher: false));
    }

    [Fact]
    public async Task Student_CannotUpload_ToActivity()
    {
        //ACT + ASSERT: students cannot attach documents to activities at all
        await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => _service.EnsureCanUploadAsync(null, null, Guid.NewGuid(), StudentId, isTeacher: false));
    }

    [Fact]
    public async Task Teacher_CanUpload_ToAnyTarget()
    {
        //ACT + ASSERT: no exception, not even for an activity or a course the teacher is not enrolled in
        await _service.EnsureCanUploadAsync(_otherCourseId, null, null, TeacherId, isTeacher: true);
        await _service.EnsureCanUploadAsync(null, null, Guid.NewGuid(), TeacherId, isTeacher: true);
    }

    /* Delete rules */

    [Fact]
    public async Task Student_CanDelete_OwnDocument()
    {
        //ARRANGE
        var document = CreateDocument(StudentId);

        //ACT
        await _service.DeleteAsync(document.Id, StudentId, isTeacher: false);

        //ASSERT
        _documents.Verify(r => r.Delete(document), Times.Once);
    }

    [Fact]
    public async Task Student_CannotDelete_ClassmatesDocument()
    {
        //ARRANGE
        var document = CreateDocument(ClassmateId);

        //ACT + ASSERT: 404, the same answer as for a document that does not exist
        await Assert.ThrowsAsync<DocumentNotFoundException>(
            () => _service.DeleteAsync(document.Id, StudentId, isTeacher: false));

        _documents.Verify(r => r.Delete(It.IsAny<Document>()), Times.Never);
    }

    [Fact]
    public async Task Student_CannotDelete_OwnHandInFile()
    {
        //ARRANGE: a file the student handed in (US09). It belongs to a submission, not to a course.
        var handIn = CreateDocument(StudentId, submissionId: Guid.NewGuid());

        //ACT + ASSERT: 404, otherwise a student could delete a hand-in that is locked after feedback
        await Assert.ThrowsAsync<DocumentNotFoundException>(
            () => _service.DeleteAsync(handIn.Id, StudentId, isTeacher: false));

        _documents.Verify(r => r.Delete(It.IsAny<Document>()), Times.Never);
    }

    [Fact]
    public async Task Teacher_CanDelete_AnyDocument()
    {
        //ARRANGE
        var document = CreateDocument(StudentId);

        //ACT
        await _service.DeleteAsync(document.Id, TeacherId, isTeacher: true);

        //ASSERT
        _documents.Verify(r => r.Delete(document), Times.Once);
    }

    /* Helpers */

    // A document uploaded by the given user, returned by the mocked repository.
    // Give a submissionId to make it a hand-in file instead of a course document.
    private Document CreateDocument(string uploadedBy, Guid? submissionId = null)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Name = "notes.pdf",
            StoredFileName = $"{Guid.NewGuid():N}.pdf",
            ContentType = "application/pdf",
            UserId = uploadedBy,
            CourseId = submissionId is null ? _ownCourseId : null,
            SubmissionId = submissionId
        };

        _documents.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(document);
        return document;
    }
}