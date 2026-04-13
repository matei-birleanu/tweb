using OrderService.Models;
using OrderService.Models.Enums;

namespace OrderService.Repositories;

public interface IFeedbackRepository
{
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<Feedback?> GetByIdAsync(int id);
    Task<IEnumerable<Feedback>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Feedback>> GetByCategoryAsync(FeedbackCategory category);
    Task<IEnumerable<Feedback>> GetUnresolvedAsync();
    Task<Feedback> CreateAsync(Feedback feedback);
    Task<Feedback> UpdateAsync(Feedback feedback);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
