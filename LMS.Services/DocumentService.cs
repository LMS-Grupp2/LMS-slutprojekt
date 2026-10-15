using Domain.Contracts;
using Domain.Models.Configurations;
using Domain.Models.Entities;
using Domain.Models.Enums;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.Extensions.Options;
using Service.Contracts;

namespace LMS.Services;

/// <summary>
/// Business rules for course documents: only allowed file types and sizes (400), a document belongs to exactly
/// one of course, module or activity (400), the target must exist (404) and students only see documents from
/// their own course (404 for anything else). The file is stored on disk, the database only keeps metadata.
/// </summary>
public class DocumentService : IDocumentService
{
    private const int MaxFileNameLength = 255;

    // Allowed extensions. The MIME type is taken from this list, never from the client,
    // so someone cannot upload a file and label it as text/html.
    private static readonly Dictionary<string, (FileType FileType, string ContentType)> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = (FileType.Pdf, "application/pdf"),
            [".doc"] = (FileType.Word, "application/msword"),
            [".docx"] = (FileType.Word, "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
            [".ppt"] = (FileType.PowerPoint, "application/vnd.ms-powerpoint"),
            [".pptx"] = (FileType.PowerPoint, "application/vnd.openxmlformats-officedocument.presentationml.presentation"),
            [".xls"] = (FileType.Other, "application/vnd.ms-excel"),
            [".xlsx"] = (FileType.Other, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            [".txt"] = (FileType.Other, "text/plain"),
            [".png"] = (FileType.Image, "image/png"),
            [".jpg"] = (FileType.Image, "image/jpeg"),
            [".jpeg"] = (FileType.Image, "image/jpeg"),
            [".gif"] = (FileType.Image, "image/gif")
        };

    private readonly IUnitOfWork _unitOfWork;
    private readonly string _uploadsRoot;
    private readonly int _maxFileSizeMb;

    public DocumentService(IUnitOfWork unitOfWork, IOptions<DocumentStorageSettings> settings)
    {
        _unitOfWork = unitOfWork;
        _maxFileSizeMb = settings.Value.MaxFileSizeMb;

        // A relative path is resolved from the folder the API runs in. Use an absolute path in production.
        _uploadsRoot = Path.GetFullPath(settings.Value.UploadsPath);
    }

    public async Task<IEnumerable<DocumentDto>> GetByCourseAsync(Guid courseId, string userId, bool isTeacher)
    {
        // Unknown course and "not your course" give the same 404 so a student can't tell them apart
        if (await _unitOfWork.Courses.GetCourseByIdAsync(courseId) is null
            || !await CanAccessCourseAsync(courseId, userId, isTeacher))
            throw new CourseNotFoundException(courseId);

        var documents = await _unitOfWork.Documents.GetByCourseIdAsync(courseId);
        return documents.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<DocumentDto>> GetByModuleAsync(Guid moduleId, string userId, bool isTeacher)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(moduleId)
            ?? throw new ModuleNotFoundException(moduleId);

        if (!await CanAccessCourseAsync(module.CourseId, userId, isTeacher))
            throw new ModuleNotFoundException(moduleId);

        var documents = await _unitOfWork.Documents.GetByModuleIdAsync(moduleId);
        return documents.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<DocumentDto>> GetByActivityAsync(Guid activityId, string userId, bool isTeacher)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(activityId)
            ?? throw new ActivityNotFoundException(activityId);

        // The activity knows its module, and the module knows the course
        var module = await _unitOfWork.Modules.GetByIdAsync(activity.ModuleId)
            ?? throw new ActivityNotFoundException(activityId);

        if (!await CanAccessCourseAsync(module.CourseId, userId, isTeacher))
            throw new ActivityNotFoundException(activityId);

        var documents = await _unitOfWork.Documents.GetByActivityIdAsync(activityId);
        return documents.Select(MapToDto).ToList();
    }

    public async Task<DocumentDto> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        long length,
        Guid? courseId,
        Guid? moduleId,
        Guid? activityId,
        Guid? submissionId,
        string userId,
        CancellationToken cancellationToken)
    {
        var maxBytes = _maxFileSizeMb * 1024L * 1024L;

        if (length <= 0)
            throw new BadRequestException("The file is empty.");

        if (length > maxBytes)
            throw new BadRequestException($"The file is too large. The maximum size is {_maxFileSizeMb} MB.");

        if (new[] { courseId, moduleId, activityId, submissionId }.Count(id => id.HasValue) != 1)
            throw new BadRequestException("Attach the document to exactly one of: a course, a module or an activity.");

        // Only the file name, never a path the client sent along
        var originalName = Path.GetFileName(fileName ?? string.Empty).Trim();

        if (originalName.Length == 0)
            throw new BadRequestException("The file needs a name.");

        if (originalName.Length > MaxFileNameLength)
            throw new BadRequestException($"The file name can be at most {MaxFileNameLength} characters.");

        var extension = Path.GetExtension(originalName);

        if (!AllowedTypes.TryGetValue(extension, out var type))
            throw new BadRequestException(
                $"The file type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedTypes.Keys)}.");

        // The target must exist
        if (courseId is { } cid && await _unitOfWork.Courses.GetCourseByIdAsync(cid) is null)
            throw new CourseNotFoundException(cid);

        if (moduleId is { } mid && await _unitOfWork.Modules.GetByIdAsync(mid) is null)
            throw new ModuleNotFoundException(mid);

        if (activityId is { } aid && await _unitOfWork.Activities.GetByIdAsync(aid) is null)
            throw new ActivityNotFoundException(aid);

        // No existence check for submissionId: only SubmissionService passes it, and the submission may be
        // new and not saved yet. It shares this unit of work, so the SaveChangesAsync below saves both.

        // The file on disk gets a generated name, so uploads with the same name never overwrite each other
        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(_uploadsRoot, storedFileName);

        Directory.CreateDirectory(_uploadsRoot);

        try
        {
            await using (var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await content.CopyToAsync(target, cancellationToken);
            }

            // Double check the real size, the reported length comes from the client
            if (new FileInfo(path).Length > maxBytes)
                throw new BadRequestException($"The file is too large. The maximum size is {_maxFileSizeMb} MB.");

            var document = new Document
            {
                Name = originalName,
                StoredFileName = storedFileName,
                ContentType = type.ContentType,
                SizeBytes = new FileInfo(path).Length,
                FileType = type.FileType,
                UploadedAt = DateTime.UtcNow,
                UserId = userId,
                CourseId = courseId,
                ModuleId = moduleId,
                ActivityId = activityId,
                SubmissionId = submissionId
            };

            _unitOfWork.Documents.Create(document);
            await _unitOfWork.SaveChangesAsync();

            var uploader = await _unitOfWork.Users.FindByIdAsync(userId);
            return ToDto(document, uploader?.Name ?? "Unknown");
        }
        catch
        {
            // Never leave a file on disk without a matching database row
            DeleteFileQuietly(path);
            throw;
        }
    }

