using FluentValidation;
using OrderService.DTOs;
using OrderService.Models.Enums;

namespace OrderService.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1");

        RuleFor(x => x.TotalPrice)
            .GreaterThan(0).WithMessage("Total price must be greater than 0");

        RuleFor(x => x.OrderType)
            .IsInEnum().WithMessage("Invalid order type");

        RuleFor(x => x.ShippingAddress)
            .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters");

        RuleFor(x => x.RentalStartDate)
            .NotNull().When(x => x.OrderType == OrderType.Rental)
            .WithMessage("Rental start date is required for rental orders");

        RuleFor(x => x.RentalEndDate)
            .NotNull().When(x => x.OrderType == OrderType.Rental)
            .WithMessage("Rental end date is required for rental orders")
            .GreaterThan(x => x.RentalStartDate).When(x => x.RentalStartDate.HasValue)
            .WithMessage("Rental end date must be after start date");
    }
}
