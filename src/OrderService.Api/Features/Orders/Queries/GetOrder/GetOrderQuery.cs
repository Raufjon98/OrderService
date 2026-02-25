using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Responses;

namespace OrderService.Api.Features.Orders.Queries.GetOrder;

public record GetOrderQuery(Guid CustomerId, Guid OrderId) : IRequest<OrderResponse>;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderResponse>
{
    private readonly OrderDbContext _context;

    public GetOrderQueryHandler(OrderDbContext context)
    {
        _context = context;
    }
    public async Task<OrderResponse> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Where(o=> o.Id == request.OrderId && o.CustomerId == request.CustomerId)
            .Select( order=> new  OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                OrderDate = order.CreatedAt,
                Items = order.Items.Select(o => new OrderItemResponse
                {
                    OrderId = o.OrderId,
                    FoodId = o.FoodId,
                    Quantity = o.Quantity,
                    UnitPrice = o.UnitPrice,
                }).ToList()
            }).FirstOrDefaultAsync(cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId.ToString());    
        }
        
        return order;
    }
}