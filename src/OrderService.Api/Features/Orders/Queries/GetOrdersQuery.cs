using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Responses;

namespace OrderService.Api.Features.Orders.Queries;

public record GetOrdersQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 10,
    OrderStatus? OrderStatus = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<List<OrderResponse>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderResponse>>
{
    private readonly OrderDbContext _context;

    public GetOrdersQueryHandler(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = _context.Orders
            .AsNoTracking()
            .Include(o=> o.Items)
            .Where(o => o.CustomerId == request.CustomerId && o.IsDeleted == false);

        if (request.OrderStatus is not null)
        {
            query = query.Where(o => o.Status == request.OrderStatus.Value);
        }
        
        if (request.DateFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            var endOfDay = request.DateTo.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt < endOfDay);
        }
        
        query = query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page -1) * request.PageSize)
            .Take(request.PageSize);
        
        var orders = await query
            .Select(order => new OrderResponse
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
            }).ToListAsync(cancellationToken);
        
        return orders;
    }
}