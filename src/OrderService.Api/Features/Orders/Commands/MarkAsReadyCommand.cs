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

namespace OrderService.Api.Features.Orders.Commands;

public record MarkAsReadyCommand (Guid OrderId) : IRequest<OrderResponse>;

public class MarkasReadyCommandHandler : IRequestHandler<MarkAsReadyCommand, OrderResponse>
{
    private readonly OrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public MarkasReadyCommandHandler(OrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task<OrderResponse> Handle(MarkAsReadyCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o=> o.Id == request.OrderId);

        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId.ToString());
        }

        if (order.Status != OrderStatus.Preparing)
        {
            throw new Exception("Only preparing orders can be supported!");
        }
        
        order.Status = OrderStatus.Ready;
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
            Items = order.Items.Select(o => new OrderItemResponse
            {
                OrderId = o.OrderId,
                FoodId = o.FoodId,
                Quantity = o.Quantity,
                UnitPrice = o.UnitPrice,
            }).ToList()
        };
    }
}