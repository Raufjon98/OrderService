using FluentValidation;
using OrderService.Contracts.OrderItem.Requests;

namespace OrderService.Api.Features.Orders.Validators;

public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x=>x.FoodId).NotEmpty()
            .WithMessage("FoodId is required");
        RuleFor(x=>x.Quantity).GreaterThan(0)
            .WithMessage("Order item quantity must be greater than 0");
    }
}