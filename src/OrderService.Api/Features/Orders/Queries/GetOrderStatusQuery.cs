using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Enums;

namespace OrderService.Api.Features.Orders.Queries;

public record GetOrderStatusQuery(Guid CustomerId,Guid OrderId) : IRequest<OrderStatus>;

public class GetOrderStatusQueryHandler : IRequestHandler<GetOrderStatusQuery, OrderStatus>
{
    private readonly OrderDbContext _context;

    public GetOrderStatusQueryHandler(OrderDbContext context)
    {
        _context = context;
    }
    
    public async Task<OrderStatus> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
    {
        var orderStatus = await _context.Orders
            .Where(o => o.Id == request.OrderId && o.CustomerId == request.CustomerId)
            .Select(o=> o.Status)
            .FirstOrDefaultAsync(cancellationToken);
        return orderStatus;
    }
}