using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ApplicationDbContext _context;
     
    public FeedbackRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Create(Feedback feedback) => _context.Feedbacks.Add(feedback);

    public async Task<IEnumerable<Feedback>> GetBySubmissionIdAsync(Guid submissionId)
    {
        return await _context.Feedbacks
            .AsNoTracking()
            .Where(f => f.SubmissionId == submissionId)
            .Include(f => f.Teacher)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}
