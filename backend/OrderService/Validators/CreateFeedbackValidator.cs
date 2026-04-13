using FluentValidation;
using OrderService.DTOs;

namespace OrderService.Validators;

public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackDto>
{
    public CreateFeedbackValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid feedback category");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message cannot exceed 2000 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).When(x => x.Rating.HasValue)
            .WithMessage("Rating must be between 1 and 5");
    }
}
