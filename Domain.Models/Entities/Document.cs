using Domain.Models.Enums;

namespace Domain.Models.Entities;

/// <summary>
/// Metadata for an uploaded course document. The file itself is stored on disk
/// (see StoredFileName), never in the database.
/// A document belongs to a course, a module or an activity (set one of the three).
/// </summary>
public class Document
{
    public Guid Id { get; set; }

    // Original file name as uploaded by the user, shown in the UI and used when downloading
    public string Name { get; set; } = null!;

    // Name of the file on disk. A generated name, so two uploads with the same
    // original name never collide and the user can't choose a path.
    public string StoredFileName { get; set; } = null!;

    // MIME type, for example "application/pdf". Sent back as Content-Type on download.
    public string ContentType { get; set; } = null!;

    // File size in bytes
    public long SizeBytes { get; set; }

    public FileType FileType { get; set; }
    public DateTime UploadedAt { get; set; }

    // The user who uploaded the document
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }

    public Guid? ModuleId { get; set; }
    public Module? Module { get; set; }

    public Guid? ActivityId { get; set; }
    public Activity? Activity { get; set; }

    public Guid? SubmissionId { get; set; }
    public Submission? Submission { get; set; }
}