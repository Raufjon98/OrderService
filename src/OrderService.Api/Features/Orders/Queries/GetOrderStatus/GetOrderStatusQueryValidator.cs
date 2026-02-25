using FluentValidation;

namespace OrderService.Api.Features.Orders.Queries.GetOrderStatus;

public class GetOrderStatusQueryValidator : AbstractValidator<GetOrderStatusQuery>
{
    public GetOrderStatusQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
    }
}