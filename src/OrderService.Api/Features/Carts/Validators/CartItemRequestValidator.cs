using FluentValidation;
using OrderService.Contracts.CartItem.Requests;

namespace OrderService.Api.Features.Carts.Validators;

public class CartItemRequestValidator : AbstractValidator<CartItemRequest>
{
    public CartItemRequestValidator()
    {
        RuleFor(x=>x.FoodId).NotEmpty()
            .WithMessage("Cart item id must not be empty");
        RuleFor(x => x.Quantity).GreaterThan(0)
            .WithMessage("Cart item quantity must be greater than 0");
    }
}