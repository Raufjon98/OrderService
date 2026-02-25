using FluentValidation;

namespace OrderService.Api.Features.Orders.Commands.CompleteOrder;

public class CompleteOrderCommandValidator : AbstractValidator<CompleteOrderCommand>
{
    public CompleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}