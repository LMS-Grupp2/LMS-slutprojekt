using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Tracked on purpose: the service deletes the entity after loading it.
    // Module and Activity.Module are included so the service can work out the document's course.
    // Submission is included so the service can see whose hand-in a file is (US09).
    public async Task<Document?> GetByIdAsync(Guid id)
    {
        return await _context.Documents
            .Include(d => d.User)
            .Include(d => d.Submission)
            .Include(d => d.Module)
            .Include(d => d.Activity)
                .ThenInclude(a => a!.Module)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    // A document belongs to a course directly, or through its module, or through its activity's module.
    // Read only (a list), so tracking is turned off. Include(User) is needed for the uploader's name.
    public async Task<IEnumerable<Document>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.CourseId == courseId
                        || (d.Module != null && d.Module.CourseId == courseId)
                        || (d.Activity != null && d.Activity.Module.CourseId == courseId))
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByModuleIdAsync(Guid moduleId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.ModuleId == moduleId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByActivityIdAsync(Guid activityId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.ActivityId == activityId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    // The newest documents in a course (US12, the student dashboard). Same rule as GetByCourseIdAsync, and
    // hand-in files are left out on purpose: they belong to a submission, not to the course.
    // User (uploader's name), Module and Activity (where the document belongs) are included, so one query is
    // enough. Read only, so tracking is turned off. Only "take" rows are fetched.
    public async Task<IEnumerable<Document>> GetRecentByCourseIdAsync(Guid courseId, int take)
    {
        return await _context.Documents
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Module)
            .Include(d => d.Activity)
            .Where(d => d.SubmissionId == null
                        && (d.CourseId == courseId
                            || (d.Module != null && d.Module.CourseId == courseId)
                            || (d.Activity != null && d.Activity.Module.CourseId == courseId)))
            .OrderByDescending(d => d.UploadedAt)
            .Take(take)
            .ToListAsync();
    }

    public void Create(Document document) => _context.Documents.Add(document);
    public void Delete(Document document) => _context.Documents.Remove(document);
}