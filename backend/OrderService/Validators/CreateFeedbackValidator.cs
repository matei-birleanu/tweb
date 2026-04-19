using FluentValidation;
using OrderService.DTOs;

namespace OrderService.Validators;

public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackDto>
{
    private static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "PRODUCT_QUALITY", "DELIVERY", "CUSTOMER_SERVICE", "WEBSITE"
    };

    public CreateFeedbackValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(c => ValidCategories.Contains(c)).WithMessage("Invalid feedback category");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required")
            .MinimumLength(10).WithMessage("Comment must be at least 10 characters")
            .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.AgreedToTerms)
            .Equal(true).WithMessage("You must agree to the terms and conditions");
    }
}
