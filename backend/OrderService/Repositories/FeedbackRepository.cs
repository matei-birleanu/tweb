using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Models.Enums;

namespace OrderService.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FeedbackRepository> _logger;

    public FeedbackRepository(ApplicationDbContext context, ILogger<FeedbackRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
        return await _context.Feedbacks
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Feedback?> GetByIdAsync(int id)
    {
        return await _context.Feedbacks
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Feedback>> GetByUserIdAsync(int userId)
    {
        return await _context.Feedbacks
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetByCategoryAsync(FeedbackCategory category)
    {
        return await _context.Feedbacks
            .Include(f => f.User)
            .Where(f => f.Category == category)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetUnresolvedAsync()
    {
        return await _context.Feedbacks
            .Include(f => f.User)
            .Where(f => !f.IsResolved)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Feedback> CreateAsync(Feedback feedback)
    {
        feedback.CreatedAt = DateTime.UtcNow;
        feedback.UpdatedAt = DateTime.UtcNow;

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Feedback created with ID: {FeedbackId}", feedback.Id);
        return feedback;
    }

    public async Task<Feedback> UpdateAsync(Feedback feedback)
    {
        feedback.UpdatedAt = DateTime.UtcNow;

        _context.Entry(feedback).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Feedback updated with ID: {FeedbackId}", feedback.Id);
        return feedback;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null)
        {
            return false;
        }

        _context.Feedbacks.Remove(feedback);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Feedback deleted with ID: {FeedbackId}", id);
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Feedbacks.AnyAsync(f => f.Id == id);
    }
}
