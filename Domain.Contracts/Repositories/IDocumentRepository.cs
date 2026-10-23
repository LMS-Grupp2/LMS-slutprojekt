using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

/// <summary>
/// Database access for documents (metadata only, the files live on disk). Nothing is saved at this level:
/// Create/Delete only mark changes, and the service saves once through the unit of work.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Returns one document with its uploader, module and activity loaded (needed to find
    /// which course the document belongs to). Null if it does not exist.
    /// </summary>
    Task<Document?> GetByIdAsync(Guid id);

    /// <summary>
    /// Returns every document in a course: documents attached to the course itself,
    /// to one of its modules, or to one of its activities. Newest first. Read only.
    /// </summary>
    Task<IEnumerable<Document>> GetByCourseIdAsync(Guid courseId);

    /// <summary>Returns the documents attached directly to a module, newest first. Read only.</summary>
    Task<IEnumerable<Document>> GetByModuleIdAsync(Guid moduleId);

    /// <summary>Returns the documents attached directly to an activity, newest first. Read only.</summary>
    Task<IEnumerable<Document>> GetByActivityIdAsync(Guid activityId);

    /// <summary>
    /// Returns the newest documents in a course (US12, the student dashboard), at most "take" of them.
    /// Same rule as GetByCourseIdAsync, but hand-in files are left out (they belong to a submission, not to the
    /// course). Uploader, module and activity are loaded so the dashboard can show where a document belongs.
    /// Read only.
    /// </summary>
    Task<IEnumerable<Document>> GetRecentByCourseIdAsync(Guid courseId, int take);

    void Create(Document document);
    void Delete(Document document);
}