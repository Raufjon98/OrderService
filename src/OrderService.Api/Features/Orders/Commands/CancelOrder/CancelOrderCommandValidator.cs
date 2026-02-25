using FluentValidation;

namespace OrderService.Api.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x=>x.CustomerId).NotEmpty();
    }   
}