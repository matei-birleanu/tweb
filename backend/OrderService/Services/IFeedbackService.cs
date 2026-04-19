using OrderService.DTOs;
using OrderService.Models.Enums;

namespace OrderService.Services;

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackDto>> GetAllFeedbackAsync();
    Task<FeedbackDto?> GetFeedbackByIdAsync(int id);
    Task<IEnumerable<FeedbackDto>> GetFeedbackByUserIdAsync(int userId);
    Task<IEnumerable<FeedbackDto>> GetFeedbackByCategoryAsync(FeedbackCategory category);
    Task<IEnumerable<FeedbackDto>> GetUnresolvedFeedbackAsync();
    Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackDto feedbackDto);
    Task<FeedbackDto> CreateFeedbackFromRequestAsync(int userId, FeedbackCategory category, string comment, int rating);
    Task<FeedbackDto> RespondToFeedbackAsync(int id, string adminResponse);
    Task<bool> DeleteFeedbackAsync(int id);
}
