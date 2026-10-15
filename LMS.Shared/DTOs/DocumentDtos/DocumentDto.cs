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
    Guid? ActivityId);