namespace LMS.Shared.DTOs.DashboardDtos;

/// <summary>
/// One row in the "Latest documents" card on the dashboard (US12).
/// Only metadata: the file is fetched through the download endpoint with the Id,
/// never through the stored file name or a path on disk.
/// </summary>
public sealed record DashboardDocumentDto(
    Guid Id,
    string Name,
    // Pdf, Word, PowerPoint, Image or Other. Used for the small file icon.
    string FileType,
    // Where the document belongs, for example "Course", "Module: Blazor" or "Activity: Exercise 3"
    string Context,
    string UploadedByName,
    // UTC. The page shows it in the browser's local time.
    DateTime UploadedAt);