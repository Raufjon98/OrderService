using FluentValidation;
using OrderService.Api.Features.Carts.Validators;

namespace OrderService.Api.Features.Carts.Commands.CreateCart;

public class CreateCartCommandValidator : AbstractValidator<CreateCartCommand>
{
    public CreateCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty()
            .WithMessage("CustomerId is required");
        RuleFor(x => x.CartRequest).NotNull()
            .ChildRules(cart =>
            {
                cart.RuleFor(x => x.Items)
                    .NotEmpty()
                    .WithMessage("Cart must contain at least one item");

                cart.RuleForEach(x => x.Items)
                    .SetValidator(new CartItemRequestValidator());
            });
    }
}