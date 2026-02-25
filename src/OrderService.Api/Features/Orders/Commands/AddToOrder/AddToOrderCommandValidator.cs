using FluentValidation;
using OrderService.Api.Features.Orders.Validators;

namespace OrderService.Api.Features.Orders.Commands.AddToOrder;

public class AddToOrderCommandValidator : AbstractValidator<AddToOrderCommand>
{
    public AddToOrderCommandValidator()
    {
        RuleFor(x=>x.CustomerId).NotEmpty();
        RuleFor(x => x.AddToOrderRequest)
            .NotNull()
            .ChildRules(order =>
            {   
                order.RuleFor(x => x.OrderId).NotEmpty();
                order.RuleFor(x => x.Items)
                    .NotEmpty()
                    .WithMessage("Adding items are required");

                order.RuleForEach(x => x.Items)
                    .SetValidator(new OrderItemRequestValidator());
            });
    }
}