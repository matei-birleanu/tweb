using AutoMapper;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Models.Enums;
using OrderService.Repositories;

namespace OrderService.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        IFeedbackRepository repository,
        IMapper mapper,
        ILogger<FeedbackService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<FeedbackDto>> GetAllFeedbackAsync()
    {
        var feedbacks = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
    }

    public async Task<FeedbackDto?> GetFeedbackByIdAsync(int id)
    {
        var feedback = await _repository.GetByIdAsync(id);
        return feedback != null ? _mapper.Map<FeedbackDto>(feedback) : null;
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbackByUserIdAsync(int userId)
    {
        var feedbacks = await _repository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbackByCategoryAsync(FeedbackCategory category)
    {
        var feedbacks = await _repository.GetByCategoryAsync(category);
        return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
    }

    public async Task<IEnumerable<FeedbackDto>> GetUnresolvedFeedbackAsync()
    {
        var feedbacks = await _repository.GetUnresolvedAsync();
        return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
    }

    public async Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackDto feedbackDto)
    {
        var feedback = _mapper.Map<Feedback>(feedbackDto);
        var createdFeedback = await _repository.CreateAsync(feedback);

        return _mapper.Map<FeedbackDto>(createdFeedback);
    }

    public async Task<FeedbackDto> CreateFeedbackFromRequestAsync(int userId, FeedbackCategory category, string comment, int rating)
    {
        var feedback = new Feedback
        {
            UserId = userId,
            Category = category,
            Subject = $"{category} Feedback",
            Message = comment,
            Rating = rating,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdFeedback = await _repository.CreateAsync(feedback);
        return _mapper.Map<FeedbackDto>(createdFeedback);
    }

    public async Task<FeedbackDto> RespondToFeedbackAsync(int id, string adminResponse)
    {
        var existingFeedback = await _repository.GetByIdAsync(id);
        if (existingFeedback == null)
        {
            throw new KeyNotFoundException($"Feedback with ID {id} not found");
        }

        existingFeedback.AdminResponse = adminResponse;
        existingFeedback.IsResolved = true;

        var updatedFeedback = await _repository.UpdateAsync(existingFeedback);
        return _mapper.Map<FeedbackDto>(updatedFeedback);
    }

    public async Task<bool> DeleteFeedbackAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}
