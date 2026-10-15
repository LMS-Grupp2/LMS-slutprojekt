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
    // True when the signed-in user may delete this document (US07): a teacher always,
    // a student only their own. The UI uses it to show the delete button.
    bool CanDelete,
    // True when a student uploaded the document (US07). The UI uses it to put the document
    // under "Shared by students" instead of "Course material".
    // Both of these are added last so existing usages keep their order.
    bool UploadedByStudent);