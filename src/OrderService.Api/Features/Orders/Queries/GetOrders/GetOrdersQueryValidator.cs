using FluentValidation;

namespace OrderService.Api.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}