using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Order.Events;
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Responses;

namespace OrderService.Api.Features.Orders.Commands;

public record StartPreparationCommand(Guid OrderId) : IRequest<OrderResponse>;

public class StartPreparationCommandHandler : IRequestHandler<StartPreparationCommand, OrderResponse>
{
    private readonly OrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public StartPreparationCommandHandler(OrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task<OrderResponse> Handle(StartPreparationCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o=>o.Id == request.OrderId, cancellationToken);
        ArgumentNullException.ThrowIfNull(order);
        
        if (order.Status != OrderStatus.Confirmed)
        {
            throw new Exception("Only confirmed orders can start preparation!");
        }
        
        order.Status = OrderStatus.Preparing;
        order.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new OrderStatusChangedEvent
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