    public async Task<DocumentDownload> DownloadAsync(Guid id, string userId, bool isTeacher)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id)
            ?? throw new DocumentNotFoundException(id);

        // A student only gets documents from their own course, and of hand-ins only their own (US09).
        // Everything else is a 404, so a student cannot find out which files exist.
        if (!isTeacher)
        {
            if (document.Submission is not null)
            {
                if (document.Submission.StudentId != userId)
                    throw new DocumentNotFoundException(id);
            }
            else
            {
                var courseId = GetCourseId(document);

                if (courseId is null || !await CanAccessCourseAsync(courseId.Value, userId, isTeacher))
                    throw new DocumentNotFoundException(id);
            }
        }

        var path = Path.Combine(_uploadsRoot, document.StoredFileName);

        // The row exists but the file is gone (for example a wiped uploads folder)
        if (!File.Exists(path))
            throw new DocumentNotFoundException(id);

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return new DocumentDownload(stream, document.ContentType, document.Name);
    }

    public async Task DeleteAsync(Guid id)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id)
            ?? throw new DocumentNotFoundException(id);

        _unitOfWork.Documents.Delete(document);
        await _unitOfWork.SaveChangesAsync();

        // The row is gone, now remove the file. A leftover file is harmless, so errors are ignored.
        DeleteFileQuietly(Path.Combine(_uploadsRoot, document.StoredFileName));
    }


    /* Helpers */

    // Teachers see every course. A student only sees the course they are enrolled in.
    private async Task<bool> CanAccessCourseAsync(Guid courseId, string userId, bool isTeacher)
    {
        if (isTeacher)
            return true;

        var enrollments = await _unitOfWork.CourseUsers.GetByUserIdAsync(userId);
        return enrollments.Any(cu => cu.CourseId == courseId);
    }

    // A document belongs to a course directly, or through its module, or through its activity's module.
    // Needs Module and Activity.Module to be loaded (Include in the repository).
    private static Guid? GetCourseId(Document document) =>
        document.CourseId
        ?? document.Module?.CourseId
        ?? document.Activity?.Module?.CourseId;

    // Needs User to be loaded (Include in the repository) for the uploader's name.
    private static DocumentDto MapToDto(Document document) => ToDto(document, document.User.Name);

    private static DocumentDto ToDto(Document document, string uploadedByName) =>
        new(
            document.Id,
            document.Name,
            document.ContentType,
            document.SizeBytes,
            document.FileType.ToString(),
            document.UploadedAt,
            uploadedByName,
            document.CourseId,
            document.ModuleId,
            document.ActivityId);

    private static void DeleteFileQuietly(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
            // Ignored on purpose, see the callers
        }
        catch (UnauthorizedAccessException)
        {
            // Ignored on purpose, see the callers
        }
    }
}