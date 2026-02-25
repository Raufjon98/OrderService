using FluentValidation;
using OrderService.Api.Features.Orders.Validators;

namespace OrderService.Api.Features.Orders.Commands.RemoveFromOrder;

public class RemoveFromOrderCommandValidator : AbstractValidator<RemoveFromOrderCommand>
{
    public RemoveFromOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.RemoveFromOrderRequest)
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