using FluentValidation;
using OrderService.Api.Features.Carts.Validators;

namespace OrderService.Api.Features.Carts.Commands.RemoveItemFromCart;

public class RemoveItemFromCartCommandValidator : AbstractValidator<RemoveItemFromCartCommand>
{
    public RemoveItemFromCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CartItems).NotNull()
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