using LMS.Shared.DTOs.DocumentDtos;

namespace Service.Contracts;

/// <summary>
/// Business rules for course documents. The service takes plain values (a Stream, names, ids) and no
/// ASP.NET types, so it can be tested without a web request. Students only get documents from their own course.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// All documents in a course: attached to the course, to one of its modules or to one of its activities.
    /// Throws a not-found error if the course does not exist or a student is not enrolled in it.
    /// </summary>
    Task<IEnumerable<DocumentDto>> GetByCourseAsync(Guid courseId, string userId, bool isTeacher);

    /// <summary>The documents attached directly to a module. Same access rules as the course list.</summary>
    Task<IEnumerable<DocumentDto>> GetByModuleAsync(Guid moduleId, string userId, bool isTeacher);

    /// <summary>The documents attached directly to an activity. Same access rules as the course list.</summary>
    Task<IEnumerable<DocumentDto>> GetByActivityAsync(Guid activityId, string userId, bool isTeacher);

    /// <summary>
    /// Validates the file (type and size), saves it to disk and stores its metadata.
    /// Exactly one of courseId, moduleId, activityId or submissionId must be set.
    /// Course, module and activity uploads are teacher only (checked in the controller).
    /// submissionId is only passed by SubmissionService (US09) for a student's hand-in.
    /// </summary>
    Task<DocumentDto> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        long length,
        Guid? courseId,
        Guid? moduleId,
        Guid? activityId,
        Guid? submissionId,
        string userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Opens a document for download. Throws a not-found error if it does not exist or the
    /// student does not belong to its course. The caller must dispose the returned stream.
    /// </summary>
    Task<DocumentDownload> DownloadAsync(Guid id, string userId, bool isTeacher);

    /// <summary>Deletes the metadata and the file on disk. Teacher only (checked in the controller).</summary>
    Task DeleteAsync(Guid id);
}

/// <summary>
/// A file ready to be sent to the browser: the open stream, its MIME type and the original file name.
/// </summary>
public sealed record DocumentDownload(Stream Content, string ContentType, string FileName);