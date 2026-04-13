using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Models.Enums;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
    {
        _feedbackService = feedbackService;
        _logger = logger;
    }

    /// <summary>
    /// Get all feedback
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<FeedbackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetAllFeedback()
    {
        var feedbacks = await _feedbackService.GetAllFeedbackAsync();
        return Ok(feedbacks);
    }

    /// <summary>
    /// Get feedback by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackDto>> GetFeedbackById(int id)
    {
        var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
        if (feedback == null)
        {
            return NotFound(new { message = $"Feedback with ID {id} not found" });
        }
        return Ok(feedback);
    }

    /// <summary>
    /// Get feedback by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<FeedbackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbackByUserId(int userId)
    {
        var feedbacks = await _feedbackService.GetFeedbackByUserIdAsync(userId);
        return Ok(feedbacks);
    }

    /// <summary>
    /// Get feedback by category
    /// </summary>
    [HttpGet("category/{category}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<FeedbackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbackByCategory(FeedbackCategory category)
    {
        var feedbacks = await _feedbackService.GetFeedbackByCategoryAsync(category);
        return Ok(feedbacks);
    }

    /// <summary>
    /// Get unresolved feedback
    /// </summary>
    [HttpGet("unresolved")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<FeedbackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetUnresolvedFeedback()
    {
        var feedbacks = await _feedbackService.GetUnresolvedFeedbackAsync();
        return Ok(feedbacks);
    }

    /// <summary>
    /// Create feedback
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeedbackDto>> CreateFeedback([FromBody] CreateFeedbackDto feedbackDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var feedback = await _feedbackService.CreateFeedbackAsync(feedbackDto);
        return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.Id }, feedback);
    }

    /// <summary>
    /// Respond to feedback
    /// </summary>
    [HttpPost("{id}/respond")]
    [Authorize]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackDto>> RespondToFeedback(int id, [FromBody] RespondToFeedbackDto responseDto)
    {
        try
        {
            var feedback = await _feedbackService.RespondToFeedbackAsync(id, responseDto.AdminResponse);
            return Ok(feedback);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Feedback with ID {id} not found" });
        }
    }

    /// <summary>
    /// Delete feedback
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFeedback(int id)
    {
        var result = await _feedbackService.DeleteFeedbackAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Feedback with ID {id} not found" });
        }
        return NoContent();
    }
}

public class RespondToFeedbackDto
{
    public string AdminResponse { get; set; } = string.Empty;
}
