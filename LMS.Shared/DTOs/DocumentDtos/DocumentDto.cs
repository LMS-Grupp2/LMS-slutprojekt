namespace LMS.Shared.DTOs.DocumentDtos;

/// <summary>
/// Output for the document endpoints (list, upload result). Only metadata leaves the API:
/// never the stored file name or the path on disk. The file itself is fetched through the download endpoint.
/// </summary>
public sealed record DocumentDto(
    Guid Id,
    string Name,
    string ContentType,
    long SizeBytes,
    string FileType,
    DateTime UploadedAt,
    string UploadedByName,
    Guid? CourseId,
    Guid? ModuleId,
    Guid? ActivityId,
    // The id of the user who uploaded the file (US07). The UI uses it to show the delete
    // button only on the student's own documents. Added last so existing usages keep their order.
    string UploadedById);