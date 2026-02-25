using FluentValidation;

namespace OrderService.Api.Features.Orders.Commands.MarkAsReady;

public class MarkAsReadyCommandValidator : AbstractValidator<MarkAsReadyCommand>
{
    public MarkAsReadyCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }   
}