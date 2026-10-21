using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly ApplicationDbContext _context;

    public SubmissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Submission?> GetByIdAsync(Guid submissionId)
    {
        return await _context.Submissions
            .FirstOrDefaultAsync(s => s.Id == submissionId);
    }

    public async Task<Submission?> GetByActivityAndStudentAsync(Guid activityId, string studentId)
    {
        return await _context.Submissions
            .Include(s => s.Documents.OrderByDescending(d => d.UploadedAt))
            .Include(s => s.Feedbacks)
            .FirstOrDefaultAsync(s => s.ActivityId == activityId && s.StudentId == studentId);
    }

    public void Create(Submission submission) => _context.Submissions.Add(submission);

    public async Task<IEnumerable<Submission>> GetByActivityIdAsync(Guid activityId)
    {
        return await _context.Submissions
            .AsNoTracking()
            .Include(s => s.Documents.OrderByDescending(d => d.UploadedAt))
            .Include(s => s.Feedbacks)
            .Where(s => s.ActivityId == activityId)
            .ToListAsync();
    }
}