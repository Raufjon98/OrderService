using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Order.Events;
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Responses;

namespace OrderService.Api.Features.Orders.Commands.CompleteOrder;

public record CompleteOrderCommand(Guid OrderId) : IRequest <OrderResponse>;

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, OrderResponse>
{
    private readonly OrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CompleteOrderCommandHandler(OrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task<OrderResponse> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o=>o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId.ToString());
        }

        if (order.Status != OrderStatus.Ready)
        {
            throw new Exception("Only Ready orders are supported!");
        }
        
        order.Status = OrderStatus.Completed;
        order.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        
        await _publishEndpoint.Publish(
            new OrderStatusChangedEvent()
            {
                Id = order.Id,
                Status = order.Status,
                OrderStatusChangedOnUtc = DateTime.UtcNow
            },
            cancellationToken);
        
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            OrderDate = order.CreatedAt,
            Items = order.Items.Select(o=>  new OrderItemResponse
            {
                OrderId = o.OrderId,
                FoodId = o.FoodId,
                Quantity = o.Quantity,
                UnitPrice = o.UnitPrice,
            }).ToList()
        };
    }
}