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
/// Who may download a handed-in file (US09). Uses a temporary folder instead of the real Uploads folder.
/// </summary>
public class DocumentServiceDownloadTests : IDisposable
{
    private const string OwnerId = "student-owner";
    private const string ClassmateId = "student-classmate";

    private readonly string _uploadsFolder = Path.Combine(Path.GetTempPath(), $"lms-tests-{Guid.NewGuid():N}");
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly DocumentService _service;
    private readonly Document _handIn;

    // TODO - restore DocumentTest
    public DocumentServiceDownloadTests()
    {
        Directory.CreateDirectory(_uploadsFolder);

        // A file handed in by OwnerId, with its file on disk
        _handIn = new Document
        {
            Id = Guid.NewGuid(),
            Name = "report.pdf",
            StoredFileName = "stored.pdf",
            ContentType = "application/pdf",
            UserId = OwnerId,
            Submission = new Submission { StudentId = OwnerId }
        };
        File.WriteAllText(Path.Combine(_uploadsFolder, _handIn.StoredFileName), "content");
        _documents.Setup(r => r.GetByIdAsync(_handIn.Id)).ReturnsAsync(_handIn);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Documents).Returns(_documents.Object);

        var settings = Options.Create(new DocumentStorageSettings { UploadsPath = _uploadsFolder });
        _service = new DocumentService(unitOfWork.Object, settings);
    }

    [Fact]
    public async Task Student_CanDownload_OwnHandIn()
    {
        //ACT
        var download = await _service.DownloadAsync(_handIn.Id, OwnerId, isTeacher: false);
        await download.Content.DisposeAsync();

        //ASSERT
        Assert.Equal("report.pdf", download.FileName);
    }

    [Fact]
    public async Task Student_CannotDownload_ClassmatesHandIn()
    {
        //ACT + ASSERT: 404, the same answer as for a file that does not exist
        await Assert.ThrowsAsync<DocumentNotFoundException>(
            () => _service.DownloadAsync(_handIn.Id, ClassmateId, isTeacher: false));
    }

    [Fact]
    public async Task Teacher_CanDownload_AnyHandIn()
    {
        //ACT
        var download = await _service.DownloadAsync(_handIn.Id, "teacher-1", isTeacher: true);
        await download.Content.DisposeAsync();

        //ASSERT
        Assert.Equal("report.pdf", download.FileName);
    }

    // Runs after each test: remove the temporary folder
    public void Dispose() => Directory.Delete(_uploadsFolder, recursive: true);
}
