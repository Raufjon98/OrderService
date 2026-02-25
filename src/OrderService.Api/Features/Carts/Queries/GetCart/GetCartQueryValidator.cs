using FluentValidation;
using OrderService.Api.Features.Carts.Queries.GetCart;

namespace OrderService.Api.Features.Carts.Queries;

public class GetCartQueryValidator : AbstractValidator<GetCartQuery>
{
    public GetCartQueryValidator()
    {
        RuleFor(x=>x.CustomerId).NotEmpty();
    }
}