using FluentValidation;

namespace OrderService.Api.Features.Orders.Commands.StartPreparation;

public class StartPreparationCommandValidator : AbstractValidator<StartPreparationCommand>
{
    public StartPreparationCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}